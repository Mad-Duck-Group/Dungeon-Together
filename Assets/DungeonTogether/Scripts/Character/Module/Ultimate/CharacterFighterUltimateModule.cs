using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using DungeonTogether.Scripts.Character.Module.Skill;
using DungeonTogether.Scripts.Manangers;
using TriInspector;


namespace DungeonTogether.Scripts.Character.Module.Ultimate
{
    [Serializable]
    public struct FighterUltimatePattern
    {
        [Group("Critical")] public float increaseCriticalChance;
        [Group("Critical")] public float increaseCriticalDamage;
        [Group("Critical")] public float increaseDuration;
        [Group("Timing"), Min(0)] public float delay;
        [Group("Timing"), Min(0)] public float duration;
        [Group("Timing"), Min(0)] public float cooldown;
        [Group("Timing"), Min(0)] public float resetComboTime;
        [Group("Cost"), Min(0)] public float energy;
        
    }
    public class CharacterFighterUltimateModule : CharacterModule
    {
        [Title("Settings")]
        [SerializeField] private Transform comboParent;

        [TableList(Draggable = true,
            HideAddButton = false,
            HideRemoveButton = false,
            AlwaysExpanded = false)]
        [SerializeField] private List<FighterUltimatePattern> fighterUltimatePatterns;

        [Title("Debug")]
        [SerializeField, DisplayAsString] protected int currentPatternIndex;
        [SerializeField, DisplayAsString] protected int previousPatternIndex = -1;
        [SerializeField, DisplayAsString] protected bool ultimateReady;
        [SerializeField, DisplayAsString] protected bool ultimateUsed;
        [SerializeField, DisplayAsString] protected float currentCooldown;
        [SerializeField, DisplayAsString] protected float currentComboTime;
        
        protected FighterUltimatePattern? CurrentPattern => fighterUltimatePatterns[currentPatternIndex];
        protected FighterUltimatePattern? PreviousPattern
        {
            get
            {
                if (previousPatternIndex == -1) return null;
                return fighterUltimatePatterns[previousPatternIndex];
            }
        }
        
        private CharacterEnergyModule energyModule;
        private CharacterBasicAttackModule basicAttackModule;
        private CharacterFighterSkillModule skillModule;
        private Coroutine ultimateCoroutine;

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
        }
        public override void PostInitialize()
        {
            base.PostInitialize();
            energyModule = characterHub.FindModuleOfType<CharacterEnergyModule>();
            basicAttackModule = characterHub.FindModuleOfType<CharacterBasicAttackModule>();
            skillModule = characterHub.FindModuleOfType<CharacterFighterSkillModule>();
        }
        public override void Shutdown()
        {
            base.Shutdown();
            currentPatternIndex = 0;
            currentCooldown = 0;
            previousPatternIndex = -1;
            
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
        
        private void UpdateUltimateActionIcon(bool available, FighterUltimatePattern? pattern = null)
        {
            if (characterHub.CharacterType is not CharacterType.Player) return;
            if (pattern != null)
            {
                float max = pattern.Value.cooldown;
                PlayerCanvasManager.Instance.UpdateUltimateIcon(currentCooldown, max);
            }
            PlayerCanvasManager.Instance.SetAvailableUltimate(available);
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
            if (!ConsumeEnergy(CurrentPattern.Value.energy))
            {
                ultimateCoroutine = null;
                yield break;
            }

            currentComboTime = 0;
            ultimateUsed = true;
            characterHub.ChangeActionState(CharacterActionState.Ultimate);
            yield return new WaitForSeconds(CurrentPattern.Value.delay);
            //change color
            basicAttackModule.ApplyCriticalBuff(CurrentPattern.Value.increaseCriticalChance, CurrentPattern.Value.increaseCriticalDamage,CurrentPattern.Value.increaseDuration);
            skillModule.ApplyCriticalBuff(CurrentPattern.Value.increaseCriticalChance, CurrentPattern.Value.increaseCriticalDamage,CurrentPattern.Value.increaseDuration);
            yield return new WaitForSeconds(CurrentPattern.Value.duration);
            characterHub.ChangeActionState(CharacterActionState.None);
            previousPatternIndex = currentPatternIndex;
            currentPatternIndex = (currentPatternIndex + 1) % fighterUltimatePatterns.Count;
            currentCooldown = 0;
            ultimateReady = false;
            ultimateUsed = false;
            ultimateCoroutine = null;
        }
        
        private bool ConsumeEnergy(float amount)
        {
            if (!energyModule || !energyModule.HasEnoughEnergy(amount)) return false;
            energyModule.ChangeEnergy(-amount);
            return true;
        }
        
    }
}


