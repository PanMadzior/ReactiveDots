using System;

namespace ReactiveDots
{
    [AttributeUsage( validOn: AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = true )]
    public class ReactiveEventAttribute : Attribute
    {
        public EventType EventType { get; private set; }
        public Type EventSystemType { get; private set; }
        public string FieldNameToCompare = "Value";

        public ReactiveEventAttribute( EventType type = EventType.All )
        {
            EventType       = type;
            EventSystemType = typeof(ReactiveDots.DefaultEventSystem);
        }

        public ReactiveEventAttribute( EventType type, Type eventSystem )
        {
            EventType       = type;
            EventSystemType = eventSystem;
        }
    }
}