using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace ReactiveDotsPlugin
{
    [Generator]
    public class EventSystemGenerator : ISourceGenerator
    {
        public void Initialize( GeneratorInitializationContext context )
        {
            context.RegisterForSyntaxNotifications( () => new EventSystemSyntaxReceiver() );
        }

        public void Execute( GeneratorExecutionContext context )
        {
            var receiver = context.SyntaxReceiver as EventSystemSyntaxReceiver;
            try {
                foreach ( var eventComponent in receiver.EventComponents ) {
                    GenerateComponentInterfaces( context, eventComponent );
                    GenerateComponentJobs( context, eventComponent );
                }

                foreach ( var eventSystem in receiver.EventSystems )
                    GenerateEventSystem( context, eventSystem );
            }
            catch ( Exception e ) {
                Debug.WriteLine( "EventSystemGenerator exception:\n" + e );
            }
        }

        private void GenerateEventSystem( GeneratorExecutionContext context, EventSystemInfo eventSystem )
        {
            var usingsInsert   = GetUsings( context, eventSystem.ClassSyntax );
            var globalTemplate = EventSystemTemplates.GetEventSystemMainTemplate();
            var source = globalTemplate
                .Replace( "$$placeForUsings$$", usingsInsert )
                .Replace( "$$namespace$$", eventSystem.SystemNamespace )
                .Replace( "$$systemName$$", eventSystem.SystemName );
            context.AddSource( $"{eventSystem.SystemName}.ReactiveEvents.g.cs",
                SourceText.From( source, Encoding.UTF8 ) );
        }

        private void GenerateComponentInterfaces( GeneratorExecutionContext context, EventComponentInfo eventComponent )
        {
            var usingsInsert   = GetUsings( context, eventComponent.StructSyntax );
            var globalTemplate = EventSystemTemplates.GetComponentInterfacesTemplate();
            var source = globalTemplate
                .Replace( "$$placeForUsings$$", usingsInsert )
                .Replace( "$$namespace$$", eventComponent.ComponentNamespace )
                .Replace( "$$systemName$$", eventComponent.EventSystemClassName )
                .Replace( "$$systemNameFull$$", eventComponent.EventSystemClassNameFull )
                .Replace( "$$componentName$$", eventComponent.ComponentName )
                .Replace( "$$componentNameFull$$", eventComponent.ComponentNameFull );
            context.AddSource(
                $"{eventComponent.ComponentName}_{eventComponent.EventSystemClassName}_Interfaces.ReactiveEvents.g.cs",
                SourceText.From( source, Encoding.UTF8 ) );
        }

        private void GenerateComponentJobs( GeneratorExecutionContext context, EventComponentInfo eventComponent )
        {
            var usingsInsert                      = GetUsings( context, eventComponent.StructSyntax );
            var globalTemplate                    = EventSystemTemplates.GetComponentJobsTemplate();
            var reactiveComponentProcessorsInsert = GetReactiveJobsInsert( eventComponent );
            var reactiveComponentDeclaration      = GetReactiveDeclarationInsert( eventComponent );
            var source = globalTemplate
                .Replace( "$$placeForUsings$$", usingsInsert )
                .Replace( "$$namespace$$", eventComponent.ComponentNamespace )
                .Replace( "$$systemName$$", eventComponent.EventSystemClassName )
                .Replace( "$$systemNameFull$$", eventComponent.EventSystemClassNameFull )
                .Replace( "$$componentName$$", eventComponent.ComponentName )
                .Replace( "$$componentNameFull$$", eventComponent.ComponentNameFull )
                .Replace( "$$reactiveComponentNameFull$$", eventComponent.ReactiveComponentNameFull )
                .Replace( "$$placeForComponents$$", reactiveComponentProcessorsInsert )
                .Replace( "$$placeForReactiveComponent$$", reactiveComponentDeclaration );
            context.AddSource(
                $"{eventComponent.ComponentName}_{eventComponent.EventSystemClassName}_Jobs.ReactiveEvents.g.cs",
                SourceText.From( source, Encoding.UTF8 ) );
        }

        private string GetReactiveDeclarationInsert( EventComponentInfo componentInfo )
        {
            var template = ReactiveSystemTemplates.GetTemplateForReactiveComponent();
            return template
                .Replace( "$$componentName$$", componentInfo.ComponentName )
                .Replace( "$$componentNameFull$$", componentInfo.ComponentNameFull );
        }

        private string GetReactiveJobsInsert( EventComponentInfo componentInfo )
        {
            var template = ReactiveSystemTemplates.GetTemplateForComponent();
            return template
                .Replace( "$$systemNameFull$$", componentInfo.EventSystemClassNameFull )
                .Replace( "$$systemName$$", componentInfo.EventSystemClassName )
                .Replace( "$$componentName$$", componentInfo.ComponentName )
                .Replace( "$$componentNameFull$$", componentInfo.ComponentNameFull )
                .Replace( "$$reactiveComponentNameFull$$", componentInfo.ReactiveComponentNameFull )
                .Replace( "$$variableNameToCompare$$", componentInfo.FieldToCompareName );
        }

        private string GetUsings( GeneratorExecutionContext context, TypeDeclarationSyntax type )
        {
            var usings = new HashSet<string>();
            PopulateSetWithCommonUsings( usings );
            GeneratorUtils.PopulateSetWithUniqueUsings( usings, type, context );
            var usingsInsert = string.Empty;
            foreach ( var u in usings )
                usingsInsert += "using " + u + ";\n";
            return usingsInsert;
        }

        private void PopulateSetWithCommonUsings( HashSet<string> set )
        {
            set.Add( "System" );
            set.Add( "System.Collections.Generic" );
            set.Add( "Unity.Collections" );
            set.Add( "Unity.Entities" );
            set.Add( "ReactiveDots" );
        }
    }
}