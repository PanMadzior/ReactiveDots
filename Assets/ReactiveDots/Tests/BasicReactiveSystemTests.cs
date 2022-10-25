using NUnit.Framework;
using Unity.Entities;
using Unity.Jobs;

namespace ReactiveDots.Tests
{
    public class BasicReactiveSystemTests : TestBase
    {
        private TestReactiveSystem _testReactive;

        protected override void OnSetup()
        {
            _testReactive = World.AddSystemManaged( new TestReactiveSystem() );
        }

        [Test]
        public void HasReactiveComponent()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData( entity, new TestComponent() { Value = 0 } );
            _testReactive.Update();
            Assert.True( EntityManager.HasComponent<TestReactiveSystem.TestComponentReactive>( entity ),
                "Entity should have a TestSystem.TestComponentReactive but has not." );
        }

        [Test]
        public void IsAdded()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData( entity, new TestComponent() { Value = 0 } );
            _testReactive.Update();

            var reactiveData = EntityManager.GetComponentData<TestReactiveSystem.TestComponentReactive>( entity ).Value;
            Assert.True( reactiveData.Added,
                "Reactive data .Added should be true in first update, but it is false!" );

            _testReactive.Update();
            reactiveData = EntityManager.GetComponentData<TestReactiveSystem.TestComponentReactive>( entity ).Value;
            Assert.False( reactiveData.Added,
                "Reactive data .Added should be false in second update, but it is true!" );
        }

        [Test]
        public void IsChanged()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData( entity, new TestComponent() { Value = 0 } );

            _testReactive.Update();
            var reactiveData1 = EntityManager.GetComponentData<TestReactiveSystem.TestComponentReactive>( entity )
                .Value;
            Assert.False( reactiveData1.Changed,
                "Reactive data .Changed should be false in first update, but it is true!" );

            _testReactive.Update();
            var reactiveData2 = EntityManager.GetComponentData<TestReactiveSystem.TestComponentReactive>( entity )
                .Value;
            Assert.False( reactiveData2.Changed,
                "Reactive data .Changed should be false in second update, but it is true!" );

            EntityManager.SetComponentData( entity, new TestComponent() { Value = 1 } );
            _testReactive.Update();
            var reactiveData3 = EntityManager.GetComponentData<TestReactiveSystem.TestComponentReactive>( entity )
                .Value;
            Assert.True( reactiveData3.Changed,
                "Reactive data .Changed should be true after change, but it is false!" );
            Assert.True( reactiveData3.PreviousValue.Value == 1,
                "Reactive data .PreviousValue.Value should be equal to main component value after change, but it is not!" );
        }

        [Test]
        public void IsRemovedOnComponentRemoval()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData( entity, new TestComponent() { Value = 0 } );
            _testReactive.Update();

            var reactiveData1 = EntityManager.GetComponentData<TestReactiveSystem.TestComponentReactive>( entity )
                .Value;
            Assert.False( reactiveData1.Removed,
                "Reactive data .Removed should be false in first update, but it is true!" );

            EntityManager.RemoveComponent<TestComponent>( entity );
            _testReactive.Update();
            var reactiveData2 = EntityManager.GetComponentData<TestReactiveSystem.TestComponentReactive>( entity )
                .Value;
            Assert.True( reactiveData2.Removed,
                "Reactive data .Added should be true after main component removal, but it is false!" );

            _testReactive.Update();
            Assert.True( EntityManager.HasComponent<TestReactiveSystem.TestComponentReactive>( entity ),
                "Reactive data should still be present in the second frame after main component removal, but it is NOT!" );
            var reactiveData3 = EntityManager.GetComponentData<TestReactiveSystem.TestComponentReactive>( entity )
                .Value;
            Assert.False( reactiveData3.Removed,
                "Reactive data .Removed should be false in the second frame after main component removal, but it is true!" );
            Assert.False( reactiveData3._AddedCheck,
                "Reactive data ._AddedCheck should be false in the second frame after main component removal, but it is true!" );
        }

        [Test]
        public void IsRemovedOnEntityDestroy()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData( entity, new TestComponent() { Value = 0 } );
            _testReactive.Update();

            var reactiveData1 = EntityManager.GetComponentData<TestReactiveSystem.TestComponentReactive>( entity )
                .Value;
            Assert.False( reactiveData1.Removed,
                "Reactive data .Removed should be false in first update, but it is true!" );

            EntityManager.DestroyEntity( entity );
            _testReactive.Update();
            var reactiveData2 = EntityManager.GetComponentData<TestReactiveSystem.TestComponentReactive>( entity )
                .Value;
            Assert.True( reactiveData2.Removed,
                "Reactive data .Added should be true after entity destroy, but it is false!" );

            _testReactive.Update();
            Assert.False( EntityManager.HasComponent<TestReactiveSystem.TestComponentReactive>( entity ),
                "Reactive data should not be present in the second frame after entity destroy, but it is!" );
        }
    }

    public struct TestComponent : IComponentData
    {
        public int Value;
    }

    [DisableAutoCreation]
    [ReactiveSystem( typeof(TestComponent), typeof(TestComponentReactive) )]
    public partial class TestReactiveSystem : SystemBase
    {
        public struct TestComponentReactive : ICleanupComponentData
        {
            public ComponentReactiveData<TestComponent> Value;
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            GetEntityQuery(
                ComponentType.Exclude<ReactiveDots.Tests.TestComponent>(),
                ComponentType.ReadWrite<ReactiveDots.Tests.TestReactiveSystem.TestComponentReactive>() );
        }

        protected override void OnUpdate()
        {
            Dependency = this.UpdateReactiveNowWithEcb( Dependency );
            Dependency.Complete();
        }
    }
}