using System;
using AYellowpaper.SerializedCollections;
using DungeonTogether.Scripts.Character;
using TriInspector;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonTogether.Scripts.Manangers
{
    public enum ClassType
    {
        Fighter,
        Wizard,
        Rogue,
        Cleric
    }
    public class ClassSelector : NetworkBehaviour
    {
        [SerializedDictionary("ClassType", "Character")] 
        public SerializedDictionary<ClassType, CharacterHub> classPrefabs;
        [SerializedDictionary("Button", "Class")] 
        public SerializedDictionary<Button, ClassType> classButtons;
        [SerializeField] private CanvasGroup classSelectionCanvas;
        [SerializeField] private Transform spawnPoint;
        //[SerializeField, ReadOnly] private CharacterHub currentCharacter;

        public static event Action<ulong> OnCharacterSpawned;
        private void Start()
        {
            foreach (var (button, classType) in classButtons)
            {
                button.onClick.AddListener(() => SelectClass(classType));
            }
        }
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            classSelectionCanvas.gameObject.SetActive(true);
        }

        private void SelectClass(ClassType classType)
        {
            var localClientID = NetworkManager.LocalClient.ClientId;
            Debug.Log($"Local client ID: {localClientID}");
            SpawnCharacterRpc(localClientID, classType);
            classSelectionCanvas.gameObject.SetActive(false);
        }

        [Rpc(SendTo.Server)]
        private void SpawnCharacterRpc(ulong clientId, ClassType selectedClass)
        {
            var character = Instantiate(classPrefabs[selectedClass], spawnPoint.position, Quaternion.identity);
            character.NetworkObject.SpawnAsPlayerObject(clientId);
            character.NetworkObject.ChangeOwnership(clientId);
            OnCharacterSpawnRpc(clientId);
        }
        
        [Rpc(SendTo.ClientsAndHost)]
        private void OnCharacterSpawnRpc(ulong clientId)
        {
            OnCharacterSpawned?.Invoke(clientId);
        }
    }
}
