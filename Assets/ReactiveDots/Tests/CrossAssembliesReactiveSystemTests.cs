using NUnit.Framework;
using ReactiveDots.Tests.SecondTestAssembly;
using Unity.Entities;

namespace ReactiveDots.Tests
{
    public class CrossAssembliesReactiveSystemTests : TestBase
    {
        private CrossAssembliesTestReactiveSystem _testReactive;

        protected override void OnSetup()
        {
            _testReactive = World.AddSystem( new CrossAssembliesTestReactiveSystem() );
        }

        [Test]
        public void HasReactiveComponent()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData( entity, new OtherAssemblyTestComponent() { Value = 0 } );
            _testReactive.Update();
            Assert.True( EntityManager.HasComponent<CrossAssembliesTestReactiveSystem.TestComponentReactive>( entity ),
                "Entity should have a TestSystem.TestComponentReactive but has not." );
        }

        [Test]
        public void IsAdded()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData( entity, new OtherAssemblyTestComponent() { Value = 0 } );
            _testReactive.Update();

            var reactiveData = EntityManager
                .GetComponentData<CrossAssembliesTestReactiveSystem.TestComponentReactive>( entity ).Value;
            Assert.True( reactiveData.Added,
                "Reactive data .Added should be true in first update, but it is false!" );

            _testReactive.Update();
            reactiveData = EntityManager
                .GetComponentData<CrossAssembliesTestReactiveSystem.TestComponentReactive>( entity ).Value;
            Assert.False( reactiveData.Added,
                "Reactive data .Added should be false in second update, but it is true!" );
        }

        [Test]
        public void IsChanged()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData( entity, new OtherAssemblyTestComponent() { Value = 0 } );

            _testReactive.Update();
            var reactiveData1 = EntityManager
                .GetComponentData<CrossAssembliesTestReactiveSystem.TestComponentReactive>( entity ).Value;
            Assert.False( reactiveData1.Changed,
                "Reactive data .Changed should be false in first update, but it is true!" );

            _testReactive.Update();
            var reactiveData2 = EntityManager
                .GetComponentData<CrossAssembliesTestReactiveSystem.TestComponentReactive>( entity ).Value;
            Assert.False( reactiveData2.Changed,
                "Reactive data .Changed should be false in second update, but it is true!" );

            EntityManager.SetComponentData( entity, new OtherAssemblyTestComponent() { Value = 1 } );
            _testReactive.Update();
            var reactiveData3 = EntityManager
                .GetComponentData<CrossAssembliesTestReactiveSystem.TestComponentReactive>( entity ).Value;
            Assert.True( reactiveData3.Changed,
                "Reactive data .Changed should be true after change, but it is false!" );
            Assert.True( reactiveData3.PreviousValue.Value == 1,
                "Reactive data .PreviousValue.Value should be equal to main component value after change, but it is not!" );
        }

        [Test]
        public void IsRemovedOnComponentRemoval()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData( entity, new OtherAssemblyTestComponent() { Value = 0 } );
            _testReactive.Update();

            var reactiveData1 = EntityManager
                .GetComponentData<CrossAssembliesTestReactiveSystem.TestComponentReactive>( entity ).Value;
            Assert.False( reactiveData1.Removed,
                "Reactive data .Removed should be false in first update, but it is true!" );

            EntityManager.RemoveComponent<OtherAssemblyTestComponent>( entity );
            _testReactive.Update();
            var reactiveData2 = EntityManager
                .GetComponentData<CrossAssembliesTestReactiveSystem.TestComponentReactive>( entity ).Value;
            Assert.True( reactiveData2.Removed,
                "Reactive data .Added should be true after main component removal, but it is false!" );

            _testReactive.Update();
            Assert.False( EntityManager.HasComponent<CrossAssembliesTestReactiveSystem.TestComponentReactive>( entity ),
                "Reactive data should not be present in the second frame after main component removal, but it is!" );
        }

        [Test]
        public void IsRemovedOnEntityDestroy()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData( entity, new OtherAssemblyTestComponent() { Value = 0 } );
            _testReactive.Update();

            var reactiveData1 = EntityManager
                .GetComponentData<CrossAssembliesTestReactiveSystem.TestComponentReactive>( entity ).Value;
            Assert.False( reactiveData1.Removed,
                "Reactive data .Removed should be false in first update, but it is true!" );

            EntityManager.DestroyEntity( entity );
            _testReactive.Update();
            var reactiveData2 = EntityManager
                .GetComponentData<CrossAssembliesTestReactiveSystem.TestComponentReactive>( entity ).Value;
            Assert.True( reactiveData2.Removed,
                "Reactive data .Added should be true after entity destroy, but it is false!" );

            _testReactive.Update();
            Assert.False( EntityManager.HasComponent<CrossAssembliesTestReactiveSystem.TestComponentReactive>( entity ),
                "Reactive data should not be present in the second frame after entity destroy, but it is!" );
        }
    }

    [DisableAutoCreation]
    [ReactiveSystem( typeof(OtherAssemblyTestComponent), typeof(TestComponentReactive) )]
    public partial class CrossAssembliesTestReactiveSystem : SystemBase
    {
        public struct TestComponentReactive : ISystemStateComponentData
        {
            public ComponentReactiveData<OtherAssemblyTestComponent> Value;
        }

        protected override void OnUpdate()
        {
            Dependency = this.UpdateReactive( Dependency );
            Dependency.Complete();
        }
    }
}