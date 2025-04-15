using System;
using DungeonTogether.Scripts.UI;
using MoreMountains.Tools;
using TriInspector;
using Unity.Netcode;
using UnityCommunity.UnitySingleton;
using UnityEngine;

namespace DungeonTogether.Scripts.Manangers
{
    public class PlayerCanvasManager : MonoSingleton<PlayerCanvasManager>
    {
        [Title("References")]
        [SerializeField, Required] private CanvasGroup playerCanvas;
        [SerializeField, Required] private MMHealthBar healthBar;
        [SerializeField, Required] private MMHealthBar manaBar;
        [SerializeField, Required] private MMHealthBar energyBar;
        [SerializeField, Required] private ActionIcon basicAttackIcon;
        [SerializeField, Required] private ActionIcon skillIcon;
        [SerializeField, Required] private ActionIcon ultimateIcon;


        private void OnEnable()
        {
            ClassSelector.OnCharacterSpawned += OnCharacterSpawned;
        }

        private void OnDisable()
        {
            ClassSelector.OnCharacterSpawned -= OnCharacterSpawned;
        }

        private void OnCharacterSpawned(ulong id)
        {
            Debug.Log($"OnCharacterSpawned called with id: {id}");
            if (id == NetworkManager.Singleton.LocalClientId)
            {
                SetActiveCanvas(true);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            SetActiveCanvas(false);
            SetAvailableBasicAttack(false);
            SetAvailableSkill(false);
            SetAvailableUltimate(false);
        }

        private void SetActiveCanvas(bool active)
        {
            playerCanvas.gameObject.SetActive(active);
        }
        
        public void UpdateManaBar(float currentMana, float maxMana, bool bump = false)
        {
            //Debug.Log($"Updating mana bar with current mana: {currentMana}, max mana: {maxMana}");
            var targetProgressBar = manaBar.TargetProgressBar;
            targetProgressBar.BumpScaleOnChange = bump;
            targetProgressBar.LerpForegroundBar = bump;
            targetProgressBar.LerpDecreasingDelayedBar = bump;
            targetProgressBar.LerpIncreasingDelayedBar = bump;
            manaBar.UpdateBar(currentMana, 0, maxMana, true);
        }
        
        public void UpdateHealthBar(float currentHealth, float maxHealth, bool bump = false)
        {
            var targetProgressBar = healthBar.TargetProgressBar;
            targetProgressBar.BumpScaleOnChange = bump;
            targetProgressBar.LerpForegroundBar = bump;
            targetProgressBar.LerpDecreasingDelayedBar = bump;
            targetProgressBar.LerpIncreasingDelayedBar = bump;
            healthBar.UpdateBar(currentHealth, 0, maxHealth, true);
        }
        
        public void UpdateEnergyBar(float currentEnergy, float maxEnergy, bool bump = false)
        {
            Debug.Log($"Updating energy bar with current energy: {currentEnergy}, max energy: {maxEnergy}");
            var targetProgressBar = energyBar.TargetProgressBar;
            targetProgressBar.BumpScaleOnChange = bump;
            targetProgressBar.LerpForegroundBar = bump;
            targetProgressBar.LerpDecreasingDelayedBar = bump;
            targetProgressBar.LerpIncreasingDelayedBar = bump;
            energyBar.UpdateBar(currentEnergy, 0, maxEnergy, true);
        }
        
        public void SetAvailableBasicAttack(bool available)
        {
            basicAttackIcon.SetAvailable(available);
        }
        
        public void SetAvailableSkill(bool available)
        {
            skillIcon.SetAvailable(available);
        }
        
        public void SetAvailableUltimate(bool available)
        {
            ultimateIcon.SetAvailable(available);
        }
        
        public void UpdateBasicAttackIcon(float current, float max)
        {
            basicAttackIcon.SetProgress(current, max);
            basicAttackIcon.SetTimer(max - current);
        }
        
        public void UpdateSkillIcon(float current, float max)
        {
            skillIcon.SetProgress(current, max);
            skillIcon.SetTimer(max - current);
        }

        public void UpdateUltimateIcon(float current, float max)
        {
            ultimateIcon.SetProgress(current, max);
            ultimateIcon.SetTimer(max - current);
        }
    }
}
