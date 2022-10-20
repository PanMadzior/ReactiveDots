using NUnit.Framework;
using Unity.Entities;

namespace ReactiveDots.Tests
{
    public class BasicEventSystemTests : TestBase, IAnyEventComponentAddedListener, IAnyEventComponentChangedListener,
        IAnyEventComponentRemovedListener
    {
        private DefaultEventSystem _defaultEventSystem;
        private bool               _anyAddedInvoked;
        private bool               _anyChangedInvoked;
        private bool               _anyRemovedInvoked;

        protected override void OnSetup()
        {
            _defaultEventSystem = World.AddSystemManaged( new DefaultEventSystem() );
        }

        [Test]
        public void AnyAddedListener()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData( entity, new EventComponent() { Value = 0 } );
            var listener = EntityManager.CreateEntity();
            EntityManager.AddComponentData( listener, new AnyEventComponentAddedListener() { Value = this } );
            _anyAddedInvoked = false;
            _defaultEventSystem.Update();
            Assert.True( _anyAddedInvoked, "Any added event should have fired, but didn't!" );
        }

        public void OnAnyEventComponentAdded( Entity entity, EventComponent component, World world )
        {
            _anyAddedInvoked = true;
        }

        [Test]
        public void AnyChangedListener()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData( entity, new EventComponent() { Value = 0 } );
            var listener = EntityManager.CreateEntity();
            EntityManager.AddComponentData( listener, new AnyEventComponentChangedListener() { Value = this } );
            _anyChangedInvoked = false;
            _defaultEventSystem.Update();
            Assert.False( _anyChangedInvoked, "Any changed event should have not fired yet, but did!" );

            EntityManager.SetComponentData( entity, new EventComponent() { Value = 1 } );
            _defaultEventSystem.Update();
            Assert.True( _anyChangedInvoked, "Any changed event should have fired, but didn't!" );

            _anyChangedInvoked = false;
            _defaultEventSystem.Update();
            Assert.False( _anyChangedInvoked,
                "Any changed event should have not fired without a component change, but did!" );
        }

        public void OnAnyEventComponentChanged( Entity entity, EventComponent component, World world )
        {
            _anyChangedInvoked = true;
        }

        [Test]
        public void AnyRemovedListenerOnComponentRemoval()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData( entity, new EventComponent() { Value = 0 } );
            var listener = EntityManager.CreateEntity();
            EntityManager.AddComponentData( listener, new AnyEventComponentRemovedListener() { Value = this } );
            _anyRemovedInvoked = false;
            _defaultEventSystem.Update();
            Assert.False( _anyRemovedInvoked, "Any removed event should have not fired after component add, but did!" );

            EntityManager.SetComponentData( entity, new EventComponent() { Value = 1 } );
            _defaultEventSystem.Update();
            Assert.False( _anyRemovedInvoked,
                "Any removed event should have not fired after component change, but did!" );

            EntityManager.RemoveComponent<EventComponent>( entity );
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
            EntityManager.AddComponentData( entity, new EventComponent() { Value = 0 } );
            var listener = EntityManager.CreateEntity();
            EntityManager.AddComponentData( listener, new AnyEventComponentRemovedListener() { Value = this } );
            _anyRemovedInvoked = false;
            _defaultEventSystem.Update();
            Assert.False( _anyRemovedInvoked, "Any removed event should have not fired after component add, but did!" );

            EntityManager.SetComponentData( entity, new EventComponent() { Value = 1 } );
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

        public void OnAnyEventComponentRemoved( Entity entity, World world )
        {
            _anyRemovedInvoked = true;
        }        
        
        [Test]
        public void MultipleEntities()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData( entity, new EventComponent() { Value = 0 } );
            var listener = EntityManager.CreateEntity();
            EntityManager.AddComponentData( listener, new AnyEventComponentChangedListener() { Value = this } );
            _defaultEventSystem.Update();
            
            Assert.DoesNotThrow( () =>
            {
                var entity2 = EntityManager.CreateEntity();
                EntityManager.AddComponentData( entity2, new EventComponent() { Value = 0 } );
                _defaultEventSystem.Update();
            }, "Exception caught when adding second entity with event component." );
        }
    }

    [ReactiveEvent( EventType.All )]
    public struct EventComponent : IComponentData
    {
        public int Value;
    }
}