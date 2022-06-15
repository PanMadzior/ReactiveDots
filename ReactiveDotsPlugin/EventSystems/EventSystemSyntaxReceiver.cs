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
            if ( syntaxNode is ClassDeclarationSyntax classNode ) {
                GeneratorUtils.GetAttributes( classNode, "ReactiveEventSystem", out var attributes );
                if ( attributes.Count > 0 )
                    EventSystems.Add( new EventSystemInfo( classNode ) );
            }

            if ( syntaxNode is StructDeclarationSyntax structNode ) {
                GeneratorUtils.GetAttributes( structNode, "ReactiveEvent", out var attributes );
                foreach ( var attribute in attributes ) {
                    EventComponents.Add( new EventComponentInfo( structNode, attribute ) );
                }
            }
        }
    }
}