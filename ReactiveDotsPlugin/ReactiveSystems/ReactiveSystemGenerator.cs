using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace ReactiveDotsPlugin
{
    [Generator]
    public class ReactiveSystemGenerator : ISourceGenerator
    {
        public void Initialize( GeneratorInitializationContext context )
        {
            context.RegisterForSyntaxNotifications( () => new ReactiveSystemSyntaxReceiver() );
        }

        public void Execute( GeneratorExecutionContext context )
        {
            var receiver = context.SyntaxReceiver as ReactiveSystemSyntaxReceiver;
            try {
                foreach ( var reactiveSystem in receiver.ReactiveSystems ) {
                    reactiveSystem.UpdateAttributes( context );
                    GenerateReactiveSystem( context, reactiveSystem );
                }
            }
            catch ( Exception e ) {
                Debug.WriteLine( "ReactiveSystemGenerator exception:\n" + e );
            }
        }

        private void GenerateReactiveSystem( GeneratorExecutionContext context, ReactiveSystemInfo reactiveSystem )
        {
            var globalTemplate           = ReactiveSystemTemplates.GetGlobalTemplate();
            var reactiveUpdatesInsert    = string.Empty;
            var reactiveComponentsInsert = string.Empty;
            for ( int i = 0; i < reactiveSystem.ReactiveAttributes.Count; i++ ) {
                reactiveUpdatesInsert += "\n" + ReplaceKeywords( ReactiveSystemTemplates.GetTemplateForSystemUpdate(),
                    reactiveSystem, i );
                reactiveComponentsInsert += "\n" + ReplaceKeywords( ReactiveSystemTemplates.GetTemplateForComponent(),
                    reactiveSystem, i );
            }

            var source = globalTemplate
                .Replace( "NAMESPACENAME", reactiveSystem.SystemNamespace )
                .Replace( "SYSNAMEFULL", reactiveSystem.SystemNameFull )
                .Replace( "SYSNAME", reactiveSystem.SystemName )
                .Replace( "PLACE_FOR_UPDATES", reactiveUpdatesInsert )
                .Replace( "PLACE_FOR_COMPONENTS", reactiveComponentsInsert );
            context.AddSource( $"{reactiveSystem.SystemName}.Reactive.g.cs", SourceText.From( source, Encoding.UTF8 ) );
        }

        private string ReplaceKeywords( string template, ReactiveSystemInfo systemInfo, int attributeIndex )
        {
            return template // order matters
                .Replace( "SYSNAMEFULL", systemInfo.SystemNameFull )
                .Replace( "SYSNAME", systemInfo.SystemName )
                .Replace( "COMPNAME", systemInfo.ReactiveAttributes[attributeIndex].ComponentName )
                .Replace( "RCOMPONENTFULL", systemInfo.ReactiveAttributes[attributeIndex].ReactiveComponentNameFull )
                .Replace( "COMPONENTFULL", systemInfo.ReactiveAttributes[attributeIndex].ComponentNameFull )
                .Replace( "VARIABLENAMETOCOMPARE", systemInfo.ReactiveAttributes[attributeIndex].FieldToCompareName );
        }
    }
}