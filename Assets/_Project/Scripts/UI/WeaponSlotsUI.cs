using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Overun.Weapons;

namespace Overun.UI
{
    /// <summary>
    /// Displays weapon slots with icons, stacks, and selection indicator.
    /// </summary>
    public class WeaponSlotsUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private WeaponInventory _inventory;
        
        [Header("Slot Template")]
        [SerializeField] private GameObject _slotPrefab;
        [SerializeField] private Transform _slotsContainer;
        
        [Header("Visual Settings")]
        [SerializeField] private Color _selectedColor = Color.white;
        [SerializeField] private Color _unselectedColor = new Color(1f, 1f, 1f, 0.5f);
        [SerializeField] private Vector2 _selectedScale = new Vector2(1.2f, 1.2f);
        
        private List<WeaponSlotUI> _slots = new List<WeaponSlotUI>();
        
        private void Awake()
        {
            if (_inventory == null)
            {
                _inventory = FindObjectOfType<WeaponInventory>();
            }
        }
        
        private void Start()
        {
            CreateSlots();
            
            if (_inventory != null)
            {
                _inventory.OnInventoryChanged += RefreshSlots;
                _inventory.OnWeaponSelected += OnWeaponSelected;
            }
        }
        
        private void OnDestroy()
        {
            if (_inventory != null)
            {
                _inventory.OnInventoryChanged -= RefreshSlots;
                _inventory.OnWeaponSelected -= OnWeaponSelected;
            }
        }
        
        private void CreateSlots()
        {
            // Clear existing
            foreach (Transform child in _slotsContainer)
            {
                Destroy(child.gameObject);
            }
            _slots.Clear();
            
            // Create 6 slots
            for (int i = 0; i < 6; i++)
            {
                GameObject slotObj = Instantiate(_slotPrefab, _slotsContainer);
                WeaponSlotUI slot = slotObj.GetComponent<WeaponSlotUI>();
                
                if (slot == null)
                {
                    slot = slotObj.AddComponent<WeaponSlotUI>();
                }
                
                slot.SetSlotNumber(i + 1);
                _slots.Add(slot);
            }
            
            RefreshSlots();
        }
        
        private void RefreshSlots()
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                WeaponInstance weapon = i < _inventory.Weapons.Count ? _inventory.Weapons[i] : null;
                _slots[i].SetWeapon(weapon);
                _slots[i].SetSelected(i == _inventory.SelectedIndex);
            }
        }
        
        private void OnWeaponSelected(int index)
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                _slots[i].SetSelected(i == index);
            }
        }
    }
    
}
