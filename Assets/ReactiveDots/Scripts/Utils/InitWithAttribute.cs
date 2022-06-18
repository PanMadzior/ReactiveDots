using System;
using System.Linq;
using System.Reflection;
using Unity.Entities;

namespace ReactiveDots
{
    [AttributeUsage( AttributeTargets.Method, AllowMultiple = false )]
    public class InitWithAttribute : Attribute
    {
        public readonly Type SystemType;

        public InitWithAttribute( Type systemType )
        {
            this.SystemType = systemType;
        }

        public static void InvokeInitMethodsFor( SystemBase system )
        {
            AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany( x => x.GetTypes() )
                .SelectMany( m => m.GetRuntimeMethods() )
                .Where( m =>
                    m.GetCustomAttributes( typeof(InitWithAttribute), false )
                        .Any( a => ( (InitWithAttribute)a ).SystemType == system.GetType() ) )
                .ToList()
                .ForEach( m => m.Invoke( system, new object[] { system } ) );
        }
    }
}