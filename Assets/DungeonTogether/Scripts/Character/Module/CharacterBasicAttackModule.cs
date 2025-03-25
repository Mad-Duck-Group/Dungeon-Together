using System;
using System.Collections;
using System.Collections.Generic;
using DungeonTogether.Scripts.Manangers;
using TriInspector;
using UnityEngine;

namespace DungeonTogether.Scripts.Character.Module
{
    [Serializable]
    public struct BasicAttackPattern
    {
        [Group("Area"), Required] public DamageArea damageArea;
        [Group("Damage"), Min(0)] public float damage;
        [Group("Timing"), Min(0)] public float delay;
        [Group("Timing"), Min(0)] public float duration;
        [Group("Timing"), Min(0)] public float interval;
        [Group("Timing"), Min(0)] public float resetComboTime;
        [Group("Energy"), Min(0)] public float getEnergy;
    }
    /// <summary>
    /// Module responsible for handling basic attacks.
    /// </summary>
    public class CharacterBasicAttackModule : CharacterModule
    {
        [Title("Settings")]
        [SerializeField] private Transform comboParent;
        [TableList(Draggable = true,
            HideAddButton = false,
            HideRemoveButton = false,
            AlwaysExpanded = false)]
        [SerializeField] private List<BasicAttackPattern> basicAttackPatterns;
        
        [Title("Debug")]
        [SerializeField, DisplayAsString] private int currentPatternIndex;
        [SerializeField, DisplayAsString] private int previousPatternIndex = -1;
        [SerializeField, DisplayAsString] private bool attackReady;
        [SerializeField, DisplayAsString] private bool attackUsed;
        [SerializeField, DisplayAsString] private float currentInterval;
        [SerializeField, DisplayAsString] private float currentComboTime;
        
        private BasicAttackPattern? CurrentPattern => basicAttackPatterns[currentPatternIndex];
        private BasicAttackPattern? PreviousPattern
        {
            get
            {
                if (previousPatternIndex == -1) return null;
                return basicAttackPatterns[previousPatternIndex];
            }
        }

        private CharacterCriticalModule criticalModule;
        private Coroutine attackCoroutine;

        public override void Initialize(CharacterHub characterHub)
        {
            base.Initialize(characterHub);
            currentPatternIndex = 0;
            currentInterval = 0;
            previousPatternIndex = -1;
            basicAttackPatterns.ForEach(pattern =>
            {
                pattern.damageArea.Initialize();
                pattern.damageArea.SetActive(false);
                pattern.damageArea.OnHitEvent += OnHit;
            });
            criticalModule = characterHub.FindModuleOfType<CharacterCriticalModule>();
        }

        public override void Shutdown()
        {
            base.Shutdown();
            currentPatternIndex = 0;
            currentInterval = 0;
            previousPatternIndex = -1;
            basicAttackPatterns.ForEach(pattern =>
            {
                pattern.damageArea.SetActive(false);
                pattern.damageArea.OnHitEvent -= OnHit;
            });
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
            if (PlayerInput.AttackButton.isDown)
            {
                Attack();
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
                UpdateBasicAttackActionIcon(false);
                return;
            }
            base.UpdateModule();
        }

        private void UpdateCooldown()
        {
            if (PreviousPattern != null)
            {
                if (attackReady && currentComboTime < PreviousPattern.Value.resetComboTime)
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
            if (!attackReady && currentInterval < pattern.Value.interval)
            {
                currentInterval += Time.deltaTime;
                
            }
            else
            {
                attackReady = true;
            }
            bool available = !attackUsed;
            UpdateBasicAttackActionIcon(available, pattern);
        }
        
        private void UpdateBasicAttackActionIcon(bool available, BasicAttackPattern? pattern = null)
        {
            if (characterHub.CharacterType is not CharacterType.Player) return;
            if (pattern != null)
            {
                float max = pattern.Value.interval;
                PlayerCanvasManager.Instance.UpdateBasicAttackIcon(currentInterval, max);
            }
            PlayerCanvasManager.Instance.SetAvailableBasicAttack(available);
        }
        
        /// <summary>
        /// Method that triggers the attack.
        /// </summary>
        public virtual void Attack()
        {
            if (!ModulePermitted) return;
            if (!attackReady) return;
            if (attackCoroutine != null) return;
            attackCoroutine = StartCoroutine(AttackCoroutine());
        }
        
        /// <summary>
        /// Sets the direction of the attack.
        /// </summary>
        /// <param name="direction">Direction of the attack.</param>
        public virtual void SetAttackDirection(Vector2 direction)
        {
            if (!ModulePermitted) return;
            direction.Normalize();
            comboParent.right = direction;
        }

        /// <summary>
        /// Coroutine that handles the timing of the attack.
        /// </summary>
        /// <returns></returns>
        protected IEnumerator AttackCoroutine()
        {
            if (CurrentPattern == null) yield break;
            attackUsed = true;
            currentComboTime = 0;
            characterHub.ChangeActionState(CharacterActionState.Basic);
            yield return new WaitForSeconds(CurrentPattern.Value.delay);
            CurrentPattern.Value.damageArea.SetActive(true);
            yield return new WaitForSeconds(CurrentPattern.Value.duration);
            CurrentPattern.Value.damageArea.SetActive(false);
            characterHub.ChangeActionState(CharacterActionState.None);
            previousPatternIndex = currentPatternIndex;
            currentPatternIndex = (currentPatternIndex + 1) % basicAttackPatterns.Count;
            currentInterval = 0;
            attackReady = false;
            attackUsed = false;
            attackCoroutine = null;
        }

        protected virtual void GetEnergy(float amount)
        {
            var energyModule = characterHub.FindModuleOfType<CharacterEnergyModule>();
            if (!energyModule || energyModule.energyData.Value.currentEnergy > energyModule.energyData.Value.maxEnergy) return;
            energyModule.ChangeEnergy(+amount);
            return;
        }
    }
}
