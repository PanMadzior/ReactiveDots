using NUnit.Framework;
using Unity.Entities;

namespace ReactiveDots.Tests
{
    public abstract class TestBase
    {
        protected EntityManager EntityManager { private set; get; }
        protected World World { private set; get; }

        [SetUp]
        public void Setup()
        {
            World         = new World( "Test World" );
            EntityManager = World.EntityManager;
            OnSetup();
        }

        protected virtual void OnSetup() { }

        [TearDown]
        public void TearDown()
        {
            if ( World != null && World.IsCreated ) {
                while ( World.Systems.Count > 0 )
                    World.DestroySystem( World.Systems[0] );
                World.Dispose();
            }

            OnTearDown();
        }

        protected virtual void OnTearDown() { }
    }
}