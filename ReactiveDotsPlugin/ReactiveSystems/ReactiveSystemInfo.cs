using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReactiveDotsPlugin
{
    public class ReactiveSystemInfo
    {
        public ClassDeclarationSyntax ClassSyntax { private set; get; }
        public List<ReactiveSystemAttributeInfo> ReactiveAttributes { private set; get; }
        
        public string SystemName { private set; get; }
        public string SystemNamespace { private set; get; }
        public string SystemNameFull { private set; get; }
        
        private List<AttributeSyntax> _tempAttributes;

        public ReactiveSystemInfo( ClassDeclarationSyntax classSyntax, List<AttributeSyntax> reactiveAttributes )
        {
            ClassSyntax        = classSyntax;
            ReactiveAttributes = new List<ReactiveSystemAttributeInfo>();
            _tempAttributes    = reactiveAttributes;
            SystemName         = ClassSyntax.Identifier.Text;
            SystemNamespace    = GeneratorUtils.GetNamespaceFor( ClassSyntax );
            SystemNameFull     = GeneratorUtils.GetFullName( ClassSyntax );
        }

        public void UpdateAttributes( GeneratorExecutionContext context )
        {
            foreach ( var reactiveAttribute in _tempAttributes )
                ReactiveAttributes.Add( new ReactiveSystemAttributeInfo( context, reactiveAttribute, ClassSyntax ) );            
        }
    }
}