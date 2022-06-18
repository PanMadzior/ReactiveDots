using System.Collections.Generic;
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

        private Entity _ballEntityPrefab;

        private void Awake()
        {
            spawnBallButton.onClick.AddListener( SpawnBall );
            spawnBalls100Button.onClick.AddListener( () => SpawnBalls( 100 ) );
            spawnBalls10000Button.onClick.AddListener( () => SpawnBalls( 10000 ) );
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