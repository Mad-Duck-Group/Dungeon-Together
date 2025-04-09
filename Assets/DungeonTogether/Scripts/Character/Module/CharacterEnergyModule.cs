using System;
using DungeonTogether.Scripts.Manangers;
using MoreMountains.Tools;
using TriInspector;
using Unity.Netcode;
using UnityEngine;

namespace DungeonTogether.Scripts.Character.Module
{
    [Serializable]
    public record EnergyData : INetworkSerializable
    {
        [ReadOnly] public float currentEnergy;
        public float maxEnergy;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref currentEnergy);
            serializer.SerializeValue(ref maxEnergy);
        }
    }
    public class CharacterEnergyModule : CharacterModule
    {
        [Title("Energy Settings")] 
        [SerializeField]
        public NetworkVariable<EnergyData> energyData = new();
        [SerializeField] 
        private float startingEnergy = 0;
        [SerializeField]
        private float bumpThreshold = 10f;
        [SerializeField] 
        private bool useMMEnergyBar = true;
        [SerializeField, ShowIf(nameof(useMMEnergyBar))] 
        private MMHealthBar energyBar;

        [Title("Debug")] 
        [SerializeField] 
        private float testAmount;
        [Button("Test Change Energy")] 
        private void TestChangeEnergy() => ChangeEnergy(testAmount);
        private float _previousChange;

        public override void OnNetworkSpawn()
        {
            energyData.OnValueChanged += OnEnergyDataChanged;
            base.OnNetworkSpawn();
            EnergyDataInitServerRpc();
        }
        
        [Rpc(SendTo.Server)]
        private void EnergyDataInitServerRpc()
        {
            energyData.Value.currentEnergy = startingEnergy;
            energyData.CheckDirtyState();
        }
        
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            energyData.OnValueChanged -= OnEnergyDataChanged;
        }

        private void OnEnergyDataChanged(EnergyData previousvalue, EnergyData newvalue)
        {
            _previousChange = newvalue.currentEnergy - previousvalue.currentEnergy;
            UpdateEnergyBar();
        }

        public virtual bool HasEnoughEnergy(float amount)
        {
            return energyData.Value.currentEnergy >= amount;
        }

        public virtual void ChangeEnergy(float amount)
        {
            if (!ModulePermitted) return;
            _previousChange = amount;
            ChangeEnergyServerRpc(amount);
            UpdateEnergyBar();
        }

        [Rpc(SendTo.Server)]
        private void ChangeEnergyServerRpc(float amount)
        {
            energyData.Value.currentEnergy += amount;
            energyData.Value.currentEnergy = Mathf.Clamp(energyData.Value.currentEnergy, 0, energyData.Value.maxEnergy);
            energyData.CheckDirtyState();
        }

        public virtual void UpdateEnergyBar()
        {
            if (!IsOwner && characterHub.CharacterType is CharacterType.Player) return;
            if (!ModulePermitted) return;
            if (useMMEnergyBar)
            {
                bool energyBarExists = energyBar;
                var currentEnergy = energyData.Value.currentEnergy;
                var maxEnergy = energyData.Value.maxEnergy;
                var shouldBump = Mathf.Abs(_previousChange) >= bumpThreshold;
                switch (energyBarExists)
                {
                    case true:
                        var targetProgressBar = energyBar.TargetProgressBar;
                        targetProgressBar.BumpScaleOnChange = shouldBump;
                        targetProgressBar.LerpForegroundBar = shouldBump;
                        targetProgressBar.LerpDecreasingDelayedBar = shouldBump;
                        targetProgressBar.LerpIncreasingDelayedBar = shouldBump;
                        energyBar.UpdateBar(currentEnergy, 0, maxEnergy, true);
                        break;
                    case false when characterHub.CharacterType is CharacterType.Player:
                        PlayerCanvasManager.Instance.UpdateEnergyBar(currentEnergy, maxEnergy, shouldBump);
                        break;
                    default:
                        Debug.LogWarning("Character is NPC but has no energy bar.");
                        break;
                }
            }
            else
            {
                //other energy bar update logic
            }
        }
    }
}
