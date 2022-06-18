using System;

namespace ReactiveDots
{
    [AttributeUsage( validOn: AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = true )]
    public class ReactiveEventAttribute : Attribute
    {
        public EventType EventType { get; protected set; }
        public Type EventSystemType { get; protected set; }
        public string FieldNameToCompare { get; protected set; } = "Value";

        public ReactiveEventAttribute( EventType eventType = EventType.All )
        {
            EventType       = eventType;
            EventSystemType = typeof(ReactiveDots.DefaultEventSystem);
        }

        public ReactiveEventAttribute( EventType eventType, Type eventSystem )
        {
            EventType       = eventType;
            EventSystemType = eventSystem;
        }
    }
}