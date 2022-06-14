using System;
using Unity.Entities;

namespace ReactiveDots
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ReactiveSystemAttribute : AlwaysUpdateSystemAttribute
    {
        public Type   ComponentType;
        public Type   RComponentType;
        public string FieldNameToCompare = "Value";

        public ReactiveSystemAttribute( Type componentType, Type rComponentType )
        {
            ComponentType  = componentType;
            RComponentType = rComponentType;
        }
    }
}