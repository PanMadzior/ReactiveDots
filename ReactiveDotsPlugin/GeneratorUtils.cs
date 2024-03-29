﻿using System.Diagnostics.Contracts;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReactiveDotsPlugin
{
    public static class GeneratorUtils
    {
        // https://stackoverflow.com/a/52694992
        public static string GetFullName( TypeDeclarationSyntax source )
        {
            Contract.Requires( null != source );

            var items  = new List<string>();
            var parent = source.Parent;
            while ( parent.IsKind( SyntaxKind.ClassDeclaration ) ) {
                var parentClass = parent as TypeDeclarationSyntax;
                Contract.Assert( null != parentClass );
                items.Add( parentClass.Identifier.Text );

                parent = parent.Parent;
            }

            var nameSpace = parent as NamespaceDeclarationSyntax;
            Contract.Assert( null != nameSpace );
            var sb = new StringBuilder().Append( nameSpace.Name ).Append( '.' );
            items.Reverse();
            items.ForEach( i => { sb.Append( i ).Append( '.' ); } );
            sb.Append( source.Identifier.Text );

            var result = sb.ToString();
            return result;
        }

        public static bool TryFindStructSyntaxByName( GeneratorExecutionContext context, string structName,
            out StructDeclarationSyntax? structSyntax, TypeDeclarationSyntax optionalPreferredParent = null )
        {
            var allFound = context.Compilation.SyntaxTrees
                .SelectMany( st => st
                    .GetRoot()
                    .DescendantNodes()
                    .OfType<StructDeclarationSyntax>() )
                .Where( str => str.Identifier.Text.Equals( structName ) )
                .ToList();
            if ( allFound.Count == 0 )
                structSyntax = null;
            else if ( allFound.Count == 1 || optionalPreferredParent == null )
                structSyntax = allFound[0];
            else {
                structSyntax = allFound.FirstOrDefault( s =>
                {
                    var parentSyntax = s.Parent as TypeDeclarationSyntax;
                    if ( parentSyntax == null )
                        return false;
                    return parentSyntax.Identifier.Text.Equals( optionalPreferredParent.Identifier.Text );
                } );
                if ( structSyntax == null )
                    structSyntax = allFound[0];
            }

            return structSyntax != null;
        }

        public static bool TryFindClassSyntaxByName( GeneratorExecutionContext context, string className,
            out ClassDeclarationSyntax? classSyntax )
        {
            classSyntax = context.Compilation.SyntaxTrees
                .SelectMany( st => st
                    .GetRoot()
                    .DescendantNodes()
                    .OfType<ClassDeclarationSyntax>() )
                .FirstOrDefault( str => str.Identifier.Text.Equals( className ) ) ?? null;
            return classSyntax != null;
        }

        public static string GetNamespaceFor( SyntaxNode s )
        {
            return s.Parent switch {
                NamespaceDeclarationSyntax namespaceDeclarationSyntax => namespaceDeclarationSyntax.Name.ToString(),
                null => string.Empty,
                _ => GetNamespaceFor( s.Parent )
            };
        }

        public static void GetAttributes( BaseTypeDeclarationSyntax typeSyntax, string attributeName,
            out List<AttributeSyntax> output )
        {
            output = new List<AttributeSyntax>();
            output.AddRange(
                from attributeList in typeSyntax.AttributeLists
                from attribute in attributeList.Attributes
                where attribute.Name.ToString() == attributeName
                select attribute
            );
        }

        public static string GetTypeofFromAttributeArgument( AttributeArgumentSyntax attributeArgument )
        {
            var expression   = attributeArgument.Expression.ToFullString();
            var compTypeName = expression.Replace( "typeof(", "" ).Replace( ")", "" ).Trim();
            return compTypeName;
        }

        public static string GetAttributeArgumentValue( AttributeSyntax attribute, string argumentName,
            string defaultValue )
        {
            var argument = attribute.ArgumentList!.Arguments
                .FirstOrDefault( arg =>
                {
                    if ( arg == null || arg.NameEquals == null )
                        return false;
                    return arg.NameEquals.Name.Identifier.ValueText.Equals( argumentName );
                } );
            var argumentValue = "Value";
            if ( argument != null )
                argumentValue = argument.Expression.NormalizeWhitespace().ToFullString()
                    .Replace( "\"", "" );
            return argumentValue;
        }

        public static string GetAttributeArgumentValue( AttributeSyntax attribute, int argumentIndex,
            string defaultValue )
        {
            if ( attribute.ArgumentList == null || attribute.ArgumentList.Arguments.Count <= argumentIndex )
                return string.Empty;
            var argument      = attribute.ArgumentList!.Arguments[argumentIndex];
            var argumentValue = string.Empty;
            if ( argument != null )
                argumentValue = argument.Expression.NormalizeWhitespace().ToFullString()
                    .Replace( "\"", "" );
            return argumentValue;
        }

        public static void PopulateSetWithUniqueUsings( HashSet<string> set, TypeDeclarationSyntax type,
            GeneratorExecutionContext context )
        {
            var usingsForType = type.SyntaxTree.GetCompilationUnitRoot( context.CancellationToken ).Usings;
            foreach ( var usingForType in usingsForType ) {
                var str = usingForType.Name.ToString();
                if ( set.Contains( str ) )
                    continue;
                set.Add( str );
            }
        }

        public static string GetUsingsInsert( GeneratorExecutionContext context, TypeDeclarationSyntax type,
            HashSet<string>? additional = null )
        {
            var usings = additional == null ? new HashSet<string>() : new HashSet<string>( additional );
            GeneratorUtils.PopulateSetWithUniqueUsings( usings, type, context );
            var usingsInsert = string.Empty;
            foreach ( var u in usings )
                usingsInsert += "using " + u + ";\n";
            return usingsInsert;
        }

        public static void SplitTypeName( string full, out string visibleNamespace, out string name )
        {
            var split = full.Split( '.' );
            name             = split[split.Length - 1];
            visibleNamespace = string.Empty;
            if ( split.Length >= 2 ) {
                for ( int i = 0; i < split.Length - 1; i++ )
                    visibleNamespace += split[i] + ( i < split.Length - 2 ? "." : "" );
            }
        }

        public struct TypeNames
        {
            public string? Name;
            public string? NamespaceWithContainingTypes;
            public string? FullName;
        }

        public static TypeNames GetTypeNamesFromAttributeArgument( GeneratorExecutionContext context,
            AttributeArgumentSyntax arg )
        {
            var typeofExpr = arg.Expression as TypeOfExpressionSyntax;
            if ( typeofExpr == null )
                return default(TypeNames);
            return GetTypeNamesFromTypeofDeclaration( context, typeofExpr );
        }

        public static TypeNames GetTypeNamesFromTypeofDeclaration( GeneratorExecutionContext context,
            TypeOfExpressionSyntax expr )
        {
            var typeNames     = new TypeNames();
            var typeInfo      = context.Compilation.GetSemanticModel( expr.Type.SyntaxTree ).GetTypeInfo( expr.Type );
            var namedSymbol   = typeInfo.Type as INamedTypeSymbol;
            var typeNamespace = namedSymbol?.ContainingNamespace.ToString();
            var typeParents   = string.Empty;
            var parent        = namedSymbol?.ContainingType;
            while ( parent != null ) {
                typeParents += parent.Name;
                parent      =  parent.ContainingType;
                if ( parent != null )
                    typeParents += ".";
            }

            // type name
            typeNames.Name = namedSymbol?.Name;

            // type path (namespace + containing types)
            typeNames.NamespaceWithContainingTypes =
                ( string.IsNullOrEmpty( typeNamespace ) ? "" : typeNamespace + "." )
                + ( string.IsNullOrEmpty( typeParents ) ? "" : typeParents + "." );
            if ( typeNames.NamespaceWithContainingTypes.EndsWith( "." ) )
                typeNames.NamespaceWithContainingTypes =
                    typeNames.NamespaceWithContainingTypes.Substring( 0,
                        typeNames.NamespaceWithContainingTypes.Length - 1 );

            // type path + type name
            typeNames.FullName = ( string.IsNullOrEmpty( typeNames.NamespaceWithContainingTypes )
                ? typeNames.Name
                : typeNames.NamespaceWithContainingTypes + "." + typeNames.Name );

            return typeNames;
        }
    }
}