using System;
using TMPro;
using TriInspector;
using Unity.Netcode;
using UnityEngine;

namespace DungeonTogether.Scripts.Character.Module
{
    public class CharacterDisplayNameModule : CharacterModule
    {
        [Title("References")]
        [SerializeField, Required] private TMP_Text displayNameText;
        public override void Initialize(CharacterHub characterHub)
        {
            base.Initialize(characterHub);
            if (!characterHub.IsLocalPlayer) return;
            NameRequestRpc(characterHub.OwnerClientId);
        }

        private void OnEnable()
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnected;
        }
        
        private void OnDisable()
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnected;
        }

        private void OnClientConnected(ulong clientId)
        {
            SetDisplayNameRpc(displayNameText.text, RpcTarget.Single(clientId, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.Server)]
        private void NameRequestRpc(ulong clientId)
        {
            var userData = HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(clientId);
            var displayName = userData?.userName ?? "Unknown";
            SetDisplayNameRpc(displayName);
        }
        
        [Rpc(SendTo.Everyone, AllowTargetOverride = true)]
        private void SetDisplayNameRpc(string displayName, RpcParams rpcParams = default)
        {
            displayNameText.SetText(displayName);
        }
    }
}