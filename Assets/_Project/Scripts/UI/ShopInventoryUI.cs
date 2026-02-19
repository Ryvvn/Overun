using System.Collections.Generic;
using UnityEngine;
using Overun.Weapons;

namespace Overun.UI
{
    public class ShopInventoryUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform _slotsContainer;
        [SerializeField] private GameObject _slotPrefab;
        [SerializeField] private int _totalSlots = 6;

        private List<InventorySlotUI> _slots = new List<InventorySlotUI>();
        private WeaponInventory _playerInventory;

        private void Awake()
        {
            // Create slots
            for (int i = 0; i < _totalSlots; i++)
            {
                GameObject slot = Instantiate(_slotPrefab, _slotsContainer);
                _slots.Add(slot.GetComponent<InventorySlotUI>());
            }
        }

        private void OnEnable()
        {
            if (_playerInventory == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    _playerInventory = player.GetComponent<WeaponInventory>();
                }
            }

            if (_playerInventory != null)
            {
                _playerInventory.OnInventoryChanged += RefreshInventory;
                RefreshInventory();
            }
        }

        private void OnDisable()
        {
            if (_playerInventory != null)
            {
                _playerInventory.OnInventoryChanged -= RefreshInventory;
            }
        }

        private void RefreshInventory()
        {
            if (_playerInventory == null) return;

            var weapons = _playerInventory.Weapons; // Assuming Weapons is a public property or field

            for (int i = 0; i < _totalSlots; i++)
            {
                if (i < weapons.Count)
                {
                    _slots[i].Setup(weapons[i]);
                }
                else
                {
                    _slots[i].Setup(null);
                }
            }
        }
    }
}
