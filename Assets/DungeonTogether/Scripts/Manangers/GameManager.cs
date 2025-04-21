using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DungeonTogether.Scripts.Character;
using DungeonTogether.Scripts.Character.Module;
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
        [Title("Settings")] 
        [SerializeField] private float respawningTime = 10f;
        
        [Title("Debug")]
        [SerializeField, ReadOnly] private Dictionary<CharacterHub, float> respawningPlayers = new();
        [SerializeField, ReadOnly] private List<CharacterHub> deadNPCs = new();

        private bool _isGameEnd;
        private float _gameTimer;
        public static event Action OnGameEnd;
        private void OnEnable()
        {
            EventBus<CharacterDeathEvent>.Event += OnCharacterDeath;
        }
        
        private void OnDisable()
        { 
            EventBus<CharacterDeathEvent>.Event -= OnCharacterDeath;
        }

        private void OnCharacterDeath(CharacterDeathEvent eventData)
        {
            if (!eventData.characterHub) return;
            if (eventData.characterHub.CharacterType is CharacterType.Player)
            {
                respawningPlayers.TryAdd(eventData.characterHub, Time.time);
                SetActiveRespawnCanvasRpc(true, RpcTarget.Single(eventData.characterHub.OwnerClientId, RpcTargetUse.Temp));
                var playerCount = NetworkManager.Singleton.ConnectedClients.Count;
                if (respawningPlayers.Count < playerCount) return;
                SetActiveRespawnCanvasRpc(false, RpcTarget.Everyone);
                SetActiveLoseCanvasRpc(true);
                _isGameEnd = true;
                OnGameEnd?.Invoke();
            } 
            else
            {
                deadNPCs.Add(eventData.characterHub);
                if (!eventData.endLevelWhenDead) return;
                SetActiveRespawnCanvasRpc(false, RpcTarget.Everyone);
                SetActiveWinCanvasRpc(true);
                SetCompletionTimeTextRpc(_gameTimer);
                _isGameEnd = true;
                OnGameEnd?.Invoke();
            }
        }

        private void Update()
        {
            if (_isGameEnd) return;
            _gameTimer += Time.deltaTime;
            UpdateRespawnTimer();
        }

        private void UpdateRespawnTimer()
        {
            if (respawningPlayers.Count == 0) return;
            foreach (var player in respawningPlayers.Keys.ToList())
            {
                if (!player) continue;
                var timeStamp = respawningPlayers[player];
                UpdateRespawnTimerRpc(
                    respawningTime - (Time.time - timeStamp),
                    RpcTarget.Single(player.OwnerClientId, RpcTargetUse.Temp));
                if (Time.time - timeStamp < respawningTime) continue;
                respawningPlayers.Remove(player);
                SetActiveRespawnCanvasRpc(false, RpcTarget.Single(player.OwnerClientId, RpcTargetUse.Temp));
                ClassSelector.SpawnSelectedClass(player.OwnerClientId);
            }
        }
        
        [Rpc(SendTo.SpecifiedInParams)]
        private void UpdateRespawnTimerRpc(float time, RpcParams clientRpcParams = default)
        {
            PlayerCanvasManager.Instance.SetRespawningTimer(time);
        }
        
        [Rpc(SendTo.Everyone, AllowTargetOverride = true)]
        private void SetActiveRespawnCanvasRpc(bool active, RpcParams clientRpcParams = default)
        {
            PlayerCanvasManager.Instance.SetActiveRespawnCanvas(active);
        }
        
        [Rpc(SendTo.Everyone, AllowTargetOverride = true)]
        private void SetActiveWinCanvasRpc(bool active, RpcParams clientRpcParams = default)
        {
            PlayerCanvasManager.Instance.SetActiveWinCanvas(active);
        }
        
        [Rpc(SendTo.Everyone, AllowTargetOverride = true)]
        private void SetCompletionTimeTextRpc(float time, RpcParams clientRpcParams = default)
        {
            PlayerCanvasManager.Instance.SetCompletionTimeText(time);
        }
        
        [Rpc(SendTo.Everyone, AllowTargetOverride = true)]
        private void SetActiveLoseCanvasRpc(bool active, RpcParams clientRpcParams = default)
        {
            PlayerCanvasManager.Instance.SetActiveLoseCanvas(active);
        }
    }
}
