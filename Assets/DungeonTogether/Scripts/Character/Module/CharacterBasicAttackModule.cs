using System;
using System.Collections;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;

namespace DungeonTogether.Scripts.Character.Module
{
    [Serializable]
    public struct BasicAttackPattern
    {
        [Required] public DamageArea damageArea;
        public float damage;
        public float delay;
        public float duration;
        public float interval;
        public float resetComboTime;
    }
    public class CharacterBasicAttackModule : CharacterModule
    {
        [Title("Settings")]
        [SerializeField] private List<BasicAttackPattern> basicAttackPatterns;
        
        [Title("Debug")]
        [SerializeField, DisplayAsString] private int currentPatternIndex;
        [SerializeField, DisplayAsString] private int previousPatternIndex = -1;
        [SerializeField, DisplayAsString] private bool attackReady;
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

        private Coroutine attackCoroutine;

        public override void Initialize(CharacterHub characterHub)
        {
            base.Initialize(characterHub);
            currentPatternIndex = 0;
            currentInterval = 0;
            previousPatternIndex = -1;
            basicAttackPatterns.ForEach(pattern =>
            {
                pattern.damageArea.SetActive(false);
                pattern.damageArea.OnHitEvent += OnHit;
            });
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

        private void OnHit(Collider2D collider)
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
            if (PlayerInput.AttackButton.isDown)
            {
                Attack();
            }
        }

        protected override void UpdateModule()
        {
            if (!ModulePermitted) return;
            base.UpdateModule();
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
                if (!attackReady && currentInterval < PreviousPattern.Value.interval)
                {
                    currentInterval += Time.deltaTime;
                    return;
                }
            }
            attackReady = true;
            currentInterval = 0;
        }
        
        public virtual void Attack()
        {
            if (!ModulePermitted) return;
            if (!attackReady) return;
            if (attackCoroutine != null) return;
            attackCoroutine = StartCoroutine(AttackCoroutine());
        }

        protected IEnumerator AttackCoroutine()
        {
            if (CurrentPattern == null) yield break;
            currentComboTime = 0;
            yield return new WaitForSeconds(CurrentPattern.Value.delay);
            CurrentPattern.Value.damageArea.SetActive(true);
            yield return new WaitForSeconds(CurrentPattern.Value.duration);
            CurrentPattern.Value.damageArea.SetActive(false);
            previousPatternIndex = currentPatternIndex;
            currentPatternIndex = (currentPatternIndex + 1) % basicAttackPatterns.Count;
            attackReady = false;
            attackCoroutine = null;
        }
    }
}
