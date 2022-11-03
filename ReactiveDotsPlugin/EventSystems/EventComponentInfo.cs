using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReactiveDotsPlugin
{
    public class EventComponentInfo
    {
        public EventType EventType { private set; get; }
        public StructDeclarationSyntax? StructSyntax { get; }
        public TypeDeclarationSyntax DeclaredOn { get; }
        public AttributeSyntax Attribute { get; }

        public string ComponentName { private set; get; }
        public string ComponentNamespace { private set; get; }
        public string ComponentNameFull { private set; get; }

        public string EventSystemClassName { private set; get; }
        public string EventSystemClassNamespace { private set; get; }
        public string EventSystemClassNameFull { private set; get; }

        public List<string> FieldsToCompareName { get; }
        public bool IsTagComponent => FieldsToCompareName.Count == 0;

        public string ReactiveComponentNameFull =>
            $"{ComponentNamespace}.{EventSystemClassName}_{ComponentName}_ReactiveEvents.{ComponentName}Reactive";

        public bool IsReactiveEventFor => StructSyntax == null;

        // [ReactiveEventFor]
        public EventComponentInfo( TypeDeclarationSyntax typeSyntax, AttributeSyntax attribute )
        {
            DeclaredOn = typeSyntax;
            Attribute  = attribute;
            EventType  = EventType.All; // default

            FieldsToCompareName = new List<string>(
                GeneratorUtils.GetAttributeArgumentValue( attribute, "FieldNameToCompare", "Value" ).Split( ',' )
                    .Where( ( str ) => !string.IsNullOrEmpty( str ) )
            );
        }

        // [ReactiveEvent]
        public EventComponentInfo( StructDeclarationSyntax structNode, AttributeSyntax attribute )
        {
            StructSyntax = structNode;
            DeclaredOn   = structNode;
            Attribute    = attribute;
            EventType    = EventType.All; // default

            FieldsToCompareName = new List<string>(
                GeneratorUtils.GetAttributeArgumentValue( attribute, "FieldNameToCompare", "Value" ).Split( ',' )
                    .Where( ( str ) => !string.IsNullOrEmpty( str ) )
            );

            ComponentName      = StructSyntax.Identifier.Text;
            ComponentNamespace = GeneratorUtils.GetNamespaceFor( StructSyntax );
            ComponentNameFull  = GeneratorUtils.GetFullName( StructSyntax );
        }

        public void UpdateWithContext( GeneratorExecutionContext context )
        {
            var componentTypeIndex = 0;
            var eventTypeIndex     = IsReactiveEventFor ? 1 : 0;
            var eventSystemIndex   = IsReactiveEventFor ? 2 : 1;
            if ( Attribute.ArgumentList != null ) {
                if ( IsReactiveEventFor && Attribute.ArgumentList.Arguments.Count >= 1 )
                    GetComponentInfo( context, Attribute.ArgumentList.Arguments[componentTypeIndex] );

                if ( Attribute.ArgumentList.Arguments.Count >= eventTypeIndex + 1 )
                    GetEventTypeInfo( Attribute, eventTypeIndex );

                if ( Attribute.ArgumentList.Arguments.Count >= eventSystemIndex + 1 )
                    GetEventSystemInfo( context, Attribute.ArgumentList.Arguments[eventSystemIndex] );
                else {
                    EventSystemClassNamespace = "ReactiveDots";
                    EventSystemClassNameFull  = "ReactiveDots.DefaultEventSystem";
                    EventSystemClassName      = "DefaultEventSystem";
                }
            }
        }

        private void GetComponentInfo( GeneratorExecutionContext context, AttributeArgumentSyntax arg )
        {
            var compNames = GeneratorUtils.GetTypeNamesFromAttributeArgument( context, arg );
            ComponentName      = compNames.Name;
            ComponentNamespace = compNames.NamespaceWithContainingTypes;
            ComponentNameFull  = compNames.FullName;
        }

        private void GetEventTypeInfo( AttributeSyntax attribute, int index )
        {
            var eventTypeStr = GeneratorUtils.GetAttributeArgumentValue( attribute, index, "EventType.All" );
            switch ( eventTypeStr ) {
                case "EventType.All":
                    EventType = EventType.All;
                    break;
                case "EventType.Added":
                    EventType = EventType.Added;
                    break;
                case "EventType.Removed":
                    EventType = EventType.Removed;
                    break;
                case "EventType.Changed":
                    EventType = EventType.Changed;
                    break;
            }
        }

        private void GetEventSystemInfo( GeneratorExecutionContext context, AttributeArgumentSyntax arg )
        {
            var compNames = GeneratorUtils.GetTypeNamesFromAttributeArgument( context, arg );
            EventSystemClassName      = compNames.Name;
            EventSystemClassNamespace = compNames.NamespaceWithContainingTypes;
            EventSystemClassNameFull  = compNames.FullName;
        }
    }
}