using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
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
                foreach ( var reactiveComponent in receiver.EventComponents )
                    reactiveComponent.Update( context );

                foreach ( var eventSystem in receiver.EventSystems ) {
                    var components =
                        receiver.EventComponents.Where( ec => ec.EventSystemClassSyntax == eventSystem.ClassSyntax );
                    GenerateEventSystem( context, eventSystem, components.ToList(), receiver.EventComponents );
                }
            }
            catch ( Exception e ) {
                Debug.WriteLine( "EventSystemGenerator exception:\n" + e );
            }
        }

        private void GenerateEventSystem( GeneratorExecutionContext context, EventSystemInfo eventSystem,
            List<EventComponentInfo> components, List<EventComponentInfo> rawComponents )
        {
            var generatedReactiveComponents       = new HashSet<string>();
            var globalTemplate                    = EventSystemTemplates.GetGlobalTemplate();
            var reactiveUpdatesChangedInsert      = string.Empty;
            var reactiveUpdatesAddedRemovedInsert = string.Empty;
            var reactiveComponentProcessorsInsert = string.Empty;
            var reactiveComponentsInsert          = string.Empty;
            var componentEventsInsert             = string.Empty;
            var eventFiresInsert                  = $"// {components.Count()}, {rawComponents.Count()}";
            for ( int i = 0; i < components.Count; i++ ) {
                if ( !generatedReactiveComponents.Contains( components[i].ComponentNameFull ) ) {
                    reactiveUpdatesAddedRemovedInsert += "\n" + ReplaceKeywords(
                        ReactiveSystemTemplates.GetTemplateForSystemUpdateAddedRemoved(),
                        eventSystem, components[i] );
                    reactiveUpdatesChangedInsert += "\n" + ReplaceKeywords(
                        ReactiveSystemTemplates.GetTemplateForSystemUpdate(),
                        eventSystem, components[i] );
                    reactiveComponentProcessorsInsert += "\n" + ReplaceKeywords(
                        ReactiveSystemTemplates.GetTemplateForComponent(),
                        eventSystem, components[i] );
                    reactiveComponentsInsert += "\n" + ReplaceKeywords(
                        ReactiveSystemTemplates.GetTemplateForReactiveComponent(),
                        eventSystem, components[i] );
                    generatedReactiveComponents.Add( components[i].ComponentNameFull );
                }

                componentEventsInsert += "\n" + ReplaceKeywords( EventSystemTemplates.GetTemplateForComponentEvents(),
                    eventSystem, components[i] );
                eventFiresInsert += "\n" + ReplaceKeywords( EventSystemTemplates.GetTemplateForEventFire(),
                    eventSystem, components[i] );
            }

            var source = globalTemplate
                .Replace( "$$namespace$$", eventSystem.SystemNamespace )
                .Replace( "$$systemNameFull$$", eventSystem.SystemNameFull )
                .Replace( "$$systemName$$", eventSystem.SystemName )
                .Replace( "$$placeForUpdatesAddedRemoved$$", reactiveUpdatesAddedRemovedInsert )
                .Replace( "$$placeForUpdatesChanged$$", reactiveUpdatesChangedInsert )
                .Replace( "$$placeForReactiveComponents$$", reactiveComponentsInsert )
                .Replace( "$$placeForComponents$$", reactiveComponentProcessorsInsert )
                .Replace( "$$placeForComponentEvents$$", componentEventsInsert )
                .Replace( "$$placeForEventFires$$", eventFiresInsert );
            context.AddSource( $"{eventSystem.SystemName}.ReactiveEvents.g.cs",
                SourceText.From( source, Encoding.UTF8 ) );
        }

        private string ReplaceKeywords( string template, EventSystemInfo systemInfo, EventComponentInfo componentInfo )
        {
            return template
                .Replace( "$$systemNameFull$$", systemInfo.SystemNameFull )
                .Replace( "$$systemName$$", systemInfo.SystemName )
                .Replace( "$$componentName$$", componentInfo.ComponentName )
                .Replace( "$$componentNameFull$$", componentInfo.ComponentNameFull )
                .Replace( "$$reactiveComponentNameFull$$", componentInfo.ReactiveComponentNameFull )
                .Replace( "$$variableNameToCompare$$", componentInfo.FieldToCompareName );
        }
    }
}