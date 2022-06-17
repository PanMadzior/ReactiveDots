using System;
using System.Linq;
using System.Reflection;

namespace ReactiveDots
{
    [AttributeUsage( AttributeTargets.Method, AllowMultiple = false )]
    public class InitMethodAttribute : Attribute
    {
        public static void InvokeInitMethodsFor( object obj )
        {
            obj.GetType()
                .GetRuntimeMethods()
                .Where( m => m.CustomAttributes.Any( a => a.AttributeType == typeof(InitMethodAttribute) ) )
                .ToList()
                .ForEach( m => m.Invoke( obj, null ) );
        }
    }
}