using System;
using System.Collections;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace DungeonTogether.Scripts.Character.Module.Ultimate
{
    [Serializable]
    public struct AreaUltimatePattern
    {
        [Group("Area"), Required] public DamageArea areaUltimate;
        [Group("Value"), Min(0)] public float value;
        [Group("Timing"), Min(0)] public float delay;
        [Group("Timing"), Min(0)] public float duration;
        [Group("Timing"), Min(0)] public float cooldown;
        [Group("Timing"), Min(0)] public float resetComboTime;
        [Group("Cost"), Min(0)] public float energy;
    }
    public class CharacterAreaUltimateModule : CharacterModule
    {
        [Title("Settings")]
        [SerializeField] protected Transform comboParent;
        [TableList(Draggable = true,
            HideAddButton = false,
            HideRemoveButton = false,
            AlwaysExpanded = false)]
        [SerializeField] protected List<AreaUltimatePattern> areaUltimatePattern;
        
        [Title("Debug")]
        [SerializeField, DisplayAsString] protected int currentPatternIndex;
        [SerializeField, DisplayAsString] protected int previousPatternIndex = -1;
        [FormerlySerializedAs("skillReady")] [SerializeField, DisplayAsString] protected bool ultimateReady;
        [SerializeField, DisplayAsString] protected float currentCooldown;
        [SerializeField, DisplayAsString] protected float currentComboTime;
        
        protected AreaUltimatePattern? CurrentPattern => areaUltimatePattern[currentPatternIndex];
        protected AreaUltimatePattern? PreviousPattern
        {
            get
            {
                if (previousPatternIndex == -1) return null;
                return areaUltimatePattern[previousPatternIndex];
            }
        }
        protected Coroutine ultimateCoroutine;
        
        public override void Initialize(CharacterHub characterHub)
        {
            base.Initialize(characterHub);
            currentPatternIndex = 0;
            currentCooldown = 0;
            previousPatternIndex = -1;
            areaUltimatePattern.ForEach(pattern =>
            {
                pattern.areaUltimate.SetActive(false);
                pattern.areaUltimate.OnHitEvent += OnHit;
            });
        }

        public override void Shutdown()
        {
            base.Shutdown();
            currentPatternIndex = 0;
            currentCooldown = 0;
            previousPatternIndex = -1;
            areaUltimatePattern.ForEach(pattern =>
            {
                pattern.areaUltimate.SetActive(false);
                pattern.areaUltimate.OnHitEvent -= OnHit;
            });
        }
        
        protected virtual void OnHit(Collider2D collider)
        {
            if (!collider.TryGetComponent(out CharacterHub characterHub)) return;
            var healthModule = characterHub.FindModuleOfType<CharacterHealthModule>();
            if (healthModule && CurrentPattern != null) 
                healthModule.ChangeHealth(-CurrentPattern.Value.value);
        }
        
        protected override void HandleInput()
        {
            if (characterHub.CharacterType is not CharacterType.Player) return;
            base.HandleInput();
            if (PlayerInput.UltimateButton.isDown)
            {
                CastUltimate();
            }
            
        }
        protected override void UpdateModule()
        {
            if (!ModulePermitted) return;
            base.UpdateModule();
            if (PreviousPattern != null)
            {
                if (ultimateReady && currentComboTime < PreviousPattern.Value.resetComboTime)
                {
                    currentComboTime += Time.deltaTime;
                }
                if (currentComboTime >= PreviousPattern.Value.resetComboTime)
                {
                    currentComboTime = 0;
                    currentPatternIndex = 0;
                    previousPatternIndex = -1;
                }
                if (!ultimateReady && PreviousPattern != null &&currentCooldown < PreviousPattern.Value.cooldown)
                {
                    currentCooldown += Time.deltaTime;
                    return;
                }
            }
            ultimateReady = true;
            currentCooldown = 0;
        }
        
        private void CastUltimate()
        {
            if (!ModulePermitted) return;
            if (!ultimateReady) return;
            if (ultimateCoroutine != null) return;
            ultimateCoroutine = StartCoroutine(CastUltimateCoroutine());
        }
        protected IEnumerator CastUltimateCoroutine()
        {
            if (CurrentPattern == null) yield break;
            currentComboTime = 0;
            ConsumeEnergy(CurrentPattern.Value.energy);
            characterHub.ChangeActionState(CharacterStates.CharacterActionState.Ultimate);
            yield return new WaitForSeconds(CurrentPattern.Value.delay);
            CurrentPattern.Value.areaUltimate.SetActive(true);
            yield return new WaitForSeconds(CurrentPattern.Value.duration);
            CurrentPattern.Value.areaUltimate.SetActive(false);
            characterHub.ChangeActionState(CharacterStates.CharacterActionState.None);
            previousPatternIndex = currentPatternIndex;
            currentPatternIndex = (currentPatternIndex + 1) % areaUltimatePattern.Count;
            ultimateReady = false;
            ultimateCoroutine = null;
            
        }
        private void ConsumeEnergy(float amount)
        {
            var energyModule = characterHub.FindModuleOfType<CharacterEnergyModule>();
            if (!energyModule || energyModule.energyData.Value.currentEnergy < amount) return;
            energyModule.ChangeEnergy(-amount);
        }
    
    }
}

