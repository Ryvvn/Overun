using UnityEngine;
using System.Collections.Generic;

namespace Overun.Weapons
{
    /// <summary>
    /// Result of attempting to add a weapon to inventory.
    /// </summary>
    public enum AddWeaponResult
    {
        Added,          // New weapon added to empty slot
        Upgraded,       // Duplicate stacked on existing weapon (tier up)
        InventoryFull,  // No slots available, not a duplicate
        AlreadyMaxTier  // Duplicate found but already at max stack
    }
    
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
        /// Try to add a weapon. Returns detailed result for UI feedback.
        /// </summary>
        public AddWeaponResult TryAddWeapon(WeaponData data)
        {
            if (data == null) return AddWeaponResult.InventoryFull;
            
            // First check if we have this exact weapon (duplicate -> upgrade)
            WeaponInstance existing = GetWeaponByData(data);
            if (existing != null)
            {
                if (existing.TryStack())
                {
                    Debug.Log($"[WeaponInventory] UPGRADED {data.WeaponName}! Now Tier {existing.StackCount}");
                    OnWeaponStacked?.Invoke(existing);
                    OnInventoryChanged?.Invoke();
                    return AddWeaponResult.Upgraded;
                }
                else
                {
                    Debug.Log($"[WeaponInventory] {data.WeaponName} already at max tier!");
                    return AddWeaponResult.AlreadyMaxTier;
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
                
                return AddWeaponResult.Added;
            }
            
            Debug.Log("[WeaponInventory] Inventory full!");
            return AddWeaponResult.InventoryFull;
        }
        
        /// <summary>
        /// Get weapon instance by WeaponData (for duplicate detection).
        /// </summary>
        public WeaponInstance GetWeaponByData(WeaponData data)
        {
            foreach (var weapon in _weapons)
            {
                if (weapon.Data == data)
                {
                    return weapon;
                }
            }
            return null;
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
