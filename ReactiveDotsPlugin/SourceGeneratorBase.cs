using Microsoft.CodeAnalysis;

namespace ReactiveDotsPlugin
{
    public abstract class SourceGeneratorBase : ISourceGenerator
    {
        public struct Replacer
        {
            public string usings;
            public string systemNamespace;
            public string checkIfChangedMethodBody;
            public string systemNameFull;
            public string systemName;
            public bool   isTagComponent;
            public string componentName;
            public string componentNameFull;
            public string reactiveComponentNameFull;
            
            public string Replace( string original )
            {
                return original
                    .Replace( "$$placeForUsings$$", usings )
                    .Replace( "$$namespace$$", systemNamespace )
                    .Replace( "$$placeForCheckIfChangedBody$$", checkIfChangedMethodBody )
                    .Replace( "$$systemNameFull$$", systemNameFull )
                    .Replace( "$$systemName$$", systemName )
                    .Replace( "$$isTagComponent$$", isTagComponent ? "true" : "false" )
                    .Replace( "$$componentName$$", componentName )
                    .Replace( "$$componentNameFull$$", componentNameFull )
                    .Replace( "$$reactiveComponentNameFull$$", reactiveComponentNameFull );
            }
        }
        
        public abstract void Initialize( GeneratorInitializationContext context );
        public abstract void Execute( GeneratorExecutionContext context );
        
        protected HashSet<string> GetCommonUsings()
        {
            var set = new HashSet<string>();
            set.Add( "System" );
            set.Add( "System.Collections.Generic" );
            set.Add( "Unity.Collections" );
            set.Add( "Unity.Entities" );
            set.Add( "Unity.Burst.Intrinsics" );
            set.Add( "ReactiveDots" );
            return set;
        }
    }
}