using NUnit.Framework;
using ReactiveDots.Tests.SecondTestAssembly;
using Unity.Entities;

namespace ReactiveDots.Tests
{
    [ReactiveEventFor( typeof(ReactiveDots.Tests.SecondTestAssembly.OtherAssemblyTestEventComponent) )]
    public class ForComponentEventSystemTests : TestBase, IAnyOtherAssemblyTestEventComponentAddedListener,
        IAnyOtherAssemblyTestEventComponentChangedListener,
        IAnyOtherAssemblyTestEventComponentRemovedListener
    {
        private DefaultEventSystem _defaultEventSystem;
        private bool               _anyAddedInvoked;
        private bool               _anyChangedInvoked;
        private bool               _anyRemovedInvoked;

        protected override void OnSetup()
        {
            _defaultEventSystem = World.AddSystem( new DefaultEventSystem() );
        }

        [Test]
        public void AnyAddedListener()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData( entity,
                new OtherAssemblyTestEventComponent() { Value = 0 } );
            var listener = EntityManager.CreateEntity();
            EntityManager.AddComponentData( listener,
                new AnyOtherAssemblyTestEventComponentAddedListener() { Value = this } );
            _anyAddedInvoked = false;
            _defaultEventSystem.Update();
            Assert.True( _anyAddedInvoked, "Any added event should have fired, but didn't!" );
        }

        public void OnAnyOtherAssemblyTestEventComponentAdded( Entity entity, OtherAssemblyTestEventComponent component,
            World world )
        {
            _anyAddedInvoked = true;
        }

        [Test]
        public void AnyChangedListener()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData( entity, new OtherAssemblyTestEventComponent() { Value = 0 } );
            var listener = EntityManager.CreateEntity();
            EntityManager.AddComponentData( listener,
                new AnyOtherAssemblyTestEventComponentChangedListener() { Value = this } );
            _anyChangedInvoked = false;
            _defaultEventSystem.Update();
            Assert.False( _anyChangedInvoked, "Any changed event should have not fired yet, but did!" );

            EntityManager.SetComponentData( entity, new OtherAssemblyTestEventComponent() { Value = 1 } );
            _defaultEventSystem.Update();
            Assert.True( _anyChangedInvoked, "Any changed event should have fired, but didn't!" );

            _anyChangedInvoked = false;
            _defaultEventSystem.Update();
            Assert.False( _anyChangedInvoked,
                "Any changed event should have not fired without a component change, but did!" );
        }

        public void OnAnyOtherAssemblyTestEventComponentChanged( Entity entity,
            OtherAssemblyTestEventComponent component, World world )
        {
            _anyChangedInvoked = true;
        }

        [Test]
        public void AnyRemovedListenerOnComponentRemoval()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData( entity, new OtherAssemblyTestEventComponent() { Value = 0 } );
            var listener = EntityManager.CreateEntity();
            EntityManager.AddComponentData( listener,
                new AnyOtherAssemblyTestEventComponentRemovedListener() { Value = this } );
            _anyRemovedInvoked = false;
            _defaultEventSystem.Update();
            Assert.False( _anyRemovedInvoked, "Any removed event should have not fired after component add, but did!" );

            EntityManager.SetComponentData( entity, new OtherAssemblyTestEventComponent() { Value = 1 } );
            _defaultEventSystem.Update();
            Assert.False( _anyRemovedInvoked,
                "Any removed event should have not fired after component change, but did!" );

            EntityManager.RemoveComponent<OtherAssemblyTestEventComponent>( entity );
            _defaultEventSystem.Update();
            Assert.True( _anyRemovedInvoked, "Any removed event should have fired, but didn't!" );
            _anyRemovedInvoked = false;

            _defaultEventSystem.Update();
            Assert.False( _anyRemovedInvoked,
                "Any removed event should have not fired in the second frame after component removal, but did!" );
        }

        [Test]
        public void AnyRemovedListenerOnEntityDestroy()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData( entity, new OtherAssemblyTestEventComponent() { Value = 0 } );
            var listener = EntityManager.CreateEntity();
            EntityManager.AddComponentData( listener,
                new AnyOtherAssemblyTestEventComponentRemovedListener() { Value = this } );
            _anyRemovedInvoked = false;
            _defaultEventSystem.Update();
            Assert.False( _anyRemovedInvoked, "Any removed event should have not fired after component add, but did!" );

            EntityManager.SetComponentData( entity, new OtherAssemblyTestEventComponent() { Value = 1 } );
            _defaultEventSystem.Update();
            Assert.False( _anyRemovedInvoked,
                "Any removed event should have not fired after component change, but did!" );

            EntityManager.DestroyEntity( entity );
            _defaultEventSystem.Update();
            Assert.True( _anyRemovedInvoked, "Any removed event should have fired, but didn't!" );
            _anyRemovedInvoked = false;

            _defaultEventSystem.Update();
            Assert.False( _anyRemovedInvoked,
                "Any removed event should have not fired in the second frame after entity destroy, but did!" );
        }

        public void OnAnyOtherAssemblyTestEventComponentRemoved( Entity entity, World world )
        {
            _anyRemovedInvoked = true;
        }
    }
}