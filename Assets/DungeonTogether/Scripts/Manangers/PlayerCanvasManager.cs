using MoreMountains.Tools;
using UnityCommunity.UnitySingleton;
using UnityEngine;

namespace DungeonTogether.Scripts.Manangers
{
    public class PlayerCanvasManager : MonoSingleton<PlayerCanvasManager>
    {
        [SerializeField] private MMHealthBar healthBar;
        [SerializeField] private MMHealthBar manaBar;
        [SerializeField] private MMHealthBar energyBar;
        
        public void UpdateManaBar(float currentMana, float maxMana, bool bump = false)
        {
            Debug.Log($"Updating mana bar with current mana: {currentMana}, max mana: {maxMana}");
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
    }
}
