using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.Utilities;

namespace DungeonTogether.Scripts.Character.Module
{
    [Serializable]
    public struct WizardSkillPattern
    {
        [Group("Area"), Required] public DamageArea damageArea;
        [Group("Damage"), Min(0)] public float damage;
        [Group("Timing"), Min(0)] public float delay;
        [Group("Timing"), Min(0)] public float duration;
        [Group("Timing"), Min(0)] public float cooldown;
        [Group("Timing"), Min(0)] public float resetComboTime;
        [Group("Cost"), Min(0)] public float mana;
    }
    public class CharacterWizardSkillModule : CharacterModule
    {
        [Title("Settings")]
        [SerializeField] private Transform comboParent;
        [TableList(Draggable = true,
            HideAddButton = false,
            HideRemoveButton = false,
            AlwaysExpanded = false)]
        [SerializeField] private List<WizardSkillPattern> wizardSkillPatterns;
        
        [Title("Debug")]
        [SerializeField, DisplayAsString] private int currentPatternIndex;
        [SerializeField, DisplayAsString] private int previousPatternIndex = -1;
        [SerializeField, DisplayAsString] private bool skillReady;
        [SerializeField, DisplayAsString] private float currentCooldown;
        [SerializeField, DisplayAsString] private float currentComboTime;
        
        private WizardSkillPattern? CurrentPattern => wizardSkillPatterns[currentPatternIndex];
        private WizardSkillPattern? PreviousPattern
        {
            get
            {
                if (previousPatternIndex == -1) return null;
                return wizardSkillPatterns[previousPatternIndex];
            }
        }
        private Coroutine skillCoroutine;
        
        public override void Initialize(CharacterHub characterHub)
        {
            base.Initialize(characterHub);
            currentPatternIndex = 0;
            currentCooldown = 0;
            previousPatternIndex = -1;
            wizardSkillPatterns.ForEach(pattern =>
            {
                pattern.damageArea.SetActive(false);
                pattern.damageArea.OnHitEvent += OnHit;
            });
        }

        public override void Shutdown()
        {
            base.Shutdown();
            currentPatternIndex = 0;
            currentCooldown = 0;
            previousPatternIndex = -1;
            wizardSkillPatterns.ForEach(pattern =>
            {
                pattern.damageArea.SetActive(false);
                pattern.damageArea.OnHitEvent -= OnHit;
            });
        }
        
        protected virtual void OnHit(Collider2D collider)
        {
            if (!collider.TryGetComponent(out CharacterHub characterHub)) return;
            var healthModule = characterHub.FindModuleOfType<CharacterHealthModule>();
            if (healthModule && CurrentPattern != null) 
                healthModule.ChangeHealth(-CurrentPattern.Value.damage);
        }
        protected override void HandleInput()
        {
            if (characterHub.CharacterType is not CharacterType.Player) return;
            base.HandleInput();
            if (PlayerInput.SkillButton.isDown)
            {
                CastSkill();
            }
            
        }
        protected override void UpdateModule()
        {
            if (!ModulePermitted) return;
            base.UpdateModule();
            if (PreviousPattern != null)
            {
                if (skillReady && currentComboTime < PreviousPattern.Value.resetComboTime)
                {
                    currentComboTime += Time.deltaTime;
                }
                if (currentComboTime >= PreviousPattern.Value.resetComboTime)
                {
                    currentComboTime = 0;
                    currentPatternIndex = 0;
                    previousPatternIndex = -1;
                }
                if (!skillReady && PreviousPattern != null &&currentCooldown < PreviousPattern.Value.cooldown)
                {
                    currentCooldown += Time.deltaTime;
                    return;
                }
            }
            skillReady = true;
            currentCooldown = 0;
        }
        
        private void CastSkill()
        {
            if (!ModulePermitted) return;
            if (!skillReady) return;
            if (skillCoroutine != null) return;
            skillCoroutine = StartCoroutine(CastSkillCoroutine());
        }
        protected IEnumerator CastSkillCoroutine()
        {
            if (CurrentPattern == null) yield break;
            currentComboTime = 0;
            ConsumeMana(CurrentPattern.Value.mana);
            characterHub.ChangeActionState(CharacterStates.CharacterActionState.Skill);
            yield return new WaitForSeconds(CurrentPattern.Value.delay);
            CurrentPattern.Value.damageArea.SetActive(true);
            yield return new WaitForSeconds(CurrentPattern.Value.duration);
            CurrentPattern.Value.damageArea.SetActive(false);
            characterHub.ChangeActionState(CharacterStates.CharacterActionState.None);
            previousPatternIndex = currentPatternIndex;
            currentPatternIndex = (currentPatternIndex + 1) % wizardSkillPatterns.Count;
            skillReady = false;
            skillCoroutine = null;
            
        }
        private void ConsumeMana(float amount)
        {
            var manaModule = characterHub.FindModuleOfType<CharacterManaModule>();
            if (!manaModule || manaModule.manaData.Value.currentMana < amount) return;
            manaModule.ChangeMana(-amount);
        }
    }
}

