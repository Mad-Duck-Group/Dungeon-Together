using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using DungeonTogether.Scripts.UI;
using MoreMountains.Tools;
using TMPro;
using TriInspector;
using Unity.Netcode;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonTogether.Scripts.Manangers
{
    [Serializable]
    public struct LoadSceneButton
    {
        public Button button;
        public SceneType sceneType;
    }
    public class PlayerCanvasManager : NetworkSingleton<PlayerCanvasManager>
    {
        [Title("References")]
        [SerializeField, Required] private CanvasGroup playerCanvas;
        [SerializeField] private CanvasGroup loseCanvas;
        [SerializeField] private CanvasGroup winCanvas;
        [SerializeField] private TMP_Text completionTimeText;
        [SerializeField] private CanvasGroup respawnCanvas;
        [SerializeField] private List<LoadSceneButton> loadSceneButtons = new();
        [SerializeField] private List<Button> disconnectButtons = new();
        [SerializeField] private TMP_Text respawningTimerText;
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

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            foreach (var button in loadSceneButtons)
            {
                button.button.onClick.AddListener(() => LoadSceneManager.Instance.LoadScene(button.sceneType));
                if (!IsHost)
                {
                    button.button.interactable = false;
                }
            }
            foreach (var button in disconnectButtons)
            {
                button.onClick.AddListener(() =>
                {
                    Debug.Log("Disconnecting from server...");
                    NetworkManager.Singleton.Shutdown();
                });
            }
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

        #region Respawning
        public void SetActiveRespawnCanvas(bool active)
        {
            respawnCanvas.gameObject.SetActive(active);
        }
        
        public void SetRespawningTimer(float time)
        {
            respawningTimerText.text = $"Respawning in: {GetTimeString(time)}";
        }
        #endregion


        #region Win
        public void SetActiveWinCanvas(bool active)
        {
            winCanvas.gameObject.SetActive(active);
        }
        
        public void SetCompletionTimeText(float time)
        {
            completionTimeText.text =
                $"Completion time: {GetTimeString(time)}";
        }

        #endregion

        #region Lose
        public void SetActiveLoseCanvas(bool active)
        {
            loseCanvas.gameObject.SetActive(active);
        }
        #endregion

        #region GUI
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
        #endregion

        #region Utils

        private string GetTimeString(float time)
        {
            var minutes = Mathf.FloorToInt(time / 60);
            var seconds = Mathf.FloorToInt(time % 60);
            var milliseconds = Mathf.FloorToInt((time - Mathf.Floor(time)) * 1000);
            return $"{minutes:00}:{seconds:00}.{milliseconds:000}";
        }
        #endregion
    }
}
