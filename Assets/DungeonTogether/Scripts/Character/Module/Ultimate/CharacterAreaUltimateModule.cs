using System;
using System.Collections;
using System.Collections.Generic;
using DungeonTogether.Scripts.Manangers;
using DungeonTogether.Scripts.Utils;
using TriInspector;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace DungeonTogether.Scripts.Character.Module.Ultimate
{
    [Serializable]
    public struct AreaUltimatePattern
    {
        [Group("Area"), Required] public DamageArea areaUltimate;
        [Group("Area")] public bool disableRotateToMouse;
        [Group("Value"), Min(0)] public float value;
        [Group("Timing"), Min(0)] public float delay;
        [Group("Timing"), Min(0)] public float duration;
        [Group("Timing"), Min(0)] public float cooldown;
        [Group("Timing"), Min(0)] public float resetComboTime;
        [Group("Cost"), Min(0)] public float energy;
        [Group("Effect")] public bool hasEffect;
        [Group("Effect"), ShowIf("hasEffect")] public float effectDuration;
    }
    public class CharacterAreaUltimateModule : CharacterModule
    {
        [Title("References")]
        [SerializeField] private RotateToMouse rotateToMouse;
        
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
        [SerializeField, DisplayAsString] protected bool ultimateReady;
        [SerializeField, DisplayAsString] protected bool ultimateUsed;
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
        private CharacterEnergyModule energyModule;
        protected Coroutine ultimateCoroutine;

        public override void Initialize(CharacterHub characterHub)
        {
            base.Initialize(characterHub);
            currentPatternIndex = 0;
            if (CurrentPattern != null)
            {
                currentCooldown = CurrentPattern.Value.cooldown;
                UpdateUltimateActionIcon(true, CurrentPattern.Value);
            }
            else
            {
                UpdateUltimateActionIcon(false);
            }
            previousPatternIndex = -1;
            areaUltimatePattern.ForEach(pattern =>
            {
                pattern.areaUltimate.Initialize();
                pattern.areaUltimate.SetActive(false);
                pattern.areaUltimate.OnHitEvent += OnHit;
            });
            
        }
        
        public override void PostInitialize()
        {
            base.PostInitialize();
            energyModule = characterHub.FindModuleOfType<CharacterEnergyModule>();
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
        
        // another class come to override effect here
        protected virtual void OnHit(Collider2D collider)
        {
            if (!collider.TryGetComponent(out CharacterHub characterHub)) return;
            var healthModule = characterHub.FindModuleOfType<CharacterHealthModule>();
            if (CurrentPattern == null) return;
            if (healthModule) 
                healthModule.ChangeHealth(-CurrentPattern.Value.value);
            if (!CurrentPattern.Value.hasEffect) return;
                characterHub.ChangeConditionState(CharacterConditionState.Stunned, CurrentPattern.Value.effectDuration);
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
        
        protected override void Update()
        {
            if (!IsOwner) return;
            if (ModulePermitted)
            {
                HandleInput();
            }
            UpdateModule();
        }
        
        protected override void UpdateModule()
        {
            UpdateCooldown();
            if (!ModulePermitted)
            {
                UpdateUltimateActionIcon(false);
                return;
            }
            base.UpdateModule();
        }

        private void UpdateCooldown()
        {
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
            }
            var pattern = PreviousPattern ?? CurrentPattern;
            if (pattern == null) return;
            bool hasEnergy = energyModule && energyModule.HasEnoughEnergy(pattern.Value.energy);
            bool coolingDown = false;
            if (!ultimateReady && currentCooldown < pattern.Value.cooldown)
            {
                currentCooldown += Time.deltaTime;
                ultimateReady = false;
                coolingDown = true;
            }
            else
            {
                ultimateReady = true;
            }
            bool available = (!ultimateUsed && ultimateReady && hasEnergy) || coolingDown;
            UpdateUltimateActionIcon(available, pattern);
        }
        
        private void UpdateUltimateActionIcon(bool available, AreaUltimatePattern? pattern = null)
        {
            if (characterHub.CharacterType is not CharacterType.Player) return;
            if (pattern != null)
            {
                float max = pattern.Value.cooldown;
                PlayerCanvasManager.Instance.UpdateUltimateIcon(currentCooldown, max);
            }
            PlayerCanvasManager.Instance.SetAvailableUltimate(available);
        }

        public virtual void CastUltimate()
        {
            if (!ModulePermitted) return;
            if (!ultimateReady) return;
            if (ultimateCoroutine != null) return;
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            ultimateCoroutine = StartCoroutine(CastUltimateCoroutine(mousePos));
        }
        protected virtual IEnumerator CastUltimateCoroutine(Vector3? spawnPosition = null)
        {
            if (CurrentPattern == null) yield break;
            if (characterHub.CharacterType is CharacterType.Player && 
                !ConsumeEnergy(CurrentPattern.Value.energy))
            {
                ultimateCoroutine = null;
                yield break;
            }
            currentComboTime = 0;
            ultimateUsed = true;
            characterHub.ChangeActionState(CharacterActionState.Ultimate);
            var area = CurrentPattern.Value.areaUltimate;
            yield return new WaitForSeconds(CurrentPattern.Value.delay);
            area.SetActive(true);
            if (spawnPosition != null)
            {
                area.SetPosition(spawnPosition.Value);
            }
            if (CurrentPattern.Value.disableRotateToMouse && rotateToMouse) rotateToMouse.SetActive(false);
            yield return new WaitForSeconds(CurrentPattern.Value.duration);
            if (CurrentPattern.Value.disableRotateToMouse && rotateToMouse) rotateToMouse.SetActive(true);
            area.SetActive(false);
            characterHub.ChangeActionState(CharacterActionState.None);
            previousPatternIndex = currentPatternIndex;
            currentPatternIndex = (currentPatternIndex + 1) % areaUltimatePattern.Count;
            currentCooldown = 0;
            ultimateReady = false;
            ultimateUsed = false;
            ultimateCoroutine = null;
            
        }
        protected bool ConsumeEnergy(float amount)
        {
            if (!energyModule || !energyModule.HasEnoughEnergy(amount)) return false;
            energyModule.ChangeEnergy(-amount);
            return true;
        }
    }
}

