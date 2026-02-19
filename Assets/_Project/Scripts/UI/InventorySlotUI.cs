using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Overun.Weapons;

namespace Overun.UI
{
    public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI References")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _rarityBorder;
        [SerializeField] private GameObject _emptyState;
        [SerializeField] private GameObject _filledState;

        private WeaponInstance _currentWeapon;

        public void Setup(WeaponInstance weapon)
        {
            _currentWeapon = weapon;

            if (weapon != null && weapon.Data != null)
            {
                _filledState.SetActive(true);
                _emptyState.SetActive(false);

                _iconImage.sprite = weapon.Data.Icon;
                _rarityBorder.color = WeaponData.GetRarityColor(weapon.Data.Rarity);
            }
            else
            {
                _filledState.SetActive(false);
                _emptyState.SetActive(true);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_currentWeapon != null && WeaponTooltipUI.Instance != null)
            {
                WeaponTooltipUI.Instance.Show(_currentWeapon.Data, _currentWeapon.StackCount);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (WeaponTooltipUI.Instance != null)
            {
                WeaponTooltipUI.Instance.Hide();
            }
        }
    }
}
