using NUnit.Framework;
using Unity.Entities;
using Unity.Jobs;

namespace ReactiveDots.Tests
{
    public class EnableableComponentReactiveSystemTests : TestBase
    {
        private TestEnableableReactiveSystem _testReactive;

        protected override void OnSetup()
        {
            _testReactive = World.AddSystemManaged( new TestEnableableReactiveSystem() );
        }

        [Test]
        public void HasReactiveComponent()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData( entity, new TestEnableableComponent() );
            _testReactive.Update();
            Assert.True( EntityManager.HasComponent<TestEnableableReactiveSystem.TestEnableableComponentReactive>( entity ),
                "Entity should have a TestEnableableComponentReactive but has not." );
        }

        [Test]
        public void DoesSetComponentEnabledTriggerAddedAndRemoved()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData( entity, new TestEnableableComponent() );
            _testReactive.Update();

            var reactiveData = EntityManager.GetComponentData<TestEnableableReactiveSystem.TestEnableableComponentReactive>( entity )
                .Value;
            Assert.True( reactiveData.Added,
                "Reactive data .Added should be true in first update, but it is false!" );

            _testReactive.Update();
            reactiveData = EntityManager.GetComponentData<TestEnableableReactiveSystem.TestEnableableComponentReactive>( entity )
                .Value;
            Assert.False( reactiveData.Added,
                "Reactive data .Added should be false in second update, but it is true!" );
            
            EntityManager.SetComponentEnabled<TestEnableableComponent>( entity, false );
            _testReactive.Update();
            reactiveData = EntityManager.GetComponentData<TestEnableableReactiveSystem.TestEnableableComponentReactive>( entity )
                .Value;
            Assert.True( reactiveData.Removed,
                "Reactive data .Removed should be true if the component was disabled, but it is false!" );
            
            _testReactive.Update();
            reactiveData = EntityManager.GetComponentData<TestEnableableReactiveSystem.TestEnableableComponentReactive>( entity )
                .Value;
            Assert.False( reactiveData.Removed,
                "Reactive data .Removed should be reset to false two frames after the component was disabled, but it is true!" );
            
            EntityManager.SetComponentEnabled<TestEnableableComponent>( entity, true );
            _testReactive.Update();
            reactiveData = EntityManager.GetComponentData<TestEnableableReactiveSystem.TestEnableableComponentReactive>( entity )
                .Value;
            Assert.True( reactiveData.Added,
                "Reactive data .Added should be true if the component was enabled, but it is false!" );
            
            _testReactive.Update();
            reactiveData = EntityManager.GetComponentData<TestEnableableReactiveSystem.TestEnableableComponentReactive>( entity )
                .Value;
            Assert.False( reactiveData.Added,
                "Reactive data .Added should be reset to false two frames after the component was enabled, but it is true!" );
        }
    }

    public struct TestEnableableComponent : IComponentData, IEnableableComponent { }

    [DisableAutoCreation]
    [ReactiveSystem( typeof(TestEnableableComponent), typeof(TestEnableableComponentReactive), FieldNameToCompare = "" )]
    public partial class TestEnableableReactiveSystem : SystemBase
    {
        public struct TestEnableableComponentReactive : ICleanupComponentData
        {
            public ComponentReactiveData<TestEnableableComponent> Value;
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            GetEntityQuery(
                ComponentType.Exclude<ReactiveDots.Tests.TestEnableableComponent>(),
                ComponentType.ReadWrite<ReactiveDots.Tests.TestEnableableReactiveSystem.TestEnableableComponentReactive>() );
        }

        protected override void OnUpdate()
        {
            Dependency = this.UpdateReactiveNowWithEcb( Dependency );
            Dependency.Complete();
        }
    }
}