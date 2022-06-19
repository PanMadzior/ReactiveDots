using System.Collections.Generic;
using ReactiveDots;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace ReactiveDotsSample
{
    public class UiManager : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
    {
        public GameObject ballsPrefab;
        public Button     spawnBallButton;
        public Button     spawnBalls100Button;
        public Button     spawnBalls10000Button;
        public Toggle     eventSystemsToggle;
        public Text       eventSystemInfo;

        private Entity _ballEntityPrefab;

        private void Awake()
        {
            spawnBallButton.onClick.AddListener( SpawnBall );
            spawnBalls100Button.onClick.AddListener( () => SpawnBalls( 100 ) );
            spawnBalls10000Button.onClick.AddListener( () => SpawnBalls( 10000 ) );
            eventSystemsToggle.onValueChanged.AddListener( ( _ ) => UpdateEventSystems() );
            UpdateEventSystems();
        }

        private void UpdateEventSystems()
        {
            var enabled = eventSystemsToggle.isOn;
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<DefaultEventSystem>().Enabled = enabled;
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<CustomEventSystem>().Enabled  = enabled;
            eventSystemInfo.text = enabled
                ? "Event systems are now enabled. Disable them if you are testing a large number of entities."
                : "Event systems are now disabled. Keep them disable if you are testing a large number of entities.";
        }

        private void SpawnBall()
        {
            // TODO: change instantiate to a DOTS way
            GameObject.Instantiate( ballsPrefab );
        }

        private void SpawnBalls( int amount )
        {
            for ( int i = 0; i < amount; i++ )
                SpawnBall();
        }

        public void DeclareReferencedPrefabs( List<GameObject> referencedPrefabs )
        {
            referencedPrefabs.Add( ballsPrefab );
        }

        public void Convert( Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem )
        {
            _ballEntityPrefab = conversionSystem.GetPrimaryEntity( ballsPrefab );
        }
    }
}