using NUnit.Framework;
using Unity.Entities;
using Unity.Jobs;

namespace ReactiveDots.Tests
{
    public class BasicReactiveSystemWithEntityManagerTests : TestBase
    {
        private TestReactiveWithEntityManagerSystem _testReactive;

        protected override void OnSetup()
        {
            _testReactive = World.AddSystem( new TestReactiveWithEntityManagerSystem() );
        }

        [Test]
        public void HasReactiveComponent()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData( entity, new TestWithEntityManagerComponent() { Value = 0 } );
            _testReactive.Update();
            Assert.True( EntityManager.HasComponent<TestReactiveWithEntityManagerSystem.TestComponentReactive>( entity ),
                "Entity should have a TestSystem.TestComponentReactive but has not." );
        }

        [Test]
        public void IsAdded()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData( entity, new TestWithEntityManagerComponent() { Value = 0 } );
            _testReactive.Update();

            var reactiveData = EntityManager.GetComponentData<TestReactiveWithEntityManagerSystem.TestComponentReactive>( entity ).Value;
            Assert.True( reactiveData.Added,
                "Reactive data .Added should be true in first update, but it is false!" );

            _testReactive.Update();
            reactiveData = EntityManager.GetComponentData<TestReactiveWithEntityManagerSystem.TestComponentReactive>( entity ).Value;
            Assert.False( reactiveData.Added,
                "Reactive data .Added should be false in second update, but it is true!" );
        }

        [Test]
        public void IsChanged()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData( entity, new TestWithEntityManagerComponent() { Value = 0 } );
            
            _testReactive.Update();
            var reactiveData1 = EntityManager.GetComponentData<TestReactiveWithEntityManagerSystem.TestComponentReactive>( entity ).Value;
            Assert.False( reactiveData1.Changed,
                "Reactive data .Changed should be false in first update, but it is true!" );

            _testReactive.Update();
            var reactiveData2 = EntityManager.GetComponentData<TestReactiveWithEntityManagerSystem.TestComponentReactive>( entity ).Value;
            Assert.False( reactiveData2.Changed,
                "Reactive data .Changed should be false in second update, but it is true!" );

            EntityManager.SetComponentData( entity, new TestWithEntityManagerComponent() { Value = 1 } );
            _testReactive.Update();
            var reactiveData3 = EntityManager.GetComponentData<TestReactiveWithEntityManagerSystem.TestComponentReactive>( entity ).Value;
            Assert.True( reactiveData3.Changed,
                "Reactive data .Changed should be true after change, but it is false!" );
            Assert.True( reactiveData3.PreviousValue.Value == 1,
                "Reactive data .PreviousValue.Value should be equal to main component value after change, but it is not!" );
        }

        [Test]
        public void IsRemovedOnComponentRemoval()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData( entity, new TestWithEntityManagerComponent() { Value = 0 } );
            _testReactive.Update();

            var reactiveData1 = EntityManager.GetComponentData<TestReactiveWithEntityManagerSystem.TestComponentReactive>( entity ).Value;
            Assert.False( reactiveData1.Removed,
                "Reactive data .Removed should be false in first update, but it is true!" );

            EntityManager.RemoveComponent<TestWithEntityManagerComponent>( entity );
            _testReactive.Update();
            var reactiveData2 = EntityManager.GetComponentData<TestReactiveWithEntityManagerSystem.TestComponentReactive>( entity ).Value;
            Assert.True( reactiveData2.Removed,
                "Reactive data .Added should be true after main component removal, but it is false!" );

            _testReactive.Update();
            Assert.False( EntityManager.HasComponent<TestReactiveWithEntityManagerSystem.TestComponentReactive>( entity ),
                "Reactive data should not be present in the second frame after main component removal, but it is!" );
        }

        [Test]
        public void IsRemovedOnEntityDestroy()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData( entity, new TestWithEntityManagerComponent() { Value = 0 } );
            _testReactive.Update();

            var reactiveData1 = EntityManager.GetComponentData<TestReactiveWithEntityManagerSystem.TestComponentReactive>( entity ).Value;
            Assert.False( reactiveData1.Removed,
                "Reactive data .Removed should be false in first update, but it is true!" );

            EntityManager.DestroyEntity( entity );
            _testReactive.Update();
            var reactiveData2 = EntityManager.GetComponentData<TestReactiveWithEntityManagerSystem.TestComponentReactive>( entity ).Value;
            Assert.True( reactiveData2.Removed,
                "Reactive data .Added should be true after entity destroy, but it is false!" );

            _testReactive.Update();
            Assert.False( EntityManager.HasComponent<TestReactiveWithEntityManagerSystem.TestComponentReactive>( entity ),
                "Reactive data should not be present in the second frame after entity destroy, but it is!" );
        }
    }

    public struct TestWithEntityManagerComponent : IComponentData
    {
        public int Value;
    }

    [DisableAutoCreation]
    [ReactiveSystem( typeof(TestWithEntityManagerComponent), typeof(TestComponentReactive) )]
    public partial class TestReactiveWithEntityManagerSystem : SystemBase
    {
        public struct TestComponentReactive : ISystemStateComponentData
        {
            public ComponentReactiveData<TestWithEntityManagerComponent> Value;
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            GetEntityQuery(
                ComponentType.Exclude<TestWithEntityManagerComponent>(),
                ComponentType.ReadWrite<TestComponentReactive>() );
        }

        protected override void OnUpdate()
        {
            Dependency = this.UpdateReactiveNowWithEntityManager( Dependency );
            Dependency.Complete();
        }
    }
}