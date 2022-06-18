using System;
using Unity.Entities;

namespace ReactiveDots
{
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true )]
    public class ReactiveSystemAttribute : AlwaysUpdateSystemAttribute
    {
        public Type ComponentType;
        public Type RComponentType;
        /// <summary>
        /// Field name in your component which will be used for comparing current and previous values of your component to determine if it has changed since the last system update.
        /// </summary>
        public string FieldNameToCompare;

        /// <summary>
        /// Use this attribute on a system which you want to react to changes to the specified component.
        /// Remember to add <c>Dependency = this.UpdateReactive( Dependency );</c> at the beginning of the system OnUpdate method.
        /// </summary>
        /// <param name="componentType">Type of the component you want your system to check for changes.</param>
        /// <param name="rComponentType">Type of the component your system will use to cache the main component changes.
        /// This must be an ISystemStateComponentData component declared in the scope of your system.
        /// This component have to have <c>public ComponentReactiveData{T} Value;</c> field, where <c>{T}</c> is the type of your main component.</param>
        public ReactiveSystemAttribute( Type componentType, Type rComponentType )
        {
            ComponentType      = componentType;
            RComponentType     = rComponentType;
            FieldNameToCompare = "Value";
        }
    }
}