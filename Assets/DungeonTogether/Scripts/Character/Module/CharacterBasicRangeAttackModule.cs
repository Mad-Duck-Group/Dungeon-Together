using System;
using System.Collections;
using System.Collections.Generic;
using DungeonTogether.Scripts.Utils;
using TriInspector;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Serialization;

namespace DungeonTogether.Scripts.Character.Module
{
    [Serializable]
    public struct RangeAttackPattern 
    {
        [Group("FirePoint"), Required] public Transform firePoint;
        [Group("BulletPrefab"), Required] public GameObject bulletPrefab;
        [FormerlySerializedAs("rangeAttack")] [Group("RangeArea"), Required] public RangeAttack rangeAttackPrefab;
        [Group("Damage"), Min(0)] public float damage;
        [Group("Speed"), Min(0)] public float projectileSpeed;
        [Group("Timing"), Min(0)] public float delay;
        [Group("Timing"), Min(0)] public bool hasDuration;
        [Group("Timing"), Min(0), ShowIf(nameof(hasDuration))] public float duration;
        [Group("Timing"), Min(0)] public float interval;
        [Group("Timing"), Min(0)] public float resetComboTime;
    }
    public class CharacterBasicRangeAttackModule : CharacterModule
    {
        [Title("Settings")]
        [TableList(Draggable = true,
            HideAddButton = false,
            HideRemoveButton = false,
            AlwaysExpanded = false)]
        [SerializeField] private List<RangeAttackPattern> rangeAttackPatterns;
        
        [Title("Debug")]
        [SerializeField, DisplayAsString] private int currentPatternIndex;
        [SerializeField, DisplayAsString] private int previousPatternIndex = -1;
        [SerializeField, DisplayAsString] private bool attackReady;
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
        
        private Coroutine attackCoroutine;
        
        public override void Initialize(CharacterHub characterHub)
        {
            base.Initialize(characterHub);
            currentPatternIndex = 0;
            currentInterval = 0;
            previousPatternIndex = -1;
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
            if (healthModule && CurrentPattern != null) 
                healthModule.ChangeHealth(-CurrentPattern.Value.damage);
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
        /// Coroutine that handles the timing of the attack.
        /// </summary>
        /// <returns></returns>
        protected IEnumerator AttackCoroutine()
        {
            if (CurrentPattern == null) yield break;
            currentComboTime = 0;
            CharacterStates.ActionStateEvent.Invoke(characterHub, characterHub.ActionState,
                CharacterStates.CharacterActionState.Basic);
            yield return new WaitForSeconds(CurrentPattern.Value.delay);
            //Create bullet
            Debug.Log("Bullet has spawn");
            RangeAttack rangeAttack = Instantiate(CurrentPattern.Value.bulletPrefab , CurrentPattern.Value.firePoint.transform.position, Quaternion.identity).GetComponent<RangeAttack>();
            rangeAttack.SetDirection(CurrentPattern.Value.firePoint.right, CurrentPattern.Value.projectileSpeed);
            rangeAttack.OnHitEvent += OnRangeHit;
            if (CurrentPattern.Value.hasDuration)
            {
                rangeAttack.SetActive(true);
                yield return new WaitForSeconds(CurrentPattern.Value.duration);
                rangeAttack.SetActive(false);
            }
            CharacterStates.ActionStateEvent.Invoke(characterHub, characterHub.ActionState,
                CharacterStates.CharacterActionState.None);
            previousPatternIndex = currentPatternIndex;
            currentPatternIndex = (currentPatternIndex + 1) % rangeAttackPatterns.Count;
            attackReady = false;
            attackCoroutine = null;
        }
    }
}
