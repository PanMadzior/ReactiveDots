using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReactiveDotsPlugin
{
    public class EventSystemSyntaxReceiver : ISyntaxReceiver
    {
        public List<EventSystemInfo> EventSystems { private set; get; } = new List<EventSystemInfo>();
        public List<EventComponentInfo> EventComponents { private set; get; } = new List<EventComponentInfo>();

        public void OnVisitSyntaxNode( SyntaxNode syntaxNode )
        {
            // Look for classes with [ReactiveEventSystem]
            CheckIfReactiveEventSystem( syntaxNode );

            // Look for structs with [ReactiveEvent]
            CheckIfReactiveEventComponent( syntaxNode );

            // Look for types with [ReactiveEventFor]
            CheckIfReactiveEventForComponent( syntaxNode );
        }

        private void CheckIfReactiveEventSystem( SyntaxNode syntaxNode )
        {
            if ( syntaxNode is ClassDeclarationSyntax classNode ) {
                GeneratorUtils.GetAttributes( classNode, "ReactiveEventSystem", out var attributes );
                if ( attributes.Count > 0 )
                    EventSystems.Add( new EventSystemInfo( classNode ) );
            }
        }

        private void CheckIfReactiveEventComponent( SyntaxNode syntaxNode )
        {
            if ( syntaxNode is StructDeclarationSyntax structNode ) {
                GeneratorUtils.GetAttributes( structNode, "ReactiveEvent", out var attributes );
                foreach ( var attribute in attributes ) {
                    EventComponents.Add( new EventComponentInfo( structNode, attribute ) );
                }
            }
        }

        private void CheckIfReactiveEventForComponent( SyntaxNode syntaxNode )
        {
            if ( syntaxNode is TypeDeclarationSyntax typeNode ) {
                GeneratorUtils.GetAttributes( typeNode, "ReactiveEventFor", out var attributes );
                foreach ( var attribute in attributes ) {
                    EventComponents.Add( new EventComponentInfo( typeNode, attribute ) );
                }
            }
        }
    }
}