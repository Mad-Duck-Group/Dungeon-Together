using System;
using DungeonTogether.Scripts.Manangers;
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
        private NetworkVariable<HealthData> healthData = new();
        [SerializeField] 
        private float startingHealth = 100;
        [SerializeField]
        private float bumpThreshold = 10f;
        [SerializeField] 
        private bool useMMHealthBar = true;
        [SerializeField, ShowIf(nameof(useMMHealthBar))] 
        private MMHealthBar healthBar;

        [Title("Debug")] 
        [SerializeField] 
        private float testAmount;
        [Button("Test Change Health")] 
        private void TestChangeHealth() => ChangeHealth(testAmount);
        private float _previousChange;

        public override void OnNetworkSpawn()
        {
            healthData.OnValueChanged += OnHealthDataChanged;
            base.OnNetworkSpawn();
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
            _previousChange = newvalue.currentHealth - previousvalue.currentHealth;
            UpdateHealthBar();
        }

        public virtual void ChangeHealth(float amount)
        {
            if (!ModulePermitted) return;
            if (healthData.Value.invincible) return;
            _previousChange = amount;
            ChangeHealthServerRpc(amount);
            if (healthData.Value.currentHealth <= 0)
            {
                DieRpc();
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
            if (!IsOwner && characterHub.CharacterType is CharacterType.Player) return;
            if (!ModulePermitted) return;
            if (useMMHealthBar)
            {
                bool healthBarExists = healthBar;
                var currentHealth = healthData.Value.currentHealth;
                var maxHealth = healthData.Value.maxHealth;
                var shouldBump = Mathf.Abs(_previousChange) >= bumpThreshold;
                switch (healthBarExists)
                {
                    case true:
                        var targetProgressBar = healthBar.TargetProgressBar;
                        targetProgressBar.BumpScaleOnChange = shouldBump;
                        targetProgressBar.LerpForegroundBar = shouldBump;
                        targetProgressBar.LerpDecreasingDelayedBar = shouldBump;
                        targetProgressBar.LerpIncreasingDelayedBar = shouldBump;
                        healthBar.UpdateBar(currentHealth, 0, maxHealth, true);
                        break;
                    case false when characterHub.CharacterType is CharacterType.Player:
                        PlayerCanvasManager.Instance.UpdateHealthBar(currentHealth, maxHealth, shouldBump);
                        break;
                    default:
                        Debug.LogWarning("Character is NPC but has no health bar.");
                        break;
                }
            }
            else
            {
                //other health bar update logic
            }
        }
        
        [Rpc(SendTo.Owner, DeferLocal = true)]
        protected virtual void DieRpc()
        {
            characterHub.ChangeConditionState(CharacterConditionState.Dead);
            if (characterHub.CharacterType is CharacterType.NPC)
            {
                characterHub.ShutdownInstant();
            }
        }
    }
}
