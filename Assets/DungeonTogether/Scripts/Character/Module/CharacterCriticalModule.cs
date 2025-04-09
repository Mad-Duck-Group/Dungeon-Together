using System;
using System.Collections;
using TriInspector;
using UnityEngine;

namespace DungeonTogether.Scripts.Character.Module
{
    public class CharacterCriticalModule : CharacterModule
    {
        [Title("Critical Settings")]
        [SerializeField] private float baseCriticalChance = 0.1f;
        [SerializeField] private float baseCriticalMultiplier = 2f;
        
        [Title("Debug")]
        [SerializeField, ReadOnly] private float currentCriticalChance;
        [SerializeField, ReadOnly] private float currentCriticalMultiplier;
        
        private Coroutine _temporalChangeCriticalChanceCoroutine;
        private Coroutine _temporalChangeCriticalMultiplierCoroutine;
        
        public override void Initialize(CharacterHub characterHub)
        {
            base.Initialize(characterHub);
            currentCriticalChance = baseCriticalChance;
            currentCriticalMultiplier = baseCriticalMultiplier;
        }
        
        public void SetCriticalChance(float newCriticalChance)
        {
            currentCriticalChance = newCriticalChance;
        }
        
        public void SetCriticalChance(float newCriticalChance, float duration)
        {
            if (_temporalChangeCriticalChanceCoroutine != null)
            {
                StopCoroutine(_temporalChangeCriticalChanceCoroutine);
                ResetCriticalChance();
            }
            _temporalChangeCriticalChanceCoroutine = 
                StartCoroutine(TemporalChangeCriticalChance(newCriticalChance, duration));
        }
        
        public void ResetCriticalChance()
        {
            currentCriticalChance = baseCriticalChance;
        }
        
        public void SetCriticalMultiplier(float newCriticalMultiplier)
        {
            currentCriticalMultiplier = newCriticalMultiplier;
        }
        
        public void SetCriticalMultiplier(float newCriticalMultiplier, float duration)
        {
            if (_temporalChangeCriticalMultiplierCoroutine != null)
            {
                StopCoroutine(_temporalChangeCriticalMultiplierCoroutine);
                ResetCriticalMultiplier();
            }
            _temporalChangeCriticalMultiplierCoroutine = 
                StartCoroutine(TemporalChangeCriticalMultiplier(newCriticalMultiplier, duration));
        }
        
        public void ResetCriticalMultiplier()
        {
            currentCriticalMultiplier = baseCriticalMultiplier;
        }

        public bool CalculateCritical(ref float damage)
        {
            if (UnityEngine.Random.value < currentCriticalChance)
            {
                damage *= currentCriticalMultiplier;
                Debug.Log($"{characterHub.name} critical hit! Damage: {damage}");
                return true;
            }
            return false;
        }
        
        private IEnumerator TemporalChangeCriticalChance(float newCriticalChance, float duration)
        {
            var previousCriticalChance = currentCriticalChance;
            currentCriticalChance = newCriticalChance;
            yield return new WaitForSeconds(duration);
            currentCriticalChance = previousCriticalChance;
            _temporalChangeCriticalChanceCoroutine = null;
        }
        
        private IEnumerator TemporalChangeCriticalMultiplier(float newCriticalMultiplier, float duration)
        {
            var previousCriticalMultiplier = currentCriticalMultiplier;
            currentCriticalMultiplier = newCriticalMultiplier;
            yield return new WaitForSeconds(duration);
            currentCriticalMultiplier = previousCriticalMultiplier;
            _temporalChangeCriticalMultiplierCoroutine = null;
        }
        
    }
}
