using System;

namespace ReactiveDots
{
    [AttributeUsage( validOn: AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = true )]
    public class ReactiveEventAttribute : Attribute
    {
        public EventType EventType { get; protected set; }
        public Type EventSystemType { get; protected set; }
        /// <summary>
        /// Field name in your component which will be used for comparing current and previous values of your component to determine if it has changed since the last system update.
        /// </summary>
        public string FieldNameToCompare { get; protected set; } = "Value";

        /// <summary>
        /// Use this attribute to mark a component you want events to be available for.
        /// Source generators will add interfaces and components automatically.
        /// </summary>
        /// <param name="eventType">Type of the event you want to generate.</param>
        public ReactiveEventAttribute( EventType eventType = EventType.All )
        {
            EventType       = eventType;
            EventSystemType = typeof(ReactiveDots.DefaultEventSystem);
        }

        /// <summary>
        /// Use this attribute to mark a component you want events to be available for.
        /// Source generators will add interfaces and components automatically.
        /// </summary>
        /// <param name="eventType">Type of the event you want to generate.</param>
        /// <param name="eventSystem">System you want to be responsible for handling event management.
        /// <c>ReactiveDots.DefaultEventSystem</c> is default. Look for [ReactiveEventSystem] attribute if you want custom event system.</param>
        public ReactiveEventAttribute( EventType eventType, Type eventSystem )
        {
            EventType       = eventType;
            EventSystemType = eventSystem;
        }
    }
}