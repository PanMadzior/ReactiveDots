using System.Collections.Generic;
using ReactiveDots;
using Unity.Collections;
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
        public Button     spawnBalls100000Button;
        public Button     spawnBalls1000000Button;
        public Toggle     eventSystemsToggle;
        public Text       eventSystemInfo;
        public Button     updateWithoutEcbButton;
        public Button     updateWithTempEcbButton;
        public Button     updateWithExternalEcbButton;

        private Entity _ballEntityPrefab;

        private void Awake()
        {
            spawnBallButton.onClick.AddListener( () => SpawnBalls( 1 ) );
            spawnBalls100Button.onClick.AddListener( () => SpawnBalls( 100 ) );
            spawnBalls10000Button.onClick.AddListener( () => SpawnBalls( 10000 ) );
            spawnBalls100000Button.onClick.AddListener( () => SpawnBalls( 100000 ) );
            spawnBalls1000000Button.onClick.AddListener( () => SpawnBalls( 1000000 ) );
            eventSystemsToggle.onValueChanged.AddListener( ( _ ) => UpdateEventSystems() );
            updateWithoutEcbButton.onClick.AddListener( () => SetUpdate( BounceCountSystem.UpdateType.WithoutEcb ) );
            updateWithTempEcbButton.onClick.AddListener( () => SetUpdate( BounceCountSystem.UpdateType.WithTempEcb ) );
            updateWithExternalEcbButton.onClick.AddListener( () =>
                SetUpdate( BounceCountSystem.UpdateType.WithExternalEcb ) );
            UpdateEventSystems();
            SetUpdate( BounceCountSystem.UpdateType.WithTempEcb );
        }

        private void SetUpdate( BounceCountSystem.UpdateType type )
        {
            switch ( type ) {
                case BounceCountSystem.UpdateType.WithoutEcb:
                    updateWithoutEcbButton.GetComponent<Image>().color      = Color.green;
                    updateWithTempEcbButton.GetComponent<Image>().color     = Color.white;
                    updateWithExternalEcbButton.GetComponent<Image>().color = Color.white;
                    break;
                case BounceCountSystem.UpdateType.WithTempEcb:
                    updateWithoutEcbButton.GetComponent<Image>().color      = Color.white;
                    updateWithTempEcbButton.GetComponent<Image>().color     = Color.green;
                    updateWithExternalEcbButton.GetComponent<Image>().color = Color.white;
                    break;
                case BounceCountSystem.UpdateType.WithExternalEcb:
                    updateWithoutEcbButton.GetComponent<Image>().color      = Color.white;
                    updateWithTempEcbButton.GetComponent<Image>().color     = Color.white;
                    updateWithExternalEcbButton.GetComponent<Image>().color = Color.green;
                    break;
            }

            World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BounceCountSystem>().updateType = type;
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

        private void SpawnBalls( int amount )
        {
            // TODO: do spawning ECS way but randomize direction and speed like
            // World.DefaultGameObjectInjectionWorld.EntityManager
            //     .Instantiate( _ballEntityPrefab, amount, Allocator.Temp )
            //     .Dispose();
            for ( int i = 0; i < amount; i++ )
                GameObject.Instantiate( ballsPrefab );
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