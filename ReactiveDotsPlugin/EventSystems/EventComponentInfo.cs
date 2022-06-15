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

        public ClassDeclarationSyntax EventSystemClassSyntax { private set; get; }
        public string EventSystemClassName { private set; get; }
        public string EventSystemClassNamespace { private set; get; }
        public string EventSystemClassNameFull { private set; get; }

        public string FieldToCompareName { private set; get; }
        public string ReactiveComponentNameFull =>
            EventSystemClassNamespace + "." + EventSystemClassName + "." + ComponentName + "Reactive";

        public EventComponentInfo( StructDeclarationSyntax structNode, AttributeSyntax attribute )
        {
            StructSyntax = structNode;
            Attribute    = attribute;
            EventType    = EventType.All; // default

            ComponentName      = StructSyntax.Identifier.Text;
            ComponentNamespace = GeneratorUtils.GetNamespaceFor( StructSyntax );
            ComponentNameFull  = GeneratorUtils.GetFullName( StructSyntax );
        }

        public void Update( GeneratorExecutionContext context )
        {
            if ( Attribute.ArgumentList != null ) {
                if ( Attribute.ArgumentList.Arguments.Count >= 1 )
                    GetEventTypeInfo( context, Attribute );
                if ( Attribute.ArgumentList.Arguments.Count >= 2 )
                    GetEventSystemInfo( context, Attribute.ArgumentList.Arguments[1] );
            }

            FieldToCompareName = GeneratorUtils.GetAttributeArgumentValue( Attribute, "FieldNameToCompare", "Value" );
        }

        private void GetEventTypeInfo( GeneratorExecutionContext context, AttributeSyntax attribute )
        {
            var eventTypeStr = GeneratorUtils.GetAttributeArgumentValue( attribute, 0, "EventType.All" );
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

        private void GetEventSystemInfo( GeneratorExecutionContext context, AttributeArgumentSyntax argument )
        {
            var systemType = GeneratorUtils.GetTypeofFromAttributeArgument( argument );
            if ( GeneratorUtils.TryFindClassSyntaxByName( context, systemType, out var classSyntax ) ) {
                EventSystemClassSyntax    = classSyntax;
                EventSystemClassNamespace = GeneratorUtils.GetNamespaceFor( classSyntax );
                EventSystemClassNameFull  = GeneratorUtils.GetFullName( classSyntax );
                EventSystemClassName      = systemType;
            }
        }
    }
}