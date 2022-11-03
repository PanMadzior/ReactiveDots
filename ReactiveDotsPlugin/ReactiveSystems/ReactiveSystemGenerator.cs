using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace ReactiveDotsPlugin
{
    [Generator]
    public class ReactiveSystemGenerator : SourceGeneratorBase
    {
        public override void Initialize( GeneratorInitializationContext context )
        {
            context.RegisterForSyntaxNotifications( () => new ReactiveSystemSyntaxReceiver() );
        }

        public override void Execute( GeneratorExecutionContext context )
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
            var replacer = new Replacer() {
                usings = GeneratorUtils.GetUsingsInsert(
                    context, reactiveSystem.ClassSyntax,
                    GetCommonUsings()
                ),
                systemNamespace           = reactiveSystem.SystemNamespace,
                checkIfChangedMethodBody  = "",
                systemNameFull            = reactiveSystem.SystemNameFull,
                systemName                = reactiveSystem.SystemName,
                isTagComponent            = false,
                componentName             = "",
                componentNameFull         = "",
                reactiveComponentNameFull = ""
            };

            var globalTemplate                                       = ReactiveSystemTemplates.GetGlobalTemplate();
            var reactiveUpdatesChangedInsert                         = string.Empty;
            var reactiveUpdatesRemovedInsert                         = string.Empty;
            var reactiveAddMissingReactiveDataInsert_WithoutEcb      = string.Empty;
            var reactiveAddMissingReactiveDataInsert_WithTempEcb     = string.Empty;
            var reactiveAddMissingReactiveDataInsert_WithExternalEcb = string.Empty;
            var reactiveComponentsInsert                             = string.Empty;
            for ( int i = 0; i < reactiveSystem.ReactiveAttributes.Count; i++ ) {
                if ( !reactiveSystem.ReactiveAttributes[i].IsValid )
                    continue;
                replacer = UpdateReplacerWithComponentInfo( replacer, reactiveSystem.ReactiveAttributes[i] );
                reactiveAddMissingReactiveDataInsert_WithoutEcb += "\n" + replacer.Replace(
                    ReactiveSystemTemplates.GetTemplateForSystemAddMissingReactiveData_WithoutEcb() );
                reactiveAddMissingReactiveDataInsert_WithTempEcb += "\n" + replacer.Replace(
                    ReactiveSystemTemplates.GetTemplateForSystemAddMissingReactiveData_WithTempEcb() );
                reactiveAddMissingReactiveDataInsert_WithExternalEcb += "\n" + replacer.Replace(
                    ReactiveSystemTemplates.GetTemplateForSystemAddMissingReactiveData_WithExternalEcb() );
                reactiveUpdatesChangedInsert += "\n" + replacer.Replace(
                    ReactiveSystemTemplates.GetTemplateForChangedOrAddedCheck() );
                reactiveUpdatesRemovedInsert += "\n" + replacer.Replace(
                    ReactiveSystemTemplates.GetTemplateForRemovedCheck() );
                reactiveComponentsInsert +=
                    "\n" + replacer.Replace( ReactiveSystemTemplates.GetTemplateForComponent() );
            }

            var source = replacer.Replace( globalTemplate )
                .Replace( "$$placeForAddMissingReactiveData_WithoutEcb$$",
                    reactiveAddMissingReactiveDataInsert_WithoutEcb )
                .Replace( "$$placeForAddMissingReactiveData_WithTempEcb$$",
                    reactiveAddMissingReactiveDataInsert_WithTempEcb )
                .Replace( "$$placeForAddMissingReactiveData_WithExternalEcb$$",
                    reactiveAddMissingReactiveDataInsert_WithExternalEcb )
                .Replace( "$$placeForUpdatesChanged$$", reactiveUpdatesChangedInsert )
                .Replace( "$$placeForUpdatesRemoved$$", reactiveUpdatesRemovedInsert )
                .Replace( "$$placeForComponents$$", reactiveComponentsInsert );
            context.AddSource( $"{reactiveSystem.SystemName}.Reactive.g.cs", SourceText.From( source, Encoding.UTF8 ) );
        }

        private Replacer UpdateReplacerWithComponentInfo( Replacer replacer, ReactiveSystemAttributeInfo attribute )
        {
            var checkIfChangedBodyInsert = string.Empty;

            for ( int i = 0; i < attribute.FieldsToCompareName.Count; i++ ) {
                var check = ReactiveSystemTemplates.GetTemplateForCheckIfChangedInstruction(
                    attribute.FieldsToCompareName[i] );
                checkIfChangedBodyInsert += check;
            }

            replacer.checkIfChangedMethodBody  = checkIfChangedBodyInsert;
            replacer.isTagComponent            = attribute.IsTagComponent;
            replacer.componentNameFull         = attribute.ComponentNameFull;
            replacer.componentName             = attribute.ComponentName;
            replacer.reactiveComponentNameFull = attribute.ReactiveComponentNameFull;

            return replacer;
        }
    }
}