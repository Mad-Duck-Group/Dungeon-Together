using System;
using System.Collections;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;

namespace DungeonTogether.Scripts.Character.Module.Skill
{
    [Serializable]
    public struct AreaSkillPattern
    {
        [Group("Area"), Required] public DamageArea areaSkill;
        [Group("Value"), Min(0)] public float value;
        [Group("Timing"), Min(0)] public float delay;
        [Group("Timing"), Min(0)] public float duration;
        [Group("Timing"), Min(0)] public float cooldown;
        [Group("Timing"), Min(0)] public float resetComboTime;
        [Group("Cost"), Min(0)] public float mana;
    }
    public class CharacterAreaSkillModule : CharacterModule
    {
        [Title("Settings")]
        [SerializeField] protected Transform comboParent;
        [TableList(Draggable = true,
            HideAddButton = false,
            HideRemoveButton = false,
            AlwaysExpanded = false)]
        [SerializeField] protected List<AreaSkillPattern> areaSkillPattern;
        
        [Title("Debug")]
        [SerializeField, DisplayAsString] protected int currentPatternIndex;
        [SerializeField, DisplayAsString] protected int previousPatternIndex = -1;
        [SerializeField, DisplayAsString] protected bool skillReady;
        [SerializeField, DisplayAsString] protected float currentCooldown;
        [SerializeField, DisplayAsString] protected float currentComboTime;
        
        protected AreaSkillPattern? CurrentPattern => areaSkillPattern[currentPatternIndex];
        protected AreaSkillPattern? PreviousPattern
        {
            get
            {
                if (previousPatternIndex == -1) return null;
                return areaSkillPattern[previousPatternIndex];
            }
        }
        protected Coroutine skillCoroutine;
        
        public override void Initialize(CharacterHub characterHub)
        {
            base.Initialize(characterHub);
            currentPatternIndex = 0;
            currentCooldown = 0;
            previousPatternIndex = -1;
            areaSkillPattern.ForEach(pattern =>
            {
                pattern.areaSkill.SetActive(false);
                pattern.areaSkill.OnHitEvent += OnHit;
            });
        }

        public override void Shutdown()
        {
            base.Shutdown();
            currentPatternIndex = 0;
            currentCooldown = 0;
            previousPatternIndex = -1;
            areaSkillPattern.ForEach(pattern =>
            {
                pattern.areaSkill.SetActive(false);
                pattern.areaSkill.OnHitEvent -= OnHit;
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
            CurrentPattern.Value.areaSkill.SetActive(true);
            yield return new WaitForSeconds(CurrentPattern.Value.duration);
            CurrentPattern.Value.areaSkill.SetActive(false);
            characterHub.ChangeActionState(CharacterStates.CharacterActionState.None);
            previousPatternIndex = currentPatternIndex;
            currentPatternIndex = (currentPatternIndex + 1) % areaSkillPattern.Count;
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

