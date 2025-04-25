using System;
using System.Collections.Generic;
using DungeonTogether.Scripts.Character;
using DungeonTogether.Scripts.Manangers;
using Redcode.Extensions;
using TriInspector;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace DungeonTogether.Scripts.Utils
{
    public class CharacterPool : NetworkBehaviour
    {
        [Serializable]
        private struct CharacterPoolItem
        {
            public CharacterHub characterPrefab;
            public int startingAmount;
        }

        [Title("References")]
        [SerializeField, Required] private Transform characterParent;
        [SerializeField] private List<CharacterPoolItem> characterPoolItems = new();
        [SerializeField, Required] private List<Transform> spawnPoints = new();
        
        [Title("Settings")]
        [SerializeField] private bool spawnEnabled = true;
        [SerializeField] private bool randomSpawnPoint = true;
        [SerializeField] private bool uniqueSpawnPoint = true;
        [SerializeField] private int maxActiveCharacterCount = 10;
        [SerializeField] private float startLevelDelay = 5f;
        [SerializeField] private float spawnInterval = 10f;
        [SerializeField] private int spawnAmount = 1;
        
        [Title("Debug")]
        [SerializeField, TriInspector.ReadOnly] private List<CharacterHub> charactersInPool = new();
        [SerializeField, TriInspector.ReadOnly] private List<CharacterHub> activeCharacters = new();
        [ShowInInspector, DisplayAsString] private int ActiveCharacterCount => activeCharacters.Count;
        [SerializeField, DisplayAsString] private float currentSpawnInterval;

        private List<Transform> availableSpawnPoints = new();
        private Dictionary<CharacterHub, Transform> slots = new();

        private void OnEnable()
        {
            GameManager.OnGameEnd += OnGameEnd;
        }

        private void OnDisable()
        {
            GameManager.OnGameEnd -= OnGameEnd;
        }

        private void OnGameEnd()
        {
            spawnEnabled = false;
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                availableSpawnPoints = new List<Transform>(spawnPoints);
                currentSpawnInterval = spawnInterval - startLevelDelay;
                SpawnInPool();
            }
        }

        private void SpawnInPool()
        {
            foreach (var item in characterPoolItems)
            {
                for (var i = 0; i < item.startingAmount; i++)
                {
                    var character = Instantiate(item.characterPrefab, characterParent);
                    character.gameObject.SetActive(false);
                    charactersInPool.Add(character);
                }
            }
        }
        
        private void Update()
        {
            if (!spawnEnabled) return;
            if (!IsServer) return;
            currentSpawnInterval += Time.deltaTime;
            if (currentSpawnInterval < spawnInterval) return;
            currentSpawnInterval = 0;
            SpawnCharacter();
        }
        
        private void SpawnCharacter()
        {
            for (var i = 0; i < spawnAmount; i++)
            {
                if (ActiveCharacterCount >= maxActiveCharacterCount) return;
                if (charactersInPool.Count == 0) return;
                var character = charactersInPool.GetRandomElement();
                character.gameObject.SetActive(true);
                var spawnPointCount = availableSpawnPoints.Count;
                var spawnIndex = i % spawnPointCount;
                var selectedSpawn = randomSpawnPoint
                    ? availableSpawnPoints.GetRandomElement()
                    : availableSpawnPoints[spawnIndex];
                character.transform.position = selectedSpawn.position;
                character.NetworkObject.Spawn();
                character.NetworkObject.DestroyWithScene = true;
                character.CharacterPool = this;
                charactersInPool.Remove(character);
                activeCharacters.Add(character);
                if (!uniqueSpawnPoint) continue;
                slots.Add(character, selectedSpawn);
                availableSpawnPoints.Remove(selectedSpawn);
            }
        }

        public void BackToPool(CharacterHub characterHub)
        {
            if (!activeCharacters.Contains(characterHub))
            {
                Debug.LogError("Character is not active or not in the pool.");
                return;
            }
            characterHub.gameObject.SetActive(false);
            activeCharacters.Remove(characterHub);
            charactersInPool.Add(characterHub);
            if (slots.TryGetValue(characterHub, out var spawnPoint))
            {
                availableSpawnPoints.Add(spawnPoint);
                slots.Remove(characterHub);
            }
            if (NetworkManager.ShutdownInProgress) return;
            DespawnRpc(characterHub.NetworkObject);
        }

        [Rpc(SendTo.Server)]
        private void DespawnRpc(NetworkObjectReference networkObject)
        {
            if (!networkObject.TryGet(out NetworkObject nob, NetworkManager)) return;
            nob.Despawn(false);
        }
    }
}
