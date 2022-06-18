using System;

namespace ReactiveDots
{
    /// <summary>
    /// Mark your custom event system with this attribute to generate some necessary code.
    /// Look for <c>ReactiveDots.DefaultEventSystem</c> for an example usage.
    /// </summary>
    [AttributeUsage( validOn: AttributeTargets.Class, AllowMultiple = false )]
    public class ReactiveEventSystemAttribute : Attribute { }
}