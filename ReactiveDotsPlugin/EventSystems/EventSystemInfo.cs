using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReactiveDotsPlugin
{
    public class EventSystemInfo
    {
        public ClassDeclarationSyntax ClassSyntax { private set; get; }

        public string SystemName { private set; get; }
        public string SystemNamespace { private set; get; }
        public string SystemNameFull { private set; get; }

        private List<AttributeSyntax> _tempAttributes;

        public EventSystemInfo( ClassDeclarationSyntax classSyntax )
        {
            ClassSyntax     = classSyntax;
            SystemName      = ClassSyntax.Identifier.Text;
            SystemNamespace = GeneratorUtils.GetNamespaceFor( ClassSyntax );
            SystemNameFull  = GeneratorUtils.GetFullName( ClassSyntax );
        }
    }
}