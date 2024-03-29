using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace ReactiveDots.Tests
{
    public class BasicReactiveSystemWithExternalEcbTests : TestBase
    {
        private TestReactiveWithExternalEcbSystem _testReactive;

        protected override void OnSetup()
        {
            _testReactive = World.AddSystemManaged( new TestReactiveWithExternalEcbSystem() );
        }

        [Test]
        public void HasReactiveComponent()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData( entity, new TestWithExternalEcbComponent() { Value = 0 } );
            _testReactive.Update();
            Assert.True( EntityManager.HasComponent<TestReactiveWithExternalEcbSystem.TestComponentReactive>( entity ),
                "Entity should have a TestSystem.TestComponentReactive but has not." );
        }

        [Test]
        public void IsAdded()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData( entity, new TestWithExternalEcbComponent() { Value = 0 } );
            _testReactive.Update();

            var reactiveData = EntityManager
                .GetComponentData<TestReactiveWithExternalEcbSystem.TestComponentReactive>( entity ).Value;
            Assert.True( reactiveData.Added,
                "Reactive data .Added should be true in first update, but it is false!" );

            _testReactive.Update();
            reactiveData = EntityManager
                .GetComponentData<TestReactiveWithExternalEcbSystem.TestComponentReactive>( entity ).Value;
            Assert.False( reactiveData.Added,
                "Reactive data .Added should be false in second update, but it is true!" );
        }

        [Test]
        public void IsChanged()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData( entity, new TestWithExternalEcbComponent() { Value = 0 } );

            _testReactive.Update();
            var reactiveData1 = EntityManager
                .GetComponentData<TestReactiveWithExternalEcbSystem.TestComponentReactive>( entity ).Value;
            Assert.False( reactiveData1.Changed,
                "Reactive data .Changed should be false in first update, but it is true!" );

            _testReactive.Update();
            var reactiveData2 = EntityManager
                .GetComponentData<TestReactiveWithExternalEcbSystem.TestComponentReactive>( entity ).Value;
            Assert.False( reactiveData2.Changed,
                "Reactive data .Changed should be false in second update, but it is true!" );

            EntityManager.SetComponentData( entity, new TestWithExternalEcbComponent() { Value = 1 } );
            _testReactive.Update();
            var reactiveData3 = EntityManager
                .GetComponentData<TestReactiveWithExternalEcbSystem.TestComponentReactive>( entity ).Value;
            Assert.True( reactiveData3.Changed,
                "Reactive data .Changed should be true after change, but it is false!" );
            Assert.True( reactiveData3.PreviousValue.Value == 1,
                "Reactive data .PreviousValue.Value should be equal to main component value after change, but it is not!" );
        }

        [Test]
        public void IsRemovedOnComponentRemoval()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData( entity, new TestWithExternalEcbComponent() { Value = 0 } );
            _testReactive.Update();

            var reactiveData1 = EntityManager
                .GetComponentData<TestReactiveWithExternalEcbSystem.TestComponentReactive>( entity ).Value;
            Assert.False( reactiveData1.Removed,
                "Reactive data .Removed should be false in first update, but it is true!" );

            EntityManager.RemoveComponent<TestWithExternalEcbComponent>( entity );
            _testReactive.Update();
            var reactiveData2 = EntityManager
                .GetComponentData<TestReactiveWithExternalEcbSystem.TestComponentReactive>( entity ).Value;
            Assert.True( reactiveData2.Removed,
                "Reactive data .Added should be true after main component removal, but it is false!" );

            _testReactive.Update();
            Assert.True( EntityManager.HasComponent<TestReactiveWithExternalEcbSystem.TestComponentReactive>( entity ),
                "Reactive data should still be present in the second frame after main component removal, but it is NOT!" );
            var reactiveData3 = EntityManager
                .GetComponentData<TestReactiveWithExternalEcbSystem.TestComponentReactive>( entity ).Value;
            Assert.False( reactiveData3.Removed,
                "Reactive data .Removed should be false in the second frame after main component removal, but it is true!" );
            Assert.False( reactiveData3._AddedCheck,
                "Reactive data ._AddedCheck should be false in the second frame after main component removal, but it is true!" );
        }

        [Test]
        public void IsRemovedOnEntityDestroy()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData( entity, new TestWithExternalEcbComponent() { Value = 0 } );
            _testReactive.Update();

            var reactiveData1 = EntityManager
                .GetComponentData<TestReactiveWithExternalEcbSystem.TestComponentReactive>( entity ).Value;
            Assert.False( reactiveData1.Removed,
                "Reactive data .Removed should be false in first update, but it is true!" );

            EntityManager.DestroyEntity( entity );
            _testReactive.Update();
            var reactiveData2 = EntityManager
                .GetComponentData<TestReactiveWithExternalEcbSystem.TestComponentReactive>( entity ).Value;
            Assert.True( reactiveData2.Removed,
                "Reactive data .Added should be true after entity destroy, but it is false!" );

            _testReactive.Update();
            Assert.False( EntityManager.HasComponent<TestReactiveWithExternalEcbSystem.TestComponentReactive>( entity ),
                "Reactive data should not be present in the second frame after entity destroy, but it is!" );
        }
    }

    public struct TestWithExternalEcbComponent : IComponentData
    {
        public int Value;
    }

    [DisableAutoCreation]
    [ReactiveSystem( typeof(TestWithExternalEcbComponent), typeof(TestComponentReactive) )]
    public partial class TestReactiveWithExternalEcbSystem : SystemBase
    {
        public struct TestComponentReactive : ICleanupComponentData
        {
            public ComponentReactiveData<TestWithExternalEcbComponent> Value;
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            GetEntityQuery(
                ComponentType.Exclude<TestWithExternalEcbComponent>(),
                ComponentType.ReadWrite<TestComponentReactive>() );
        }

        protected override void OnUpdate()
        {
            var ecbForAdded      = new EntityCommandBuffer( Allocator.TempJob );
            var ecbForMissingTag = new EntityCommandBuffer( Allocator.TempJob );
            var ecbForCleanup    = new EntityCommandBuffer( Allocator.TempJob );
            Dependency = this.UpdateReactive( Dependency, ecbForAdded, ecbForMissingTag, ecbForCleanup );
            Dependency.Complete();
            ecbForAdded.Playback( EntityManager );
            ecbForAdded.Dispose();
            ecbForMissingTag.Playback( EntityManager );
            ecbForMissingTag.Dispose();
            ecbForCleanup.Playback( EntityManager );
            ecbForCleanup.Dispose();
        }
    }
}