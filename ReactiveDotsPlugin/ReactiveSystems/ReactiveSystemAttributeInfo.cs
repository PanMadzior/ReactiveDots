using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReactiveDotsPlugin
{
    public class ReactiveSystemAttributeInfo
    {
        public AttributeSyntax AttributeSyntax { private set; get; }
        public ClassDeclarationSyntax SystemClassSyntax { private set; get; }

        public string ComponentName { private set; get; }
        public string ComponentNameFull { private set; get; }
        public string ComponentVisibleNamespace { private set; get; }

        public string ReactiveComponentName { private set; get; }
        public string ReactiveComponentNameFull { private set; get; }
        public string ReactiveComponentNamespace { private set; get; }
        public StructDeclarationSyntax ReactiveComponentStructSyntax { private set; get; }

        public string FieldToCompareName { private set; get; }

        public bool IsValid => !string.IsNullOrEmpty( ComponentName ) && !string.IsNullOrEmpty( ReactiveComponentName );

        public ReactiveSystemAttributeInfo( GeneratorExecutionContext context, AttributeSyntax attribute,
            ClassDeclarationSyntax system )
        {
            AttributeSyntax   = attribute;
            SystemClassSyntax = system;
            if ( attribute.ArgumentList != null ) {
                if ( attribute.ArgumentList.Arguments.Count >= 1 )
                    GetComponentInfo( attribute );
                if ( attribute.ArgumentList.Arguments.Count >= 2 )
                    GetReactiveComponentInfo( context, attribute, system );
            }

            FieldToCompareName = GeneratorUtils.GetAttributeArgumentValue( attribute, "FieldNameToCompare", "Value" );
        }

        private void GetReactiveComponentInfo( GeneratorExecutionContext context, AttributeSyntax attribute,
            ClassDeclarationSyntax system )
        {
            ReactiveComponentName =
                GeneratorUtils.GetTypeofFromAttributeArgument( attribute.ArgumentList.Arguments[1] );
            if ( GeneratorUtils.TryFindStructSyntaxByName( context, ReactiveComponentName, out var structSyntax,
                    system ) ) {
                ReactiveComponentStructSyntax = structSyntax;
                ReactiveComponentNamespace    = GeneratorUtils.GetNamespaceFor( structSyntax );
                ReactiveComponentNameFull     = GeneratorUtils.GetFullName( structSyntax );
            }
            else {
                ReactiveComponentNamespace = GeneratorUtils.GetNamespaceFor( system );
                ReactiveComponentNameFull = string.IsNullOrEmpty( ReactiveComponentNamespace )
                    ? ReactiveComponentName
                    : ReactiveComponentNamespace + "." + ReactiveComponentName;
            }
        }

        private void GetComponentInfo( AttributeSyntax attribute )
        {
            ComponentNameFull =
                GeneratorUtils.GetTypeofFromAttributeArgument( attribute.ArgumentList.Arguments[0] );
            var split = ComponentNameFull.Split( '.' );
            ComponentName             = split[split.Length - 1];
            ComponentVisibleNamespace = string.Empty;
            if ( split.Length >= 2 ) {
                for ( int i = 0; i < split.Length - 1; i++ )
                    ComponentVisibleNamespace += split[i] + ( i < split.Length - 2 ? "." : "" );
            }
        }
    }
}