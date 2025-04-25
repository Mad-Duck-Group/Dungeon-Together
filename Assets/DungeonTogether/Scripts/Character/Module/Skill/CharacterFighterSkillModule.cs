using System;
using System.Collections;
using System.Collections.Generic;
using DungeonTogether.Scripts.Manangers;
using DungeonTogether.Scripts.Utils;
using TriInspector;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace DungeonTogether.Scripts.Character.Module.Skill
{
    [Serializable]
    public struct FighterSkillPattern
    {
        [Group("Spawn Point"), Required] public Transform spawnPoint;
        [Group("Sword"), Required] public DamageArea damageArea;
        [Group("Sword"), Min(0)] public float swingAngle;
        [Group("Damage"), Min(0)] public float damage;
        [Group("Damage"), Min(0)] public LayerMask passThroughLayer;
        [Group("Timing"), Min(0)] public float delay;
        [Group("Timing"), Min(0)] public float duration;
        [Group("Timing"), Min(0)] public float cooldown;
        [Group("Timing"), Min(0)] public float resetComboTime;
        [Group("Cost"), Min(0)] public float mana;
        [Group("Energy"), Min(0)] public float getEnergy;
    }
    public class CharacterFighterSkillModule : CharacterModule
    {
        [Title("References")]
        [SerializeField] private RotateToMouse rotateToMouse;
        
        [Title("Settings")]
        [SerializeField] private Transform comboParent;
        [TableList(Draggable = true,
            HideAddButton = false,
            HideRemoveButton = false,
            AlwaysExpanded = false)]
        [SerializeField] private List<FighterSkillPattern> fighterSkillPatterns;
        
        [Title("Debug")]
        [SerializeField, DisplayAsString] private int currentPatternIndex;
        [SerializeField, DisplayAsString] private int previousPatternIndex = -1;
        [SerializeField, DisplayAsString] private bool skillReady;
        [SerializeField, DisplayAsString] private bool skillUsed;
        [SerializeField, DisplayAsString] private float currentCooldown;
        [SerializeField, DisplayAsString] private float currentComboTime;

        private FighterSkillPattern? CurrentPattern => fighterSkillPatterns[currentPatternIndex];

        private FighterSkillPattern? PreviousPattern
        {
            get
            {
                if (previousPatternIndex == -1) return null;
                return fighterSkillPatterns[previousPatternIndex];
            }
        }
        private CharacterCriticalModule criticalModule;
        private CharacterManaModule manaModule;
        private CharacterEnergyModule energyModule;
        private Coroutine skillCoroutine;

        public override void Initialize(CharacterHub characterHub)
        {
            base.Initialize(characterHub);
            currentPatternIndex = 0;
            currentCooldown = CurrentPattern?.cooldown ?? 0;
            previousPatternIndex = -1;
            fighterSkillPatterns.ForEach(pattern =>
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
            
            energyModule = characterHub.FindModuleOfType<CharacterEnergyModule>();
        }
        public override void Shutdown()
        {
            base.Shutdown();
            currentPatternIndex = 0;
            currentCooldown = 0;
            previousPatternIndex = -1;
            fighterSkillPatterns.ForEach(pattern =>
            {
                pattern.damageArea.SetActive(false);
                pattern.damageArea.OnHitEvent -= OnHit;
            });
        }
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
        private void UpdateSkillActionIcon(bool available, FighterSkillPattern? pattern = null)
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
        public virtual void SetSkillDirection(Vector2 direction)
        {
            if (!ModulePermitted) return;
            direction.Normalize();
            comboParent.right = direction;
        }
        protected IEnumerator SkillCoroutine()
        {
            if (CurrentPattern == null) yield break;
            if (!ConsumeMana(CurrentPattern.Value.mana))
            {
                skillCoroutine = null;
                yield break;
            }

            skillUsed = true;
            currentComboTime = 0;
            characterHub.ChangeActionState(CharacterActionState.Skill);
            yield return new WaitForSeconds(CurrentPattern.Value.delay);
            var damageArea = CurrentPattern.Value.damageArea;
            damageArea.SetActive(true);
            rotateToMouse.SetActive(false);
            SwingSwordRpc(CurrentPattern.Value.swingAngle, CurrentPattern.Value.duration);
            yield return new WaitForSeconds(CurrentPattern.Value.duration);
            characterHub.ChangeActionState(CharacterActionState.None);
            rotateToMouse.SetActive(true);
            damageArea.SetActive(false);
            previousPatternIndex = currentPatternIndex;
            currentPatternIndex = (currentPatternIndex + 1) % fighterSkillPatterns.Count;
            currentCooldown = 0;
            skillReady = false;
            skillUsed = false;
            skillCoroutine = null;
        }

        [Rpc(SendTo.Everyone)]
        private void SwingSwordRpc(float swingAngle, float duration)
        {
            if (CurrentPattern == null) return;
            var swordObj = CurrentPattern.Value.damageArea;
            StartCoroutine(SwingSword(swordObj.transform, swingAngle, duration));
        }

        private IEnumerator SwingSword(Transform sword, float swingAngle, float duration)
        {
            float timer = 0f;
            float startAngle = swingAngle / 2; 
            float endAngle = -swingAngle / 2;
            float angle;
            while (timer < duration)
            {
                angle = Mathf.Lerp(startAngle, endAngle, timer / duration); 
                sword.localRotation = Quaternion.Euler(0, 0, angle);
                timer += Time.deltaTime;
                yield return null;
            }
            angle = Mathf.Lerp(startAngle, endAngle, 1f); 
            sword.localRotation = Quaternion.Euler(0, 0, angle);
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
