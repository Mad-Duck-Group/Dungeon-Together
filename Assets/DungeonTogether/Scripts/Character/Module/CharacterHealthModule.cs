using System;
using DungeonTogether.Scripts.Utils;
using MoreMountains.Tools;
using TriInspector;
using Unity.Netcode;
using UnityEngine;

namespace DungeonTogether.Scripts.Character.Module
{
    [Serializable]
    public record HealthData : INetworkSerializable
    {
        [ReadOnly] public float currentHealth;
        public float maxHealth;
        public bool invincible;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref currentHealth);
            serializer.SerializeValue(ref maxHealth);
            serializer.SerializeValue(ref invincible);
        }
    }
    /// <summary>
    /// Module responsible for handling character health.
    /// </summary>
    public class CharacterHealthModule : CharacterModule
    {
        [Title("Health Settings")] 
        [SerializeField] 
        private NetworkVariable<HealthData> healthData =
            new(default, NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);
        [SerializeField] 
        private float startingHealth = 100;
        [SerializeField] 
        private bool useMMHealthBar = true;
        [SerializeField, ShowIf(nameof(useMMHealthBar))] 
        private MMHealthBar healthBar;

        [Title("Debug")] 
        [SerializeField] 
        private float testAmount;
        [Button("Test Change Health")] 
        private void TestChangeHealth() => ChangeHealth(testAmount);

        public override void OnNetworkSpawn()
        {
            healthData.OnValueChanged += OnHealthDataChanged;
            base.OnNetworkSpawn();
            if (!IsOwner)
            {
                UpdateHealthBar();
                return;
            }
            HealthDataInitServerRpc();
        }
        
        [Rpc(SendTo.Server)]
        private void HealthDataInitServerRpc()
        {
            healthData.Value.currentHealth = startingHealth;
            healthData.CheckDirtyState();
        }
        
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            healthData.OnValueChanged -= OnHealthDataChanged;
        }

        private void OnHealthDataChanged(HealthData previousvalue, HealthData newvalue)
        {
            UpdateHealthBar();
        }

        public virtual void ChangeHealth(float amount)
        {
            if (!ModulePermitted) return;
            if (healthData.Value.invincible) return;
            ChangeHealthServerRpc(amount);
            if (healthData.Value.currentHealth <= 0)
            {
                healthData.Value.currentHealth = 0;
                Die();
            }
            
            UpdateHealthBar();
        }

        [Rpc(SendTo.Server)]
        private void ChangeHealthServerRpc(float amount)
        {
            healthData.Value.currentHealth += amount;
            healthData.Value.currentHealth = Mathf.Clamp(healthData.Value.currentHealth, 0, healthData.Value.maxHealth);
            healthData.CheckDirtyState();
        }

        public virtual void UpdateHealthBar()
        {
            if (!ModulePermitted) return;
            if (useMMHealthBar)
            {
                healthBar.UpdateBar(healthData.Value.currentHealth, 0, healthData.Value.maxHealth, true);
            }
            else
            {
                //other health bar update logic
            }
        }
        
        protected virtual void Die()
        {
            if (!ModulePermitted) return;
            CharacterStates.ConditionStateEvent.Invoke(characterHub, characterHub.ConditionState,
                CharacterStates.CharacterConditionState.Dead);
        }
    }
}
