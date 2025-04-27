using System;
using System.Collections;
using System.Collections.Generic;
using DungeonTogether.Scripts.Manangers;
using DungeonTogether.Scripts.Utils;
using TriInspector;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace DungeonTogether.Scripts.Character.Module
{
    [Serializable]
    public struct RangeAttackPattern 
    {
        [Group("FirePoint"), Required] public Transform firePoint;
        [Group("RangeArea"), Required] public ProjectileDamageArea projectileDamageAreaPrefab;
        [Group("Damage"), Min(0)] public float damage;
        [Group("Damage"), Min(0)] public LayerMask passThroughLayer;
        [Group("Speed"), Min(0)] public float projectileSpeed;
        [Group("Timing"), Min(0)] public float delay;
        [Group("Timing"), Min(0)] public bool hasDuration;
        [Group("Timing"), Min(0), ShowIf(nameof(hasDuration))] public float duration;
        [Group("Timing"), Min(0)] public float interval;
        [Group("Timing"), Min(0)] public float resetComboTime;
        [Group("Energy"), Min(0)] public float getEnergy;
    }
    public class CharacterBasicRangeAttackModule : CharacterModule
    {
        [Title("Settings")]
        [SerializeField] private Transform comboParent;
        [TableList(Draggable = true,
            HideAddButton = false,
            HideRemoveButton = false,
            AlwaysExpanded = false)]
        [SerializeField] private List<RangeAttackPattern> rangeAttackPatterns;
        
        [Title("Debug")]
        [SerializeField, DisplayAsString] private int currentPatternIndex;
        [SerializeField, DisplayAsString] private int previousPatternIndex = -1;
        [SerializeField, DisplayAsString] private bool attackReady;
        [SerializeField, DisplayAsString] private bool attackUsed;
        [SerializeField, DisplayAsString] private float currentInterval;
        [SerializeField, DisplayAsString] private float currentComboTime;

        private RangeAttackPattern? CurrentPattern => rangeAttackPatterns[currentPatternIndex];
        private RangeAttackPattern? PreviousPattern
        {
            get
            {
                if (previousPatternIndex == -1) return null;
                return rangeAttackPatterns[previousPatternIndex];
            }
        }
        
        private CharacterCriticalModule criticalModule;
        private CharacterEnergyModule energyModule;
        private Coroutine attackCoroutine;

        public override void Initialize(CharacterHub characterHub)
        {
            base.Initialize(characterHub);
            currentPatternIndex = 0;
            currentInterval = 0;
            previousPatternIndex = -1;
        }

        public override void PostInitialize()
        {
            base.PostInitialize();
            criticalModule = characterHub.FindModuleOfType<CharacterCriticalModule>();
            energyModule = characterHub.FindModuleOfType<CharacterEnergyModule>();
        }
        
        public override void Shutdown()
        {
            base.Shutdown();
            currentPatternIndex = 0;
            currentInterval = 0;
            previousPatternIndex = -1;
        }

        /// <summary>
        /// Method called when the damage area hits a collider.
        /// </summary>
        /// <param name="collider">Collider that was hit.</param>
        protected virtual void OnRangeHit(Collider2D collider)
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
        
        // Input
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
        
        private void UpdateBasicAttackActionIcon(bool available, RangeAttackPattern? pattern = null)
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

        [Rpc(SendTo.ClientsAndHost)]
        private void SpawnProjectileRPC()
        {
            if (IsOwner)
            { return; }
        
            SpawnProjectile();
        }
        
        private void SpawnProjectile()
        {
            ProjectileDamageArea projectileDamageArea = 
                Instantiate(CurrentPattern.Value.projectileDamageAreaPrefab, CurrentPattern.Value.firePoint.transform.position, Quaternion.identity);
            projectileDamageArea.SetPassThroughLayer(CurrentPattern.Value.passThroughLayer);
            projectileDamageArea.SetDirection(CurrentPattern.Value.firePoint.right, CurrentPattern.Value.projectileSpeed);
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
            //Create bullet
            Debug.Log("Bullet has spawn");
            
            SpawnProjectileRPC();

            ProjectileDamageArea projectileDamageArea = Instantiate(CurrentPattern.Value.projectileDamageAreaPrefab,
                CurrentPattern.Value.firePoint.transform.position, Quaternion.identity);
            projectileDamageArea.SetPassThroughLayer(CurrentPattern.Value.passThroughLayer);
            projectileDamageArea.SetDirection(CurrentPattern.Value.firePoint.right, CurrentPattern.Value.projectileSpeed);
            projectileDamageArea.OnHitEvent += OnRangeHit;
            if (CurrentPattern.Value.hasDuration)
            {
                projectileDamageArea.Initialize();
                projectileDamageArea.SetActive(true);
                yield return new WaitForSeconds(CurrentPattern.Value.duration);
                projectileDamageArea.SetActive(false);
            }
            
            characterHub.ChangeActionState(CharacterActionState.None);
            previousPatternIndex = currentPatternIndex;
            currentPatternIndex = (currentPatternIndex + 1) % rangeAttackPatterns.Count;
            currentInterval = 0;
            attackReady = false;
            attackUsed = false;
            attackCoroutine = null;
        }
        
        protected virtual void GetEnergy(float amount)
        {
            if (!energyModule) return;
            energyModule.ChangeEnergy(+amount);
            return;
        }
    }
}
