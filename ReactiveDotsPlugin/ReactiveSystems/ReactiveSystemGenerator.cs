using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
            var globalTemplate = ReactiveSystemTemplates.GetGlobalTemplate();
            var usingsInsert = GeneratorUtils.GetUsingsInsert( context, reactiveSystem.ClassSyntax, GetCommonUsings() );
            var reactiveUpdatesChangedInsert = string.Empty;
            var reactiveUpdatesAddedRemovedInsert = string.Empty;
            var reactiveComponentsInsert = string.Empty;
            for ( int i = 0; i < reactiveSystem.ReactiveAttributes.Count; i++ ) {
                if ( !reactiveSystem.ReactiveAttributes[i].IsValid )
                    continue;
                reactiveUpdatesAddedRemovedInsert += "\n" + ReplaceKeywords(
                    ReactiveSystemTemplates.GetTemplateForSystemUpdateAddedRemoved(),
                    reactiveSystem, i );
                reactiveUpdatesChangedInsert += "\n" + ReplaceKeywords(
                    ReactiveSystemTemplates.GetTemplateForSystemUpdate(),
                    reactiveSystem, i );
                reactiveComponentsInsert += "\n" + ReplaceKeywords( ReactiveSystemTemplates.GetTemplateForComponent(),
                    reactiveSystem, i );
            }

            var source = globalTemplate
                .Replace( "$$placeForUsings$$", usingsInsert )
                .Replace( "$$namespace$$", reactiveSystem.SystemNamespace )
                .Replace( "$$systemNameFull$$", reactiveSystem.SystemNameFull )
                .Replace( "$$systemName$$", reactiveSystem.SystemName )
                .Replace( "$$placeForUpdatesAddedRemoved$$", reactiveUpdatesAddedRemovedInsert )
                .Replace( "$$placeForUpdatesChanged$$", reactiveUpdatesChangedInsert )
                .Replace( "$$placeForComponents$$", reactiveComponentsInsert );
            context.AddSource( $"{reactiveSystem.SystemName}.Reactive.g.cs", SourceText.From( source, Encoding.UTF8 ) );
        }

        private string ReplaceKeywords( string template, ReactiveSystemInfo systemInfo, int attributeIndex )
        {
            return template
                .Replace( "$$systemNameFull$$", systemInfo.SystemNameFull )
                .Replace( "$$systemName$$", systemInfo.SystemName )
                .Replace( "$$componentName$$", systemInfo.ReactiveAttributes[attributeIndex].ComponentName )
                .Replace( "$$componentNameFull$$", systemInfo.ReactiveAttributes[attributeIndex].ComponentNameFull )
                .Replace( "$$reactiveComponentNameFull$$",
                    systemInfo.ReactiveAttributes[attributeIndex].ReactiveComponentNameFull )
                .Replace( "$$variableNameToCompare$$",
                    systemInfo.ReactiveAttributes[attributeIndex].FieldToCompareName );
        }

        private HashSet<string> GetCommonUsings()
        {
            var set = new HashSet<string>();
            set.Add( "System" );
            set.Add( "System.Collections.Generic" );
            set.Add( "Unity.Collections" );
            set.Add( "Unity.Entities" );
            set.Add( "ReactiveDots" );
            return set;
        }
    }
}