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
        public string ComponentNamespace { private set; get; }
        public StructDeclarationSyntax ComponentStructSyntax { private set; get; }

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
                    GetComponentInfo( context, attribute, system );
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
            if ( GeneratorUtils.TryFindStructSyntaxByName( context, ReactiveComponentName,
                    out var structSyntax ) ) {
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

        private void GetComponentInfo( GeneratorExecutionContext context, AttributeSyntax attribute,
            ClassDeclarationSyntax system )
        {
            ComponentName =
                GeneratorUtils.GetTypeofFromAttributeArgument( attribute.ArgumentList.Arguments[0] );
            if ( GeneratorUtils.TryFindStructSyntaxByName( context, ComponentName, out var structSyntax ) ) {
                ComponentStructSyntax = structSyntax;
                ComponentNamespace    = GeneratorUtils.GetNamespaceFor( structSyntax );
                ComponentNameFull     = GeneratorUtils.GetFullName( structSyntax );
            }
            else {
                ComponentNamespace = GeneratorUtils.GetNamespaceFor( system );
                ComponentNameFull = string.IsNullOrEmpty( ComponentNamespace )
                    ? ComponentName
                    : ComponentNamespace + "." + ComponentName;
            }
        }
    }
}