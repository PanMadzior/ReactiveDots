using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReactiveDotsPlugin
{
    public class ReactiveSystemSyntaxReceiver : ISyntaxReceiver
    {
        public List<ReactiveSystemInfo> ReactiveSystems { private set; get; } = new List<ReactiveSystemInfo>();

        public void OnVisitSyntaxNode( SyntaxNode syntaxNode )
        {
            if ( syntaxNode is not ClassDeclarationSyntax classNode )
                return;
            GeneratorUtils.GetAttributes( classNode, "ReactiveSystem", out var attributes );
            if ( attributes.Count > 0 )
                ReactiveSystems.Add( new ReactiveSystemInfo( classNode, attributes ) );
        }
    }
}