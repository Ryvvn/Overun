using UnityEngine;
using System.Collections.Generic;

namespace Overun.Weapons
{
    /// <summary>
    /// Player's weapon inventory. Manages weapon slots, stacking, and switching.
    /// </summary>
    public class WeaponInventory : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int _maxSlots = 6;
        
        [Header("Current State")]
        [SerializeField] private int _selectedIndex = 0;
        
        private List<WeaponInstance> _weapons = new List<WeaponInstance>();
        
        // Events
        public event System.Action<WeaponInstance> OnWeaponAdded;
        public event System.Action<WeaponInstance> OnWeaponStacked;
        public event System.Action<int> OnWeaponSelected;
        public event System.Action OnInventoryChanged;
        
        // Properties
        public List<WeaponInstance> Weapons => _weapons;
        public int SelectedIndex => _selectedIndex;
        public int WeaponCount => _weapons.Count;
        public int MaxSlots => _maxSlots;
        public bool HasWeapons => _weapons.Count > 0;
        
        public WeaponInstance SelectedWeapon => 
            _weapons.Count > 0 && _selectedIndex >= 0 && _selectedIndex < _weapons.Count 
            ? _weapons[_selectedIndex] 
            : null;
        
        /// <summary>
        /// Try to add a weapon. Returns true if added or stacked.
        /// </summary>
        public bool TryAddWeapon(WeaponData data)
        {
            if (data == null) return false;
            
            // First check if we can stack with existing weapon
            foreach (var weapon in _weapons)
            {
                if (weapon.Data == data || weapon.Data.Type == data.Type)
                {
                    if (weapon.TryStack())
                    {
                        Debug.Log($"[WeaponInventory] Stacked {data.WeaponName}! Now at {weapon.StackCount}");
                        OnWeaponStacked?.Invoke(weapon);
                        OnInventoryChanged?.Invoke();
                        return true;
                    }
                }
            }
            
            // Otherwise add as new weapon if we have slots
            if (_weapons.Count < _maxSlots)
            {
                WeaponInstance newWeapon = new WeaponInstance(data);
                _weapons.Add(newWeapon);
                
                Debug.Log($"[WeaponInventory] Added {data.WeaponName} to slot {_weapons.Count}");
                OnWeaponAdded?.Invoke(newWeapon);
                OnInventoryChanged?.Invoke();
                
                // Auto-select if first weapon
                if (_weapons.Count == 1)
                {
                    SelectWeapon(0);
                }
                
                return true;
            }
            
            Debug.Log("[WeaponInventory] Inventory full!");
            return false;
        }
        
        /// <summary>
        /// Select a weapon by index.
        /// </summary>
        public void SelectWeapon(int index)
        {
            if (index < 0 || index >= _weapons.Count) return;
            
            _selectedIndex = index;
            OnWeaponSelected?.Invoke(_selectedIndex);
            
            Debug.Log($"[WeaponInventory] Selected weapon {_selectedIndex + 1}: {SelectedWeapon?.Data.WeaponName}");
        }
        
        /// <summary>
        /// Select next weapon.
        /// </summary>
        public void SelectNextWeapon()
        {
            if (_weapons.Count == 0) return;
            
            int nextIndex = (_selectedIndex + 1) % _weapons.Count;
            SelectWeapon(nextIndex);
        }
        
        /// <summary>
        /// Select previous weapon.
        /// </summary>
        public void SelectPreviousWeapon()
        {
            if (_weapons.Count == 0) return;
            
            int prevIndex = _selectedIndex - 1;
            if (prevIndex < 0) prevIndex = _weapons.Count - 1;
            SelectWeapon(prevIndex);
        }
        
        /// <summary>
        /// Get all weapons except the selected one (for auto-fire).
        /// </summary>
        public List<WeaponInstance> GetSecondaryWeapons()
        {
            var secondary = new List<WeaponInstance>();
            for (int i = 0; i < _weapons.Count; i++)
            {
                if (i != _selectedIndex)
                {
                    secondary.Add(_weapons[i]);
                }
            }
            return secondary;
        }
        
        /// <summary>
        /// Check if player has a specific weapon type.
        /// </summary>
        public bool HasWeaponType(WeaponType type)
        {
            foreach (var weapon in _weapons)
            {
                if (weapon.Data.Type == type) return true;
            }
            return false;
        }
        
        /// <summary>
        /// Clear all weapons (for new run).
        /// </summary>
        public void Clear()
        {
            _weapons.Clear();
            _selectedIndex = 0;
            OnInventoryChanged?.Invoke();
        }
    }
}
