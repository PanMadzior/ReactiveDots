using NUnit.Framework;
using Unity.Entities;
using Unity.Jobs;

namespace ReactiveDots.Tests
{
    public class TagComponentReactiveSystemTests : TestBase
    {
        private TestTagReactiveSystem _testReactive;

        protected override void OnSetup()
        {
            _testReactive = World.AddSystemManaged( new TestTagReactiveSystem() );
        }

        [Test]
        public void HasReactiveComponent()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData( entity, new TestTagComponent() );
            _testReactive.Update();
            Assert.True( EntityManager.HasComponent<TestTagReactiveSystem.TestTagComponentReactive>( entity ),
                "Entity should have a TestTagComponentReactive but has not." );
        }

        [Test]
        public void IsAdded()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData( entity, new TestTagComponent() );
            _testReactive.Update();

            var reactiveData = EntityManager.GetComponentData<TestTagReactiveSystem.TestTagComponentReactive>( entity )
                .Value;
            Assert.True( reactiveData.Added,
                "Reactive data .Added should be true in first update, but it is false!" );

            _testReactive.Update();
            reactiveData = EntityManager.GetComponentData<TestTagReactiveSystem.TestTagComponentReactive>( entity )
                .Value;
            Assert.False( reactiveData.Added,
                "Reactive data .Added should be false in second update, but it is true!" );
        }

        [Test]
        public void IsRemovedOnComponentRemoval()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData( entity, new TestTagComponent() );
            _testReactive.Update();

            var reactiveData1 = EntityManager.GetComponentData<TestTagReactiveSystem.TestTagComponentReactive>( entity )
                .Value;
            Assert.False( reactiveData1.Removed,
                "Reactive data .Removed should be false in first update, but it is true!" );

            EntityManager.RemoveComponent<TestTagComponent>( entity );
            _testReactive.Update();
            var reactiveData2 = EntityManager.GetComponentData<TestTagReactiveSystem.TestTagComponentReactive>( entity )
                .Value;
            Assert.True( reactiveData2.Removed,
                "Reactive data .Removed should be true after main component removal, but it is false!" );

            _testReactive.Update();
            Assert.True( EntityManager.HasComponent<TestTagReactiveSystem.TestTagComponentReactive>( entity ),
                "Reactive data should still be present in the second frame after main component removal, but it is NOT!" );
            var reactiveData3 = EntityManager.GetComponentData<TestTagReactiveSystem.TestTagComponentReactive>( entity )
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
            EntityManager.AddComponentData( entity, new TestTagComponent() );
            _testReactive.Update();

            var reactiveData1 = EntityManager.GetComponentData<TestTagReactiveSystem.TestTagComponentReactive>( entity )
                .Value;
            Assert.False( reactiveData1.Removed,
                "Reactive data .Removed should be false in first update, but it is true!" );

            EntityManager.DestroyEntity( entity );
            _testReactive.Update();
            var reactiveData2 = EntityManager.GetComponentData<TestTagReactiveSystem.TestTagComponentReactive>( entity )
                .Value;
            Assert.True( reactiveData2.Removed,
                "Reactive data .Added should be true after entity destroy, but it is false!" );

            _testReactive.Update();
            Assert.False( EntityManager.HasComponent<TestTagReactiveSystem.TestTagComponentReactive>( entity ),
                "Reactive data should not be present in the second frame after entity destroy, but it is!" );
        }
    }

    public struct TestTagComponent : IComponentData { }

    [DisableAutoCreation]
    [ReactiveSystem( typeof(TestTagComponent), typeof(TestTagComponentReactive), FieldNameToCompare = "" )]
    public partial class TestTagReactiveSystem : SystemBase
    {
        public struct TestTagComponentReactive : ICleanupComponentData
        {
            public ComponentReactiveData<TestTagComponent> Value;
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            GetEntityQuery(
                ComponentType.Exclude<ReactiveDots.Tests.TestTagComponent>(),
                ComponentType.ReadWrite<ReactiveDots.Tests.TestTagReactiveSystem.TestTagComponentReactive>() );
        }

        protected override void OnUpdate()
        {
            Dependency = this.UpdateReactiveNowWithEcb( Dependency );
            Dependency.Complete();
        }
    }
}