using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using DungeonTogether.Scripts.Character;
using DungeonTogether.Scripts.Utils;
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
        RogueMelee,
        RogueRange,
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

        private static ClassSelector _instance;
        
        private static readonly Dictionary<ulong, ClassType> SelectedClasses = new();
        public static event Action<ulong> OnPreCharacterSpawned;
        public static event Action<ulong> OnCharacterSpawned;

        private void Start()
        {
            _instance = this;
            foreach (var (button, classType) in classButtons)
            {
                button.onClick.AddListener(() => SelectClass(classType));
            }
        }
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            var localClientId = NetworkManager.LocalClient.ClientId;
            if (SelectedClasses.TryGetValue(localClientId, out var classType))
            {
                SpawnCharacterRpc(localClientId, classType);
                return;
            }
            SetActive(true);
        }

        public void SetActive(bool active)
        {
            classSelectionCanvas.gameObject.SetActive(active);
        }

        private void SelectClass(ClassType classType)
        {
            var localClientID = NetworkManager.LocalClient.ClientId;
            Debug.Log($"Local client ID: {localClientID}");
            SpawnCharacterRpc(localClientID, classType);
            SetActive(false);
        }

        public static void SpawnSelectedClass(ulong clientId)
        {
            if (SelectedClasses.TryGetValue(clientId, out var classType))
            {
                _instance.SpawnCharacterRpc(clientId, classType);
            }
            else
            {
                Debug.LogError($"No class selected for client ID: {clientId}");
            }
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void SpawnCharacterRpc(ulong clientId, ClassType selectedClass)
        {
            Debug.Log($"Spawning character for client ID: {clientId} with class: {selectedClass}");
            OnPreCharacterSpawned?.Invoke(clientId);
            var character = Instantiate(classPrefabs[selectedClass], spawnPoint.position, Quaternion.identity);
            character.NetworkObject.SpawnAsPlayerObject(clientId);
            character.NetworkObject.ChangeOwnership(clientId);
            character.NetworkObject.DestroyWithScene = true;
            OnCharacterSpawnRpc(clientId, selectedClass);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void OnCharacterSpawnRpc(ulong clientId, ClassType selectedClass)
        {
            SelectedClasses.TryAdd(clientId, selectedClass);
            SelectedClasses[clientId] = selectedClass;
            OnCharacterSpawned?.Invoke(clientId);
        }
    }
}
