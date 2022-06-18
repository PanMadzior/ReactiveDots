using System;

namespace ReactiveDots
{
    public class ReactiveEventForAttribute : ReactiveEventAttribute
    {
        public Type ComponentType { get; protected set; }

        /// <summary>
        /// Use this attribute to mark a component you want events to be available for.
        /// Use this attribute instead of [ReactiveEvent] when you are unable to mark component directly.
        /// Source generators will add interfaces and components automatically.
        /// </summary>
        /// <param name="component">Type of the component you want generate events for.
        /// IMPORTANT NOTE: Use full name with namespace for proper generation, for example <c>typeof( Path.To.Your.Component )</c></param>
        /// <param name="eventType">Type of the event you want to generate.</param>
        public ReactiveEventForAttribute( Type component, EventType eventType = EventType.All )
        {
            ComponentType   = component;
            EventType       = eventType;
            EventSystemType = typeof(ReactiveDots.DefaultEventSystem);
        }

        /// <summary>
        /// Use this attribute to mark a component you want events to be available for.
        /// Use this attribute instead of [ReactiveEvent] when you are unable to mark component directly.
        /// Source generators will add interfaces and components automatically.
        /// </summary>
        /// <param name="component">Type of the component you want generate events for.
        /// IMPORTANT NOTE: Use full name with namespace for proper generation, for example <c>typeof( Path.To.Your.Component )</c></param>
        /// <param name="eventType">Type of the event you want to generate.</param>
        /// <param name="eventSystem">System you want to be responsible for handling event management.
        /// <c>ReactiveDots.DefaultEventSystem</c> is default. Look for [ReactiveEventSystem] attribute if you want custom event system.</param>
        public ReactiveEventForAttribute( Type component, EventType eventType, Type eventSystem )
        {
            ComponentType   = component;
            EventType       = eventType;
            EventSystemType = eventSystem;
        }
    }
}