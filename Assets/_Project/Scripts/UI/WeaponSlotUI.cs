using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Overun.Weapons;

namespace Overun.UI
{
    /// <summary>
    /// Individual weapon slot display.
    /// Attach to each slot prefab instance.
    /// </summary>
    public class WeaponSlotUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _rarityBorder;
        [SerializeField] private TextMeshProUGUI _stackText;
        [SerializeField] private TextMeshProUGUI _slotNumberText;
        
        [Header("Visual")]
        [SerializeField] private Color _emptyColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        [SerializeField] private Color _filledColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        [SerializeField] private Color _selectedColor = new Color(1f, 0.8f, 0.2f, 1f);
        
        private RectTransform _rectTransform;   
        private Vector3 _originalScale;
        private bool _isSelected = false;
        
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (_rectTransform != null)
            {
                _originalScale = _rectTransform.localScale;
            }
            
            // Auto-find components if not assigned
            if (_iconImage == null) 
                _iconImage = transform.Find("Icon")?.GetComponent<Image>();
            if (_backgroundImage == null) 
                _backgroundImage = GetComponent<Image>();
            if (_stackText == null)
            {
                var stackObj = transform.Find("StackCount");
                if (stackObj != null) _stackText = stackObj.GetComponent<TextMeshProUGUI>();
            }
            if (_slotNumberText == null)
            {
                var numObj = transform.Find("SlotNumber");
                if (numObj != null) _slotNumberText = numObj.GetComponent<TextMeshProUGUI>();
            }
        }
        
        /// <summary>
        /// Set the slot number display (1-6).
        /// </summary>
        public void SetSlotNumber(int number)
        {
            if (_slotNumberText != null)
            {
                _slotNumberText.text = number.ToString();
            }
        }
        
        /// <summary>
        /// Update the slot with weapon data.
        /// </summary>
        public void SetWeapon(WeaponInstance weapon)
        {
            if (weapon == null || weapon.Data == null)
            {
                // Empty slot
                if (_iconImage != null)
                {
                    _iconImage.enabled = false;
                }
                if (_backgroundImage != null)
                {
                    _backgroundImage.color = _emptyColor;
                }
                if (_stackText != null)
                {
                    _stackText.text = "";
                }
                if (_rarityBorder != null)
                {
                    _rarityBorder.enabled = false;
                }
            }
            else
            {
                // Filled slot
                if (_iconImage != null)
                {
                    _iconImage.enabled = weapon.Data.Icon != null;
                    _iconImage.sprite = weapon.Data.Icon;
                }
                if (_backgroundImage != null && !_isSelected)
                {
                    _backgroundImage.color = _filledColor;
                }
                if (_stackText != null)
                {
                    _stackText.text = weapon.StackCount > 1 ? $"x{weapon.StackCount}" : "";
                }
                if (_rarityBorder != null)
                {
                    _rarityBorder.enabled = true;
                    _rarityBorder.color = WeaponData.GetRarityColor(weapon.Data.Rarity);
                }
            }
        }
        
        /// <summary>
        /// Set the selected state of this slot.
        /// </summary>
        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            
            if (_rectTransform != null)
            {
                _rectTransform.localScale = selected ? _originalScale * 1.15f : _originalScale;
            }
            
            if (_backgroundImage != null)
            {
                _backgroundImage.color = selected ? _selectedColor : _filledColor;
            }
        }
    }
}
