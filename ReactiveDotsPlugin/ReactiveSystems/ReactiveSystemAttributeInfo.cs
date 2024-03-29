﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReactiveDotsPlugin
{
    public class ReactiveSystemAttributeInfo
    {
        public AttributeSyntax AttributeSyntax { get; }
        public ClassDeclarationSyntax SystemClassSyntax { get; }

        public string ComponentName { private set; get; }
        public string ComponentNamespace { private set; get; }
        public string ComponentNameFull { private set; get; }

        public string ReactiveComponentName { private set; get; }
        public string ReactiveComponentNamespace { private set; get; }
        public string ReactiveComponentNameFull { private set; get; }

        public List<string> FieldsToCompareName { get; }
        public bool IsTagComponent => FieldsToCompareName.Count == 0;

        public bool IsValid => !string.IsNullOrEmpty( ComponentName ) && !string.IsNullOrEmpty( ReactiveComponentName );

        public ReactiveSystemAttributeInfo( GeneratorExecutionContext context, AttributeSyntax attribute,
            ClassDeclarationSyntax system )
        {
            AttributeSyntax   = attribute;
            SystemClassSyntax = system;
            if ( attribute.ArgumentList != null ) {
                if ( attribute.ArgumentList.Arguments.Count >= 1 )
                    GetComponentInfo( context, attribute.ArgumentList.Arguments[0] );
                if ( attribute.ArgumentList.Arguments.Count >= 2 )
                    GetReactiveComponentInfo( context, attribute.ArgumentList.Arguments[1] );
            }

            FieldsToCompareName = new List<string>(
                GeneratorUtils.GetAttributeArgumentValue( attribute, "FieldNameToCompare", "Value" ).Split( ',' )
                    .Where( ( str ) => !string.IsNullOrEmpty( str ) )
            );
        }

        private void GetComponentInfo( GeneratorExecutionContext context, AttributeArgumentSyntax arg )
        {
            var compNames = GeneratorUtils.GetTypeNamesFromAttributeArgument( context, arg );
            ComponentName      = compNames.Name;
            ComponentNamespace = compNames.NamespaceWithContainingTypes;
            ComponentNameFull  = compNames.FullName;
        }

        private void GetReactiveComponentInfo( GeneratorExecutionContext context, AttributeArgumentSyntax arg )
        {
            var compNames = GeneratorUtils.GetTypeNamesFromAttributeArgument( context, arg );
            ReactiveComponentName      = compNames.Name;
            ReactiveComponentNamespace = compNames.NamespaceWithContainingTypes;
            ReactiveComponentNameFull  = compNames.FullName;
        }
    }
}