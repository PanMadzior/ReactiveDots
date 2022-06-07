using System;
using System.Diagnostics.Contracts;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReactiveDotsPlugin
{
    public static class GeneratorUtilsOld
    {
        public const string NESTED_CLASS_DELIMITER    = "+";
        public const string NAMESPACE_CLASS_DELIMITER = ".";

        public static string GetNamespaceFrom( SyntaxNode s )
        {
            return s.Parent switch {
                NamespaceDeclarationSyntax namespaceDeclarationSyntax => namespaceDeclarationSyntax.Name.ToString(),
                null => string.Empty, // or whatever you want to do
                _ => GetNamespaceFrom( s.Parent )
            };
        }

        public static bool FindStruct( GeneratorExecutionContext context, string structName,
            out StructDeclarationSyntax structSyntax )
        {
            structSyntax = context.Compilation.SyntaxTrees
                .SelectMany( st => st
                    .GetRoot()
                    .DescendantNodes()
                    .OfType<StructDeclarationSyntax>() )
                .FirstOrDefault( str => str.Identifier.Text.Equals( structName ) );
            return structSyntax != null;
        }

        // https://stackoverflow.com/a/52694992
        public static string GetFullName( StructDeclarationSyntax source )
        {
            Contract.Requires( null != source );

            var items  = new List<string>();
            var parent = source.Parent;
            while ( parent.IsKind( SyntaxKind.ClassDeclaration ) ) {
                var parentClass = parent as StructDeclarationSyntax;
                Contract.Assert( null != parentClass );
                items.Add( parentClass.Identifier.Text );

                parent = parent.Parent;
            }

            var nameSpace = parent as NamespaceDeclarationSyntax;
            Contract.Assert( null != nameSpace );
            var sb = new StringBuilder().Append( nameSpace.Name ).Append( NAMESPACE_CLASS_DELIMITER );
            items.Reverse();
            items.ForEach( i => { sb.Append( i ).Append( NESTED_CLASS_DELIMITER ); } );
            sb.Append( source.Identifier.Text );

            var result = sb.ToString();
            return result;
        }

        public static string GetFullName( ClassDeclarationSyntax source )
        {
            Contract.Requires( null != source );

            var items  = new List<string>();
            var parent = source.Parent;
            while ( parent.IsKind( SyntaxKind.ClassDeclaration ) ) {
                var parentClass = parent as ClassDeclarationSyntax;
                Contract.Assert( null != parentClass );
                items.Add( parentClass.Identifier.Text );

                parent = parent.Parent;
            }

            var nameSpace = parent as NamespaceDeclarationSyntax;
            Contract.Assert( null != nameSpace );
            var sb = new StringBuilder().Append( nameSpace.Name ).Append( NAMESPACE_CLASS_DELIMITER );
            items.Reverse();
            items.ForEach( i => { sb.Append( i ).Append( NESTED_CLASS_DELIMITER ); } );
            sb.Append( source.Identifier.Text );

            var result = sb.ToString();
            return result;
        }
    }
}