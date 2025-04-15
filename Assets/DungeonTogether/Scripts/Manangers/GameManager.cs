using System;
using System.Collections.Generic;
using System.Linq;
using DungeonTogether.Scripts.Utils;
using TriInspector;
using Unity.Netcode;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DungeonTogether.Scripts.Manangers
{
    public class GameManager : NetworkBehaviour
    {
        private void OnEnable()
        {
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }
        
        private void OnDisable()
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        }

        public override void OnNetworkSpawn()
        {
        }

        protected override void OnNetworkPostSpawn()
        {
        }
        
        public override void OnNetworkDespawn()
        {
        }

        private void OnActiveSceneChanged(Scene before, Scene after)
        {
            var gameScene = LoadSceneManager.Instance.sceneAssets[SceneType.Game];
            Debug.Log($"After scene path: {after.name}");
            if (after.path != gameScene.Path) return;
            var localClientId = NetworkManager.Singleton.LocalClient.ClientId;
            Debug.Log($"Local client ID: {localClientId}");
            SpawnSelectedClassEvent.Invoke(localClientId);
        }
    }
}
