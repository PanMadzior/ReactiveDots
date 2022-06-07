using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace ReactiveDotsPlugin
{
    //[Generator]
    public class ReactiveSystemGeneratorOld// : ISourceGenerator
    {
        private const string AttributeName = "ReactiveSystem";

        public void Initialize( GeneratorInitializationContext context ) { }

        public void Execute( GeneratorExecutionContext context )
        {
            try {
                var classesWithAttribute = context.Compilation.SyntaxTrees
                    .SelectMany( st => st
                        .GetRoot()
                        .DescendantNodes()
                        .OfType<ClassDeclarationSyntax>()
                        .Where( r => r.AttributeLists
                            .SelectMany( al => al.Attributes )
                            .Any( a => a.Name.GetText().ToString() == AttributeName ) ) );
                foreach ( var cds in classesWithAttribute ) {
                    var reactiveAttributes = cds.AttributeLists
                        .SelectMany( al => al.Attributes )
                        .Where( a => a.Name.GetText().ToString() == AttributeName )
                        .ToList();
                    foreach ( var reactiveAttribute in reactiveAttributes ) {
                        var namespaceName = GeneratorUtilsOld.GetNamespaceFrom( cds );
                        var systemName    = cds.Identifier.Text;

                        var compTypeName     = GetComponentTypeString( reactiveAttribute );
                        var compTypeNameFull = $"{namespaceName}.{compTypeName}";
                        if ( GeneratorUtilsOld.FindStruct( context, compTypeName, out var compSyntax ) )
                            compTypeNameFull = GeneratorUtilsOld.GetFullName( compSyntax );

                        var reactiveTypeName     = GetReactiveComponentTypeString( reactiveAttribute );
                        var reactiveTypeNameFull = $"{namespaceName}.{systemName}.{reactiveTypeName}";
                        // TODO: looking for struct syntax crashes unity compiler and rcomp have to be in class
                        //if ( GeneratorUtils.FindStruct( context, reactiveTypeName, out var rCompSyntax ) )
                            //reactiveTypeNameFull = GeneratorUtils.GetFullName( rCompSyntax );

                        var fieldName = GetFieldNameToCompareString( reactiveAttribute );

                        var template = GetSourceString();
                        var source = template
                            .Replace( "NAMESPACENAME", namespaceName )
                            .Replace( "SYSNAME", systemName )
                            .Replace( "COMPNAME", compTypeName )
                            .Replace( "RCOMPONENTFULL", reactiveTypeNameFull )
                            .Replace( "COMPONENTFULL", compTypeNameFull )
                            .Replace( "VARIABLENAMETOCOMPARE", fieldName ); // order matters

                        context.AddSource( $"{systemName}.Reactive.Gen.cs", SourceText.From( source, Encoding.UTF8 ) );
                    }
                }
            }
            catch ( Exception e ) {
                CreateExceptionClass( context, e );
            }
        }

        private static void CreateExceptionClass( GeneratorExecutionContext context, Exception e )
        {
            context.AddSource( $"ReactiveSystemGenerationException.Gen.cs",
                SourceText.From(
                    $"namespace Game {{ public class ReactiveSystemGenerationException {{ " +
                    $"/* {e.ToString()} */" +
                    $" }} }}", Encoding.UTF8 )
            );
        }

        private static string GetComponentTypeString( AttributeSyntax reactiveAttribute )
        {
            var componentTypeArgument = reactiveAttribute.ArgumentList!.Arguments[0];
            var expression            = componentTypeArgument.Expression.ToFullString();
            var compTypeName          = expression.Replace( "typeof(", "" ).Replace( ")", "" ).Trim();
            return compTypeName;
        }

        private static string GetReactiveComponentTypeString( AttributeSyntax reactiveAttribute )
        {
            var componentTypeArgument = reactiveAttribute.ArgumentList!.Arguments[1];
            var expression            = componentTypeArgument.Expression.ToFullString();
            var compTypeName          = expression.Replace( "typeof(", "" ).Replace( ")", "" ).Trim();
            return compTypeName;
        }

        private static string GetFieldNameToCompareString( AttributeSyntax reactiveAttribute )
        {
            var fieldArgument = reactiveAttribute.ArgumentList!.Arguments
                .FirstOrDefault( arg =>
                {
                    if ( arg == null || arg.NameEquals == null )
                        return false;
                    return arg.NameEquals.Name.Identifier.ValueText.Equals( "FieldNameToCompare" );
                } );
            var fieldName = "Value";
            if ( fieldArgument != null )
                fieldName = fieldArgument.Expression.NormalizeWhitespace().ToFullString()
                    .Replace( "\"", "" );
            return fieldName;
        }

        private string GetSourceString()
        {
            return @"// Auto Generated Code
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using ReactiveDots;

namespace NAMESPACENAME
{
    public static class SYSNAME_Reactive
    {
        private struct InstanceData
        {
            public EntityQuery addedQuery;
            public EntityQuery removedQuery;
            public EntityQuery changedQuery;
        }

        private static Dictionary<SYSNAME, InstanceData> Instances =
            new Dictionary<SYSNAME, InstanceData>();

        private static InstanceData GetOrCreateInstanceData( SYSNAME sys )
        {
            if ( !Instances.ContainsKey( sys ) )
                Instances.Add( sys, CreateInstanceData( sys ) );
            return Instances[sys];
        }

        private static InstanceData CreateInstanceData( SYSNAME sys )
        {
            var data = new InstanceData();
            data.addedQuery = sys.EntityManager.CreateEntityQuery(
                ComponentType.ReadOnly<COMPONENTFULL>(),
                ComponentType.Exclude<RCOMPONENTFULL>()
            );
            data.removedQuery = sys.EntityManager.CreateEntityQuery(
                ComponentType.Exclude<COMPONENTFULL>(),
                ComponentType.ReadWrite<RCOMPONENTFULL>()
            );
            data.changedQuery = sys.EntityManager.CreateEntityQuery(
                ComponentType.ReadOnly<COMPONENTFULL>(),
                ComponentType.ReadWrite<RCOMPONENTFULL>()
            );
            return data;
        }

        public static Unity.Jobs.JobHandle UpdateReactive( this SYSNAME sys,
            Unity.Jobs.JobHandle dependency )
        {
            var instanceData = GetOrCreateInstanceData( sys );
            UpdateAdded( sys, instanceData );
            UpdateRemoved( sys, instanceData );
            return GetReactiveUpdateJob( sys, out var query ).ScheduleParallel( query, dependency );
        }

        private static void UpdateAdded( SYSNAME sys, InstanceData instanceData )
        {
            var addedEntities = instanceData.addedQuery.ToEntityArray( Allocator.Temp );
            foreach ( var e in addedEntities ) {
                sys.EntityManager.AddComponentData( e, new RCOMPONENTFULL() {
                    Value = new ComponentReactiveData<COMPONENTFULL>() {
                        PreviousValue        = sys.EntityManager.GetComponentData<COMPONENTFULL>( e ),
                        Added                = true,
                        Changed              = true,
                        Removed              = false,
                        _FirstCheckCompleted = false
                    }
                } );
            }

            addedEntities.Dispose();
        }

        private static void UpdateRemoved( SYSNAME sys, InstanceData instanceData )
        {
            var removedEntities = instanceData.removedQuery.ToEntityArray( Allocator.Temp );
            foreach ( var e in removedEntities ) {
                var rComp = sys.EntityManager.GetComponentData<RCOMPONENTFULL>( e );
                var rCompData = rComp.Value;
                if ( rCompData.Removed )
                    sys.EntityManager.RemoveComponent<RCOMPONENTFULL>( e );
                else {
                    rCompData.Added   = false;
                    rCompData.Changed = false;
                    rCompData.Removed = true;
                    rComp.Value       = rCompData;
                }
            }

            removedEntities.Dispose();
        }

        private static ReactiveUpdateJob GetReactiveUpdateJob( this SYSNAME sys,
            out EntityQuery query )
        {
            var instanceData = GetOrCreateInstanceData( sys );
            query = instanceData.changedQuery;
            return new ReactiveUpdateJob() {
                compHandle  = sys.GetComponentTypeHandle<COMPONENTFULL>( true ),
                rCompHandle = sys.GetComponentTypeHandle<RCOMPONENTFULL>( false )
            };
        }

        [Unity.Burst.BurstCompile]
        private struct ReactiveUpdateJob : IJobEntityBatch
        {
            [ReadOnly]
            public ComponentTypeHandle<COMPONENTFULL> compHandle;
            public ComponentTypeHandle<RCOMPONENTFULL> rCompHandle;

            public void Execute( ArchetypeChunk batchInChunk, int batchIndex )
            {
                var compsArrayPtr =
                    InternalCompilerInterface.UnsafeGetChunkNativeArrayReadOnlyIntPtr( batchInChunk, compHandle );
                var rCompArrayPtr =
                    InternalCompilerInterface.UnsafeGetChunkNativeArrayIntPtr( batchInChunk, rCompHandle );
                for ( int i = 0; i < batchInChunk.Count; i++ ) {
                    Execute(
                        ref InternalCompilerInterface
                            .UnsafeGetRefToNativeArrayPtrElement<COMPONENTFULL>( compsArrayPtr, i ),
                        ref InternalCompilerInterface
                            .UnsafeGetRefToNativeArrayPtrElement<RCOMPONENTFULL>(
                                rCompArrayPtr, i ) 
                        );
                }
            }

            private static void Execute( ref COMPONENTFULL comp, ref RCOMPONENTFULL rComp )
            {
                rComp.Value = new ComponentReactiveData<COMPONENTFULL>() {
                    PreviousValue        = comp,
                    Added                = !rComp.Value._FirstCheckCompleted,
                    Changed              = rComp.Value.Added || BooleanSimplifier.Any( comp.VARIABLENAMETOCOMPARE != rComp.Value.PreviousValue.VARIABLENAMETOCOMPARE ),
                    Removed              = false,
                    _FirstCheckCompleted = true
                };
            }
        }
    }
}
";
        }
    }
}