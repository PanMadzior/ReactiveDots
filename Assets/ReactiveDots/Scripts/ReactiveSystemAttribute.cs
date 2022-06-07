using System;
using Unity.Entities;

namespace ReactiveDots
{
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