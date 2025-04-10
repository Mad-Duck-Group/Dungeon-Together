using TMPro;
using TriInspector;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonTogether.Scripts.UI
{
    public class ActionIcon : MonoBehaviour
    {
        [Title("References")]
        [SerializeField, Required] private Image background;
        [SerializeField, Required] private Image icon;
        [SerializeField, Required] private TMP_Text timerText;

        public void SetImage(Sprite sprite)
        {
            icon.sprite = sprite;
            background.sprite = sprite;
        }
        
        public void SetAvailable(bool available)
        {
            icon.color = available ? Color.white : background.color;
        }

        public void SetProgress(float current, float max)
        {
            current = Mathf.Clamp(current, 0, max);
            icon.fillAmount = current / max;
        }
        
        public void SetTimer(float time)
        {
            time = Mathf.Max(0, time);
            timerText.text = time == 0 ? "" : time.ToString("F1");
        }
    }
}
