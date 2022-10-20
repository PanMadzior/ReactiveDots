using System.Collections.Generic;
using ReactiveDots;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace ReactiveDotsSample
{
    public class UiManager : MonoBehaviour
    {
        public Button     destroyAllBalls;
        public Button     spawnBallButton;
        public Button     spawnBalls100Button;
        public Button     spawnBalls10000Button;
        public Button     spawnBalls100000Button;
        public Button     spawnBalls1000000Button;
        public Toggle     everyFrameSpawn;
        public InputField everyFrameSpawnAmountField;
        public Toggle     eventSystemsToggle;
        public Text       eventSystemInfo;
        public Button     updateWithoutEcbButton;
        public Button     updateWithTempEcbButton;
        public Button     updateWithExternalEcbButton;

        private void Awake()
        {
            destroyAllBalls.onClick.AddListener( DestroyAllBalls );
            spawnBallButton.onClick.AddListener( () => SpawnBalls( 1 ) );
            spawnBalls100Button.onClick.AddListener( () => SpawnBalls( 100 ) );
            spawnBalls10000Button.onClick.AddListener( () => SpawnBalls( 10000 ) );
            spawnBalls100000Button.onClick.AddListener( () => SpawnBalls( 100000 ) );
            spawnBalls1000000Button.onClick.AddListener( () => SpawnBalls( 1000000 ) );
            eventSystemsToggle.onValueChanged.AddListener( ( _ ) => UpdateEventSystems() );
            updateWithoutEcbButton.onClick.AddListener( () =>
                SetUpdate( BounceCountSystem.UpdateType.NowWithEntityManager ) );
            updateWithTempEcbButton.onClick.AddListener( () => SetUpdate( BounceCountSystem.UpdateType.NowWithEcb ) );
            updateWithExternalEcbButton.onClick.AddListener( () =>
                SetUpdate( BounceCountSystem.UpdateType.WithExternalEcb ) );
            UpdateEventSystems();
            SetUpdate( BounceCountSystem.UpdateType.NowWithEcb );
        }

        private void Update()
        {
            if ( everyFrameSpawn.isOn ) {
                var amount = int.Parse( everyFrameSpawnAmountField.text );
                SpawnBalls( amount );
            }
        }

        private void DestroyAllBalls()
        {
            var em    = World.DefaultGameObjectInjectionWorld.EntityManager;
            var query = em.CreateEntityQuery( typeof(Ball) );
            var balls = query.ToEntityArray( Allocator.Temp );
            em.DestroyEntity( balls );
            balls.Dispose();
        }

        private void SetUpdate( BounceCountSystem.UpdateType type )
        {
            switch ( type ) {
                case BounceCountSystem.UpdateType.NowWithEntityManager:
                    updateWithoutEcbButton.GetComponent<Image>().color      = Color.green;
                    updateWithTempEcbButton.GetComponent<Image>().color     = Color.white;
                    updateWithExternalEcbButton.GetComponent<Image>().color = Color.white;
                    break;
                case BounceCountSystem.UpdateType.NowWithEcb:
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

            World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<BounceCountSystem>().updateType = type;
        }

        private void UpdateEventSystems()
        {
            var enabled = eventSystemsToggle.isOn;
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<DefaultEventSystem>().Enabled = enabled;
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<CustomEventSystem>().Enabled  = enabled;
            eventSystemInfo.text = enabled
                ? "Event systems are now enabled. Disable them if you are testing a large number of entities."
                : "Event systems are now disabled. Keep them disable if you are testing a large number of entities.";
        }

        private void SpawnBalls( int amount )
        {
            var prefab = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<BounceCountSystem>()
                .GetSingleton<Arena>().BallPrefab;
            World.DefaultGameObjectInjectionWorld.EntityManager
                .Instantiate( prefab, amount, Allocator.Temp )
                .Dispose();
        }
    }
}