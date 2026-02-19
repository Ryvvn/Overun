using UnityEngine;
using TMPro;
using Overun.Weapons;

namespace Overun.UI
{
    public class WeaponTooltipUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _typeText;
        [SerializeField] private TextMeshProUGUI _statsText; // Damage, FireRate, etc.
        [SerializeField] private TextMeshProUGUI _rarityText;
        [SerializeField] private GameObject _panel;

        public static WeaponTooltipUI Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Hide();
        }

        public void Show(WeaponData data, int stackCount)
        {
            if (data == null) return;

            _nameText.text = data.WeaponName;
            _typeText.text = data.Type.ToString();
            
            // Calculate stats based on stack
            float damage = data.GetDamage(stackCount);
            float fireRate = data.GetFireRate(stackCount);
            
            _statsText.text = $"Dmg: {damage:F1}\nRate: {fireRate:F1}/s\nTier: {stackCount}";
            _rarityText.text = data.Rarity.ToString();
            _rarityText.color = WeaponData.GetRarityColor(data.Rarity);

            _panel.SetActive(true);
            
            // Follow mouse
            UpdatePosition();
        }

        public void Hide()
        {
            _panel.SetActive(false);
        }

        private void Update()
        {
            if (_panel.activeSelf)
            {
                UpdatePosition();
            }
        }

        private void UpdatePosition()
        {
            Vector2 mousePos = UnityEngine.Input.mousePosition;
            transform.position = mousePos + new Vector2(20, 20); // Offset to not cover cursor
        }
    }
}
