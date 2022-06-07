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
            // TODO: add support for multi reactive system (now it is only first attribute generation)
            var template = ReactiveSystemTemplates.GetReactiveSystemTemplate();
            var source = template // order matters
                .Replace( "NAMESPACENAME", reactiveSystem.SystemNamespace )
                .Replace( "SYSNAME", reactiveSystem.SystemName )
                .Replace( "COMPNAME", reactiveSystem.ReactiveAttributes[0].ComponentName )
                .Replace( "RCOMPONENTFULL", reactiveSystem.ReactiveAttributes[0].ReactiveComponentNameFull )
                .Replace( "COMPONENTFULL", reactiveSystem.ReactiveAttributes[0].ComponentNameFull )
                .Replace( "VARIABLENAMETOCOMPARE", reactiveSystem.ReactiveAttributes[0].FieldToCompareName );
            context.AddSource( $"{reactiveSystem.SystemName}.Reactive.g.cs", SourceText.From( source, Encoding.UTF8 ) );
        }
    }
}