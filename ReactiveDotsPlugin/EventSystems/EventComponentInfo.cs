using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReactiveDotsPlugin
{
    public class EventComponentInfo
    {
        public EventType EventType { private set; get; }
        public StructDeclarationSyntax? StructSyntax { private set; get; }
        public TypeDeclarationSyntax DeclaredOn { private set; get; }
        public AttributeSyntax Attribute { private set; get; }

        public string ComponentName { private set; get; }
        public string ComponentVisibleNamespace { private set; get; }
        public string ComponentNameFull { private set; get; }

        public string EventSystemClassName { private set; get; }
        public string EventSystemClassNamespace { private set; get; }
        public string EventSystemClassNameFull { private set; get; }

        public string FieldToCompareName { private set; get; }
        public string ReactiveComponentNameFull =>
            $"{ComponentVisibleNamespace}.{EventSystemClassName}_{ComponentName}_ReactiveEvents.{ComponentName}Reactive";

        // [ReactiveEventFor]
        public EventComponentInfo( TypeDeclarationSyntax typeSyntax, AttributeSyntax attribute )
        {
            DeclaredOn = typeSyntax;
            Attribute  = attribute;
            EventType  = EventType.All; // default

            ComponentNameFull = GeneratorUtils.GetTypeofFromAttributeArgument( attribute.ArgumentList.Arguments[0] );
            GeneratorUtils.SplitTypeName( ComponentNameFull, out var visibleNamespace, out var componentName );
            ComponentVisibleNamespace = visibleNamespace;
            ComponentName             = componentName;

            if ( Attribute.ArgumentList != null ) {
                if ( Attribute.ArgumentList.Arguments.Count >= 2 )
                    GetEventTypeInfo( Attribute, 1 );
                if ( Attribute.ArgumentList.Arguments.Count >= 3 )
                    GetEventSystemInfo( Attribute.ArgumentList.Arguments[2] );
                else {
                    EventSystemClassNamespace = "ReactiveDots";
                    EventSystemClassNameFull  = "ReactiveDots.DefaultEventSystem";
                    EventSystemClassName      = "DefaultEventSystem";
                }
            }

            FieldToCompareName = GeneratorUtils.GetAttributeArgumentValue( Attribute, "FieldNameToCompare", "Value" );
        }

        // [ReactiveEvent]
        public EventComponentInfo( StructDeclarationSyntax structNode, AttributeSyntax attribute )
        {
            StructSyntax = structNode;
            DeclaredOn   = structNode;
            Attribute    = attribute;
            EventType    = EventType.All; // default

            ComponentName             = StructSyntax.Identifier.Text;
            ComponentVisibleNamespace = GeneratorUtils.GetNamespaceFor( StructSyntax );
            ComponentNameFull         = GeneratorUtils.GetFullName( StructSyntax );

            if ( Attribute.ArgumentList != null ) {
                if ( Attribute.ArgumentList.Arguments.Count >= 1 )
                    GetEventTypeInfo( Attribute, 0 );
                if ( Attribute.ArgumentList.Arguments.Count >= 2 )
                    GetEventSystemInfo( Attribute.ArgumentList.Arguments[1] );
                else {
                    EventSystemClassNamespace = "ReactiveDots";
                    EventSystemClassNameFull  = "ReactiveDots.DefaultEventSystem";
                    EventSystemClassName      = "DefaultEventSystem";
                }
            }

            FieldToCompareName = GeneratorUtils.GetAttributeArgumentValue( Attribute, "FieldNameToCompare", "Value" );
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

        private void GetEventSystemInfo( AttributeArgumentSyntax argument )
        {
            var systemType = GeneratorUtils.GetTypeofFromAttributeArgument( argument );
            var split      = systemType.Split( '.' );
            EventSystemClassNamespace = String.Empty;
            for ( int i = 0; i < split.Length - 1; i++ )
                EventSystemClassNamespace = split[i] + ( i < split.Length - 2 ? "." : "" );
            EventSystemClassNameFull = systemType;
            EventSystemClassName     = split[split.Length - 1];
        }
    }
}