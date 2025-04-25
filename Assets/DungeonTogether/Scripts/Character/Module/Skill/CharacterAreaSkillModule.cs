using System;
using System.Collections;
using System.Collections.Generic;
using DungeonTogether.Scripts.Manangers;
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
        [Group("Energy"), Min(0)] public float getEnergy;
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
        [SerializeField, DisplayAsString] protected bool skillUsed;
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
        private CharacterManaModule manaModule;
        private CharacterEnergyModule energyModule;
        
        public override void Initialize(CharacterHub characterHub)
        {
            base.Initialize(characterHub);
            currentPatternIndex = 0;
            if (CurrentPattern != null)
            {
                currentCooldown = CurrentPattern.Value.cooldown;
                UpdateSkillActionIcon(true, CurrentPattern.Value);
            }
            else
            {
                UpdateSkillActionIcon(false);
            }
            previousPatternIndex = -1;
            areaSkillPattern.ForEach(pattern =>
            {
                pattern.areaSkill.Initialize();
                pattern.areaSkill.SetActive(false);
                pattern.areaSkill.OnHitEvent += OnHit;
            });
        }
        
        public override void PostInitialize()
        {
            base.PostInitialize();
            manaModule = characterHub.FindModuleOfType<CharacterManaModule>();
            energyModule = characterHub.FindModuleOfType<CharacterEnergyModule>();
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
            GetEnergy(CurrentPattern.Value.getEnergy);
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
                UpdateSkillActionIcon(false);
                return;
            }
            base.UpdateModule();
        }

        private void UpdateCooldown()
        {
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
            }
            
            var pattern = PreviousPattern ?? CurrentPattern;
            if (pattern == null) return;
            bool hasMana = manaModule && manaModule.HasEnoughMana(pattern.Value.mana);
            bool coolingDown = false;
            if (!skillReady && currentCooldown < pattern.Value.cooldown)
            {
                currentCooldown += Time.deltaTime;
                skillReady = false;
                coolingDown = true;
            }
            else
            {
                skillReady = true;
            }
            bool available = (!skillUsed && skillReady && hasMana) || coolingDown;
            UpdateSkillActionIcon(available, pattern);
        }

        private void UpdateSkillActionIcon(bool available, AreaSkillPattern? pattern = null)
        {
            if (characterHub.CharacterType is not CharacterType.Player) return;
            if (pattern != null)
            {
                float max = pattern.Value.cooldown;
                PlayerCanvasManager.Instance.UpdateSkillIcon(currentCooldown, max);
            }
            PlayerCanvasManager.Instance.SetAvailableSkill(available);
        }

        public void CastSkill()
        {
            if (!ModulePermitted) return;
            if (!skillReady) return;
            if (skillCoroutine != null) return;
            skillCoroutine = StartCoroutine(CastSkillCoroutine());
        }
        protected IEnumerator CastSkillCoroutine()
        {
            if (CurrentPattern == null) yield break;
            if (characterHub.CharacterType == CharacterType.Player && !ConsumeMana(CurrentPattern.Value.mana))
            {
                skillCoroutine = null;
                yield break;
            }
            skillUsed = true;
            currentComboTime = 0;
            characterHub.ChangeActionState(CharacterActionState.Skill);
            yield return new WaitForSeconds(CurrentPattern.Value.delay);
            CurrentPattern.Value.areaSkill.Initialize();
            CurrentPattern.Value.areaSkill.SetActive(true);
            yield return new WaitForSeconds(CurrentPattern.Value.duration);
            CurrentPattern.Value.areaSkill.SetActive(false);
            characterHub.ChangeActionState(CharacterActionState.None);
            previousPatternIndex = currentPatternIndex;
            currentPatternIndex = (currentPatternIndex + 1) % areaSkillPattern.Count;
            skillUsed = false;
            currentCooldown = 0;
            skillReady = false;
            skillCoroutine = null;
        }
        private bool ConsumeMana(float amount)
        {
            if (!manaModule || !manaModule.HasEnoughMana(amount)) return false;
            manaModule.ChangeMana(-amount);
            return true;
        }
        
        protected virtual void GetEnergy(float amount)
        {
            if (!energyModule || energyModule.energyData.Value.currentEnergy > energyModule.energyData.Value.maxEnergy) return;
            energyModule.ChangeEnergy(+amount);
            return;
        }
    }
}

