using System;

namespace ReactiveDots
{
    [AttributeUsage( validOn: AttributeTargets.Class, AllowMultiple = false )]
    public class ReactiveEventSystemAttribute : Attribute { }
}