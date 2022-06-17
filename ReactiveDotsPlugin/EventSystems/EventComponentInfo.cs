using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReactiveDotsPlugin
{
    public class EventComponentInfo
    {
        public EventType EventType { private set; get; }
        public StructDeclarationSyntax StructSyntax { private set; get; }
        public AttributeSyntax Attribute { private set; get; }

        public string ComponentName { private set; get; }
        public string ComponentNamespace { private set; get; }
        public string ComponentNameFull { private set; get; }

        public string EventSystemClassName { private set; get; }
        public string EventSystemClassNamespace { private set; get; }
        public string EventSystemClassNameFull { private set; get; }

        public string FieldToCompareName { private set; get; }
        public string ReactiveComponentNameFull => EventSystemClassNameFull + "." + ComponentName + "Reactive";

        public EventComponentInfo( StructDeclarationSyntax structNode, AttributeSyntax attribute )
        {
            StructSyntax = structNode;
            Attribute    = attribute;
            EventType    = EventType.All; // default

            ComponentName      = StructSyntax.Identifier.Text;
            ComponentNamespace = GeneratorUtils.GetNamespaceFor( StructSyntax );
            ComponentNameFull  = GeneratorUtils.GetFullName( StructSyntax );

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