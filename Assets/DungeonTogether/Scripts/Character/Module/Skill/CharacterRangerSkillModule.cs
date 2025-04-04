using System;
using System.Collections;
using System.Collections.Generic;
using DungeonTogether.Scripts.Manangers;
using TriInspector;
using Unity.Netcode;
using UnityEngine;

namespace DungeonTogether.Scripts.Character.Module.Skill
{
    [Serializable]
    public struct RangerSkillPattern 
    {
        [Group("FirePoint"), Required] public Transform firePoint;
        [Group("Arrow"), Required] public ProjectileDamageArea projectileDamageAreaPrefab;
        [Group("Arrow")] public float arrowCount;
        [Group("Damage"), Min(0)] public float damage;
        [Group("Damage"), Min(0)] public LayerMask passThroughLayer;
        [Group("Speed"), Min(0)] public float projectileSpeed;
        [Group("Critical")] public float increaseCriticalChance;
        [Group("Critical")] public float increaseCriticalDamage;
        [Group("Critical")] public float increaseDuration;
        [Group("Timing"), Min(0)] public float delay;
        [Group("Timing"), Min(0)] public bool hasDuration;
        [Group("Timing"), Min(0), ShowIf(nameof(hasDuration))] public float duration;
        [Group("Timing"), Min(0)] public float cooldown;
        [Group("Timing"), Min(0)] public float resetComboTime;
        [Group("Cost"), Min(0)] public float mana;
        [Group("Energy"), Min(0)] public float getEnergy;
    }
    public class CharacterRangerSkillModule : CharacterModule
    {
        [Title("Settings")]
        [SerializeField] private Transform comboParent;
        [TableList(Draggable = true,
            HideAddButton = false,
            HideRemoveButton = false,
            AlwaysExpanded = false)]
        [SerializeField] private List<RangerSkillPattern> rangerSkillPatterns;
        
        [Title("Debug")]
        [SerializeField, DisplayAsString] private int currentPatternIndex;
        [SerializeField, DisplayAsString] private int previousPatternIndex = -1;
        [SerializeField, DisplayAsString] private bool skillReady;
        [SerializeField, DisplayAsString] private bool skillUsed;
        [SerializeField, DisplayAsString] private float currentCooldown;
        [SerializeField, DisplayAsString] private float currentComboTime;

        private RangerSkillPattern? CurrentPattern => rangerSkillPatterns[currentPatternIndex];
        private RangerSkillPattern? PreviousPattern
        {
            get
            {
                if (previousPatternIndex == -1) return null;
                return rangerSkillPatterns[previousPatternIndex];
            }
        }
        private CharacterManaModule manaModule;
        private CharacterCriticalModule criticalModule;
        private CharacterEnergyModule energyModule;
        private Coroutine skillCoroutine;
        
        public override void Initialize(CharacterHub characterHub)
        {
            base.Initialize(characterHub);
            currentPatternIndex = 0;
            currentCooldown = CurrentPattern?.cooldown ?? 0;
            previousPatternIndex = -1;
        }
        public override void PostInitialize()
        {
            base.PostInitialize();
            manaModule = characterHub.FindModuleOfType<CharacterManaModule>();
            criticalModule = characterHub.FindModuleOfType<CharacterCriticalModule>();
            energyModule = characterHub.FindModuleOfType<CharacterEnergyModule>();
        }
        public override void Shutdown()
        {
            base.Shutdown();
            currentPatternIndex = 0;
            currentCooldown = 0;
            previousPatternIndex = -1;
        }
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
        
        private void UpdateSkillActionIcon(bool available, RangerSkillPattern? pattern = null)
        {
            if (characterHub.CharacterType is not CharacterType.Player) return;
            if (pattern != null)
            {
                float max = pattern.Value.cooldown;
                PlayerCanvasManager.Instance.UpdateSkillIcon(currentCooldown, max);
            }
            PlayerCanvasManager.Instance.SetAvailableSkill(available);
        }
        public virtual void CastSkill()
        {
            if (!ModulePermitted) return;
            if (!skillReady) return;
            if (skillCoroutine != null) return;
            skillCoroutine = StartCoroutine(SkillCoroutine());
        }
        
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
        protected IEnumerator SkillCoroutine()
        {
            if (CurrentPattern == null) yield break;
            if (!ConsumeMana(CurrentPattern.Value.mana) || 
                characterHub.MovementState is CharacterMovementState.Dashing)
            {
                skillCoroutine = null;
                yield break;
            }
            skillUsed = true;
            currentComboTime = 0;
            characterHub.ChangeActionState(CharacterActionState.Skill);
    
            yield return new WaitForSeconds(CurrentPattern.Value.delay);

            for (int i = 0; i < CurrentPattern.Value.arrowCount; i++)
            {
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
                    criticalModule.SetCriticalChance(CurrentPattern.Value.increaseCriticalChance, CurrentPattern.Value.increaseDuration);
                    criticalModule.SetCriticalMultiplier(CurrentPattern.Value.increaseCriticalDamage, CurrentPattern.Value.increaseDuration);
                    yield return new WaitForSeconds(CurrentPattern.Value.duration);
                    if (projectileDamageArea)
                    {
                        projectileDamageArea.SetActive(false);
                        projectileDamageArea.OnHitEvent -= OnRangeHit;
                    }
                }
                yield return new WaitForSeconds(CurrentPattern.Value.duration);
            }

            characterHub.ChangeActionState(CharacterActionState.None);
            previousPatternIndex = currentPatternIndex;
            currentPatternIndex = (currentPatternIndex + 1) % rangerSkillPatterns.Count;
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

