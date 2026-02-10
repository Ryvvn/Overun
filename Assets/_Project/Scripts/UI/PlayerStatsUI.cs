using UnityEngine;
using TMPro;
using Overun.Core;

namespace Overun.UI
{
    public class PlayerStatsUI : MonoBehaviour
    {
        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI _damageText;
        [SerializeField] private TextMeshProUGUI _speedText;
        [SerializeField] private TextMeshProUGUI _healthText;
        [SerializeField] private TextMeshProUGUI _critText;
        
        private void Start()
        {
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.OnStatsChanged += UpdateDisplay;
                UpdateDisplay();
            }
        }
        
        private void OnDestroy()
        {
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.OnStatsChanged -= UpdateDisplay;
            }
        }
        
        private void UpdateDisplay()
        {
            if (PlayerStats.Instance == null) return;
            
            if (_damageText) _damageText.text = $"DMG: x{PlayerStats.Instance.DamageMultiplier:F2}";
            if (_speedText) _speedText.text = $"SPD: x{PlayerStats.Instance.MoveSpeedMultiplier:F2}";
            if (_healthText) _healthText.text = $"HP: x{PlayerStats.Instance.MaxHealthMultiplier:F2}";
            if (_critText) _critText.text = $"CRIT: {PlayerStats.Instance.CritChance * 100:F0}%";
        }
    }
}
