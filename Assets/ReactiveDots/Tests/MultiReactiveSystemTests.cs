using NUnit.Framework;
using Unity.Entities;

namespace ReactiveDots.Tests
{
    public class MultiReactiveSystemTests : TestBase
    {
        private TestMultiReactiveSystem _testReactive;

        protected override void OnSetup()
        {
            _testReactive = World.AddSystemManaged( new TestMultiReactiveSystem() );
        }

        [Test]
        public void HasReactiveComponent()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData( entity, new CompA() { Value = 0 } );
            EntityManager.AddComponentData( entity, new CompB() { Value = 10 } );
            _testReactive.Update();
            Assert.True( EntityManager.HasComponent<TestMultiReactiveSystem.CompAReactive>( entity ),
                "Entity should have a TestMultiReactiveSystem.CompAReactive but has not." );
            Assert.True( EntityManager.HasComponent<TestMultiReactiveSystem.CompBReactive>( entity ),
                "Entity should have a TestMultiReactiveSystem.CompBReactive but has not." );
        }

        [Test]
        public void IsAdded()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData( entity, new CompA() { Value = 0 } );
            _testReactive.Update();

            var reactiveDataForA = EntityManager.GetComponentData<TestMultiReactiveSystem.CompAReactive>( entity ).Value;
            Assert.True( reactiveDataForA.Added,
                "Reactive data .Added for CompA should be true in first update, but it is false!" );
            Assert.False( EntityManager.HasComponent<TestMultiReactiveSystem.CompBReactive>( entity ),
                "Entity should not have TestMultiReactiveSystem.CompBReactive yet, but has!" );

            EntityManager.AddComponentData( entity, new CompB() { Value = 10 } );
            _testReactive.Update();
            reactiveDataForA = EntityManager.GetComponentData<TestMultiReactiveSystem.CompAReactive>( entity ).Value;
            Assert.False( reactiveDataForA.Added,
                "Reactive data .Added for CompA should be false in second update, but it is true!" );
            var reactiveDataForB = EntityManager.GetComponentData<TestMultiReactiveSystem.CompBReactive>( entity ).Value;
            Assert.True( reactiveDataForB.Added,
                "Reactive data .Added for CompB should be true, but it is false!" );
            
            _testReactive.Update();
            reactiveDataForB = EntityManager.GetComponentData<TestMultiReactiveSystem.CompBReactive>( entity ).Value;
            Assert.False( reactiveDataForB.Added,
                "Reactive data .Added for CompB should be false in third update, but it is true!" );
        }

        [Test]
        public void IsChanged()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData( entity, new CompA() { Value = 0 } );
            _testReactive.Update();

            var reactiveDataForA = EntityManager.GetComponentData<TestMultiReactiveSystem.CompAReactive>( entity ).Value;
            Assert.False( reactiveDataForA.Changed,
                "Reactive data .Changed for CompA should be false in first update, but it is true!" );
            Assert.False( EntityManager.HasComponent<TestMultiReactiveSystem.CompBReactive>( entity ),
                "Entity should not have TestMultiReactiveSystem.CompBReactive yet, but has!" );

            EntityManager.SetComponentData( entity, new CompA() { Value = 1 } );
            EntityManager.AddComponentData( entity, new CompB() { Value = 10 } );
            _testReactive.Update();
            reactiveDataForA = EntityManager.GetComponentData<TestMultiReactiveSystem.CompAReactive>( entity ).Value;
            Assert.True( reactiveDataForA.Changed,
                "Reactive data .Changed for CompA should be true in second update, but it is false!" );
            var reactiveDataForB = EntityManager.GetComponentData<TestMultiReactiveSystem.CompBReactive>( entity ).Value;
            Assert.False( reactiveDataForB.Changed,
                "Reactive data .Changed for CompB should be false, but it is true!" );
            
            EntityManager.SetComponentData( entity, new CompB() { Value = 11 } );
            _testReactive.Update();
            reactiveDataForA = EntityManager.GetComponentData<TestMultiReactiveSystem.CompAReactive>( entity ).Value;
            Assert.False( reactiveDataForA.Changed,
                "Reactive data .Changed for CompA should be false in third update, but it is true!" );        
            reactiveDataForB = EntityManager.GetComponentData<TestMultiReactiveSystem.CompBReactive>( entity ).Value;
            Assert.True( reactiveDataForB.Changed,
                "Reactive data .Added for CompB should be true in third update, but it is false!" );
        }

        [Test]
        public void IsRemovedOnComponentRemoval()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData( entity, new CompA() { Value = 0 } );
            EntityManager.AddComponentData( entity, new CompB() { Value = 10 } );
            _testReactive.Update();

            var reactiveDataForA = EntityManager.GetComponentData<TestMultiReactiveSystem.CompAReactive>( entity ).Value;
            var reactiveDataForB = EntityManager.GetComponentData<TestMultiReactiveSystem.CompBReactive>( entity ).Value;
            Assert.False( reactiveDataForA.Removed,
                "Reactive data .Removed for CompA should be false in first update, but it is true!" );
            Assert.False( reactiveDataForB.Removed,
                "Reactive data .Removed for CompB should be false in first update, but it is true!" );

            EntityManager.RemoveComponent<CompA>( entity );
            _testReactive.Update();
            reactiveDataForA = EntityManager.GetComponentData<TestMultiReactiveSystem.CompAReactive>( entity ).Value;
            reactiveDataForB = EntityManager.GetComponentData<TestMultiReactiveSystem.CompBReactive>( entity ).Value;
            Assert.True( reactiveDataForA.Removed,
                "Reactive data .Added for CompA should be true after main component removal, but it is false!" );
            Assert.False( reactiveDataForB.Removed,
                "Reactive data .Removed for CompB should still be false after CompA removal, but it is true!" );

            _testReactive.Update();
            reactiveDataForB = EntityManager.GetComponentData<TestMultiReactiveSystem.CompBReactive>( entity ).Value;
            Assert.False( EntityManager.HasComponent<TestMultiReactiveSystem.CompAReactive>( entity ),
                "Reactive data for CompA should not be present in the second frame after main component removal, but it is!" );
            Assert.False( reactiveDataForB.Removed,
                "Reactive data .Removed for CompB should still be false after CompA removal, but it is true!" );
        }

        [Test]
        public void IsRemovedOnEntityDestroy()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData( entity, new CompA() { Value = 0 } );
            EntityManager.AddComponentData( entity, new CompB() { Value = 10 } );
            _testReactive.Update();

            var reactiveDataForA = EntityManager.GetComponentData<TestMultiReactiveSystem.CompAReactive>( entity ).Value;
            var reactiveDataForB = EntityManager.GetComponentData<TestMultiReactiveSystem.CompBReactive>( entity ).Value;
            Assert.False( reactiveDataForA.Removed,
                "Reactive data .Removed for CompA should be false in first update, but it is true!" );
            Assert.False( reactiveDataForB.Removed,
                "Reactive data .Removed for CompB should be false in first update, but it is true!" );

            EntityManager.DestroyEntity( entity );
            _testReactive.Update();
            reactiveDataForA = EntityManager.GetComponentData<TestMultiReactiveSystem.CompAReactive>( entity ).Value;
            reactiveDataForB = EntityManager.GetComponentData<TestMultiReactiveSystem.CompBReactive>( entity ).Value;
            Assert.True( reactiveDataForA.Removed,
                "Reactive data .Added for CompA should be true after entity destroy, but it is false!" );
            Assert.True( reactiveDataForB.Removed,
                "Reactive data .Added for CompB should be true after entity destroy, but it is false!" );

            _testReactive.Update();
            Assert.False( EntityManager.HasComponent<TestMultiReactiveSystem.CompAReactive>( entity ),
                "Reactive data for CompA should not be present in the second frame after entity destroy, but it is!" );
            Assert.False( EntityManager.HasComponent<TestMultiReactiveSystem.CompBReactive>( entity ),
                "Reactive data for CompB should not be present in the second frame after entity destroy, but it is!" );
        }
    }

    public struct CompA : IComponentData
    {
        public int Value;
    }

    public struct CompB : IComponentData
    {
        public int Value;
    }

    [DisableAutoCreation]
    [ReactiveSystem( typeof(CompA), typeof(CompAReactive) )]
    [ReactiveSystem( typeof(CompB), typeof(CompBReactive) )]
    public partial class TestMultiReactiveSystem : SystemBase
    {
        public struct CompAReactive : ICleanupComponentData
        {
            public ComponentReactiveData<CompA> Value;
        }
        
        public struct CompBReactive : ICleanupComponentData
        {
            public ComponentReactiveData<CompB> Value;
        }

        protected override void OnUpdate()
        {
            Dependency = this.UpdateReactiveNowWithEcb( Dependency );
            Dependency.Complete();
        }
    }
}