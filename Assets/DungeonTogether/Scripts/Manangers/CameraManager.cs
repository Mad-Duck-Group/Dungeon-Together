using DungeonTogether.Scripts.Character;
using Unity.Netcode;
using UnityEngine;

namespace DungeonTogether.Scripts.Manangers
{
    public class CameraManager : NetworkBehaviour
    {
        [SerializeField] private Camera camera;

        private void Start()
        {
            ClassSelector.OnCharacterSpawned += OnSpawnPlayer;
        }

        private void OnSpawnPlayer(ulong clientId)
        {
            var localPlayer = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
            if (!localPlayer || !localPlayer.IsOwner) return;
            camera.transform.SetParent(localPlayer.transform);
            camera.transform.localPosition = new Vector3(0, 0, -10);
        }

        public override void OnNetworkDespawn()
        {
            camera.transform.SetParent(null);
            ClassSelector.OnCharacterSpawned -= OnSpawnPlayer;
        }
    }
}
