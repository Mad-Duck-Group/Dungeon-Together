using System;
using DungeonTogether.Scripts.Manangers;
using MoreMountains.Tools;
using TriInspector;
using Unity.Netcode;
using UnityEngine;

namespace DungeonTogether.Scripts.Character.Module
{
    [Serializable]
    public record ManaData : INetworkSerializable
    {
        [ReadOnly] public float currentMana;
        public float maxMana;
        public float regenRate;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref currentMana);
            serializer.SerializeValue(ref maxMana);
            serializer.SerializeValue(ref regenRate);
        }
    }
    public class CharacterManaModule : CharacterModule
    {
        [Title("Mana Settings")]
        [SerializeField]
        private NetworkVariable<ManaData> manaData = new();
        [SerializeField]
        private float startingMana = 100;
        [SerializeField]
        private float bumpThreshold = 10f;
        [SerializeField]
        private bool useMMManaBar = true;
        [SerializeField, ShowIf(nameof(useMMManaBar))]
        private MMHealthBar manaBar;
        
        [Title("Debug")]
        [SerializeField]
        private float testAmount;
        [Button("Test Change Mana")]
        private void TestChangeMana() => ChangeMana(testAmount);
        private float _previousChange;
        
         public override void OnNetworkSpawn()
        {
            manaData.OnValueChanged += OnManaDataChanged;
            base.OnNetworkSpawn();
            ManaDataInitServerRpc();
        }
        
        [Rpc(SendTo.Server)]
        private void ManaDataInitServerRpc()
        {
            manaData.Value.currentMana = startingMana;
            manaData.CheckDirtyState();
        }
        
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            manaData.OnValueChanged -= OnManaDataChanged;
        }

        private void OnManaDataChanged(ManaData previousvalue, ManaData newvalue)
        {
            _previousChange = newvalue.currentMana - previousvalue.currentMana;
            UpdateManaBar();
        }

        protected override void UpdateModule()
        {
            base.UpdateModule();
            RegenerateMana();
        }
        
        public virtual void RegenerateMana()
        {
            if (!IsOwner && !IsOwnedByServer) return;
            if (!ModulePermitted) return;
            if (manaData.Value.currentMana >= manaData.Value.maxMana) return;
            ChangeMana(manaData.Value.regenRate * Time.deltaTime);
        }

        public virtual bool ChangeMana(float amount)
        {
            if (!ModulePermitted) return false;
            if (amount < 0 && manaData.Value.currentMana <= 0) return false;
            _previousChange = amount;
            ChangeManaServerRpc(amount);
            UpdateManaBar();
            return true;
        }

        [Rpc(SendTo.Server)]
        private void ChangeManaServerRpc(float amount)
        {
            manaData.Value.currentMana += amount;
            manaData.Value.currentMana = Mathf.Clamp(manaData.Value.currentMana, 0, manaData.Value.maxMana);
            manaData.CheckDirtyState();
        }

        public virtual void UpdateManaBar()
        {
            if (!IsOwner && characterHub.CharacterType is CharacterType.Player) return;
            if (!ModulePermitted) return;
            if (useMMManaBar)
            {
                bool healthBarExists = manaBar;
                var currentMana = manaData.Value.currentMana;
                var maxMana = manaData.Value.maxMana;
                var shouldBump = Mathf.Abs(_previousChange) >= bumpThreshold;
                switch (healthBarExists)
                {
                    case true:
                        var targetProgressBar = manaBar.TargetProgressBar;
                        targetProgressBar.BumpScaleOnChange = shouldBump;
                        targetProgressBar.LerpForegroundBar = shouldBump;
                        targetProgressBar.LerpDecreasingDelayedBar = shouldBump;
                        targetProgressBar.LerpIncreasingDelayedBar = shouldBump;
                        manaBar.UpdateBar(currentMana, 0, maxMana, true);
                        break;
                    case false when characterHub.CharacterType is CharacterType.Player:
                        PlayerCanvasManager.Instance.UpdateManaBar(currentMana, maxMana, shouldBump);
                        break;
                    default:
                        Debug.LogWarning("Character is NPC but has no mana bar.");
                        break;
                }
            }
            else
            {
                //other mana bar update logic
            }
        }
    }
}
