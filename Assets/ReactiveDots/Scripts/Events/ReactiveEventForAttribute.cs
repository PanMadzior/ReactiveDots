using System;

namespace ReactiveDots
{
    public class ReactiveEventForAttribute : ReactiveEventAttribute
    {
        public Type ComponentType { get; protected set; }

        public ReactiveEventForAttribute( Type component, EventType eventType = EventType.All )
        {
            ComponentType   = component;
            EventType       = eventType;
            EventSystemType = typeof(ReactiveDots.DefaultEventSystem);
        }

        public ReactiveEventForAttribute( Type component, EventType eventType, Type eventSystem )
        {
            ComponentType   = component;
            EventType       = eventType;
            EventSystemType = eventSystem;
        }
    }
}