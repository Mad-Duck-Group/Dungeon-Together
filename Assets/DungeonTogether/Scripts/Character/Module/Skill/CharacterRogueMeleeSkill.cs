using System;
using System.Collections;
using System.Collections.Generic;
using DungeonTogether.Scripts.Manangers;
using DungeonTogether.Scripts.Utils;
using TriInspector;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.Serialization;

namespace DungeonTogether.Scripts.Character.Module.Skill
{
    [Serializable]
    public struct RogueSkillPattern
    {
        [Group("Area"), Required] public DamageArea damageArea;
        [Group("Damage"), Min(0)] public float damage;
        [Group("Timing"), Min(0)] public float delay;
        [Group("Timing"), Min(0)] public float duration;
        [Group("Timing"), Min(0)] public float cooldown;
        [Group("Timing"), Min(0)] public float resetComboTime;
        [Group("Dashing"), Min(0)] public float dashForce;
        [Group("Dashing")] public LayerMask ignoreLayer;
        [Group("Critical")] public float increaseCriticalChance;
        [Group("Critical")] public float increaseCriticalDamage;
        [Group("Critical")] public float increaseDuration;
        [Group("Cost"), Min(0)] public float mana;
        [Group("Energy"), Min(0)] public float getEnergy;
    }

    public struct RogueUltimateSkillEvent
    {
        public CharacterHub characterHub;
        public float duration;

        private static RogueUltimateSkillEvent _event;
        public static void Invoke(CharacterHub characterHub, float duration)
        {
            _event.characterHub = characterHub;
            _event.duration = duration;
            EventBus<RogueUltimateSkillEvent>.Invoke(_event);
        }
    }
    public class CharacterRogueMeleeSkill : CharacterModule
    {
        [Title("Settings")]
        [SerializeField] private Transform comboParent;
        [TableList(Draggable = true,
            HideAddButton = false,
            HideRemoveButton = false,
            AlwaysExpanded = false)]
        [SerializeField] private List<RogueSkillPattern> rougeAttackPatterns;

        [TableList(Draggable = true,
            HideAddButton = false,
            HideRemoveButton = false,
            AlwaysExpanded = false)]
        [SerializeField]
        private List<RogueSkillPattern> rougeUltimateSkillPatterns;
        
        [Title("Debug")]
        [SerializeField, DisplayAsString] private int currentPatternIndex;
        [SerializeField, DisplayAsString] private int previousPatternIndex = -1;
        [SerializeField, DisplayAsString] private bool skillReady;
        [SerializeField, DisplayAsString] private bool skillUsed;
        [SerializeField, DisplayAsString] private float currentCooldown;
        [SerializeField, DisplayAsString] private float currentComboTime;
        
        private RogueSkillPattern? CurrentPattern => currentPatterns[currentPatternIndex];
        private RogueSkillPattern? PreviousPattern
        {
            get
            {
                if (previousPatternIndex == -1) return null;
                return currentPatterns[previousPatternIndex];
            }
        }

        private List<RogueSkillPattern> currentPatterns = new();
        private CharacterMovementModule movementModule;
        private CharacterManaModule manaModule;
        private CharacterEnergyModule energyModule;
        private CharacterCriticalModule criticalModule;
        private Coroutine skillCoroutine;
        private Coroutine ultimateSkillCoroutine;

        public override void Initialize(CharacterHub characterHub)
        {
            base.Initialize(characterHub);
            currentPatterns = rougeAttackPatterns;
            currentPatternIndex = 0;
            currentCooldown = CurrentPattern?.cooldown ?? 0;
            previousPatternIndex = -1;
            rougeAttackPatterns.ForEach(pattern =>
            {
                pattern.damageArea.Initialize();
                pattern.damageArea.SetActive(false);
                pattern.damageArea.OnHitEvent += OnHit;
            });
            rougeUltimateSkillPatterns.ForEach(pattern =>
            {
                pattern.damageArea.Initialize();
                pattern.damageArea.SetActive(false);
                pattern.damageArea.OnHitEvent += OnHit;
            });
        }

        public override void PostInitialize()
        {
            base.PostInitialize();
            criticalModule = characterHub.FindModuleOfType<CharacterCriticalModule>();
            manaModule = characterHub.FindModuleOfType<CharacterManaModule>();
            movementModule = characterHub.FindModuleOfType<CharacterMovementModule>();
            energyModule = characterHub.FindModuleOfType<CharacterEnergyModule>();
        }

        public override void Shutdown()
        {
            base.Shutdown();
            currentPatterns = rougeAttackPatterns;
            currentPatternIndex = 0;
            currentCooldown = 0;
            previousPatternIndex = -1;
            rougeAttackPatterns.ForEach(pattern =>
            {
                pattern.damageArea.SetActive(false);
                pattern.damageArea.OnHitEvent -= OnHit;
            });
            rougeUltimateSkillPatterns.ForEach(pattern =>
            {
                pattern.damageArea.SetActive(false);
                pattern.damageArea.OnHitEvent -= OnHit;
            });
        }

        protected override void Subscribe()
        {
            base.Subscribe();
            EventBus<RogueUltimateSkillEvent>.Event += OnRogueUltimateSkillEvent;
        }

        protected override void Unsubscribe()
        {
            base.Unsubscribe();
            EventBus<RogueUltimateSkillEvent>.Event -= OnRogueUltimateSkillEvent;
        }

        private void OnRogueUltimateSkillEvent(RogueUltimateSkillEvent eventData)
        {
            if (eventData.characterHub != characterHub) return;
            if (ultimateSkillCoroutine != null) return;
            ultimateSkillCoroutine = StartCoroutine(UltimateSkillCoroutine(eventData.duration));
        }

        private IEnumerator UltimateSkillCoroutine(float duration)
        {
            currentPatterns = rougeUltimateSkillPatterns;
            currentPatternIndex = 0;
            currentCooldown = CurrentPattern?.cooldown ?? 0;
            previousPatternIndex = -1;
            yield return new WaitForSeconds(duration);
            currentPatterns = rougeAttackPatterns;
            currentPatternIndex = 0;
            currentCooldown = CurrentPattern?.cooldown ?? 0;
            previousPatternIndex = -1;
            ultimateSkillCoroutine = null;
        }

        /// <summary>
        /// Method called when the damage area hits a collider.
        /// </summary>
        /// <param name="collider">Collider that was hit.</param>
        protected virtual void OnHit(Collider2D collider)
        {
            if (!collider.TryGetComponent(out CharacterHub characterHub)) return;
            var healthModule = characterHub.FindModuleOfType<CharacterHealthModule>();
            if (!healthModule || CurrentPattern == null) return;
            var damage = -CurrentPattern.Value.damage;
            if (criticalModule)
            {
                criticalModule.CalculateCritical(ref damage);
            }
            healthModule.ChangeHealth(damage);
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
            if (!skillReady && currentCooldown < pattern.Value.cooldown)
            {
                currentCooldown += Time.deltaTime;
                
            }
            else
            {
                skillReady = true;
            }
            bool available = !skillUsed;
            UpdateSkillActionIcon(available, pattern);
        }
        
        private void UpdateSkillActionIcon(bool available, RogueSkillPattern? pattern = null)
        {
            if (characterHub.CharacterType is not CharacterType.Player) return;
            if (pattern != null)
            {
                float max = pattern.Value.cooldown;
                PlayerCanvasManager.Instance.UpdateSkillIcon(currentCooldown, max);
            }
            PlayerCanvasManager.Instance.SetAvailableSkill(available);
        }
        
        /// <summary>
        /// Method that triggers the attack.
        /// </summary>
        public virtual void CastSkill()
        {
            if (!ModulePermitted) return;
            if (!skillReady) return;
            if (skillCoroutine != null) return;
            skillCoroutine = StartCoroutine(SkillCoroutine());
        }
        
        /// <summary>
        /// Sets the direction of the skill.
        /// </summary>
        /// <param name="direction">Direction of the attack.</param>
        public virtual void SetSkillDirection(Vector2 direction)
        {
            if (!ModulePermitted) return;
            direction.Normalize();
            comboParent.right = direction;
        }

        /// <summary>
        /// Coroutine that handles the timing of the skill.
        /// </summary>
        /// <returns></returns>
        protected IEnumerator SkillCoroutine()
        {
            if (CurrentPattern == null) yield break;
            if (characterHub.CharacterType == CharacterType.Player && 
                (!ConsumeMana(CurrentPattern.Value.mana) || 
                characterHub.MovementState is CharacterMovementState.Dashing))
            {
                skillCoroutine = null;
                yield break;
            }
            skillUsed = true;
            currentComboTime = 0;
            characterHub.ChangeActionState(CharacterActionState.Skill);
            yield return new WaitForSeconds(CurrentPattern.Value.delay);
            CurrentPattern.Value.damageArea.SetActive(true);
            movementModule.Dash(comboParent.right, CurrentPattern.Value.dashForce, 
                CurrentPattern.Value.ignoreLayer, CurrentPattern.Value.duration);
            criticalModule.SetCriticalChance(CurrentPattern.Value.increaseCriticalChance, CurrentPattern.Value.increaseDuration);
            criticalModule.SetCriticalMultiplier(CurrentPattern.Value.increaseCriticalDamage, CurrentPattern.Value.increaseDuration);
            yield return new WaitForSeconds(CurrentPattern.Value.duration);
            CurrentPattern.Value.damageArea.SetActive(false);
            characterHub.ChangeActionState(CharacterActionState.None);
            previousPatternIndex = currentPatternIndex;
            currentPatternIndex = (currentPatternIndex + 1) % rougeAttackPatterns.Count;
            currentCooldown = 0;
            skillReady = false;
            skillUsed = false;
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
            if (!energyModule) return;
            energyModule.ChangeEnergy(+amount);
            return;
        }
    }
}
