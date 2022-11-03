using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace ReactiveDotsPlugin
{
    [Generator]
    public class EventSystemGenerator : SourceGeneratorBase
    {
        public override void Initialize( GeneratorInitializationContext context )
        {
            context.RegisterForSyntaxNotifications( () => new EventSystemSyntaxReceiver() );
        }

        public override void Execute( GeneratorExecutionContext context )
        {
            var receiver = context.SyntaxReceiver as EventSystemSyntaxReceiver;
            try {
                foreach ( var eventComponent in receiver.EventComponents ) {
                    eventComponent.UpdateWithContext( context );
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
            var globalTemplate = EventSystemTemplates.GetEventSystemMainTemplate();
            var source         = GetReplacerForEventSystem( context, eventSystem ).Replace( globalTemplate );
            context.AddSource( $"{eventSystem.SystemName}.ReactiveEvents.g.cs",
                SourceText.From( source, Encoding.UTF8 ) );
        }

        private void GenerateComponentInterfaces( GeneratorExecutionContext context, EventComponentInfo eventComponent )
        {
            var globalTemplate = EventSystemTemplates.GetComponentInterfacesTemplate();
            var source         = GetReplacerForComponentInfo( context, eventComponent ).Replace( globalTemplate );
            context.AddSource(
                $"{eventComponent.ComponentName}_{eventComponent.EventSystemClassName}_Interfaces.ReactiveEvents.g.cs",
                SourceText.From( source, Encoding.UTF8 ) );
        }

        private void GenerateComponentJobs( GeneratorExecutionContext context, EventComponentInfo eventComponent )
        {
            var globalTemplate                    = EventSystemTemplates.GetComponentJobsTemplate();
            var reactiveComponentProcessorsInsert = ReactiveSystemTemplates.GetTemplateForComponent();
            var reactiveComponentDeclaration      = ReactiveSystemTemplates.GetTemplateForReactiveComponent();
            var sourceTemplate = globalTemplate
                .Replace( "$$placeForComponents$$", reactiveComponentProcessorsInsert )
                .Replace( "$$placeForReactiveComponent$$", reactiveComponentDeclaration );
            var source = GetReplacerForComponentInfo( context, eventComponent ).Replace( sourceTemplate );
            context.AddSource(
                $"{eventComponent.ComponentName}_{eventComponent.EventSystemClassName}_Jobs.ReactiveEvents.g.cs",
                SourceText.From( source, Encoding.UTF8 ) );
        }

        private Replacer GetReplacerForComponentInfo( GeneratorExecutionContext context,
            EventComponentInfo componentInfo )
        {
            var checkIfChangedBodyInsert = string.Empty;
            for ( int i = 0; i < componentInfo.FieldsToCompareName.Count; i++ ) {
                var check = ReactiveSystemTemplates.GetTemplateForCheckIfChangedInstruction(
                    componentInfo.FieldsToCompareName[i] );
                checkIfChangedBodyInsert += check;
            }

            return new Replacer() {
                usings = GeneratorUtils.GetUsingsInsert(
                    context, componentInfo.DeclaredOn,
                    GetCommonUsings()
                ),
                systemNamespace           = componentInfo.ComponentNamespace,
                checkIfChangedMethodBody  = checkIfChangedBodyInsert,
                systemNameFull            = componentInfo.EventSystemClassNameFull,
                systemName                = componentInfo.EventSystemClassName,
                isTagComponent            = componentInfo.IsTagComponent,
                componentName             = componentInfo.ComponentName,
                componentNameFull         = componentInfo.ComponentNameFull,
                reactiveComponentNameFull = componentInfo.ReactiveComponentNameFull
            };
        }

        private Replacer GetReplacerForEventSystem( GeneratorExecutionContext context, EventSystemInfo eventSystem )
        {
            return new Replacer() {
                usings = GeneratorUtils.GetUsingsInsert(
                    context, eventSystem.ClassSyntax,
                    GetCommonUsings()
                ),
                systemNamespace           = eventSystem.SystemNamespace,
                checkIfChangedMethodBody  = "",
                systemNameFull            = eventSystem.SystemNameFull,
                systemName                = eventSystem.SystemName,
                isTagComponent            = false,
                componentName             = "",
                componentNameFull         = "",
                reactiveComponentNameFull = ""
            };
        }
    }
}