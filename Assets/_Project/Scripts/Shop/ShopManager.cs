using UnityEngine;
using System;
using System.Collections.Generic;
using Overun.Currency;
using Overun.Weapons;

namespace Overun.Shop
{
    /// <summary>
    /// Manages the shop phase between waves.
    /// Handles item generation, purchasing, and state.
    /// </summary>
    public class ShopManager : MonoBehaviour
    {
        public static ShopManager Instance { get; private set; }
        
        [Header("Configuration")]
        [SerializeField] private List<WeaponData> _availableWeapons;
        [SerializeField] private int _itemsPerShop = 3;
        [SerializeField] private int _rerollCost = 5;
        
        [Header("State")]
        [SerializeField] private bool _isShopOpen = false;
        [SerializeField] private List<ShopItem> _currentItems = new List<ShopItem>();
        
        public event Action OnShopOpened;
        public event Action OnShopClosed;
        public event Action OnItemsRefreshed;
        
        public bool IsShopOpen => _isShopOpen;
        public List<ShopItem> CurrentItems => _currentItems;
        public int RerollCost => _rerollCost;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        public void OpenShop()
        {
            if (_isShopOpen) return;
            
            _isShopOpen = true;
            Time.timeScale = 0f; // Pause game
            
            RefreshItems(true); // Free refresh on open
            
            OnShopOpened?.Invoke();
            Debug.Log("[ShopManager] Shop Opened");
        }
        
        public void CloseShop()
        {
            if (!_isShopOpen) return;
            
            _isShopOpen = false;
            Time.timeScale = 1f; // Resume game
            
            OnShopClosed?.Invoke();
            Debug.Log("[ShopManager] Shop Closed");
        }
        
        public void RerollItems()
        {
            if (CurrencyManager.Instance.SpendGold(_rerollCost))
            {
                RefreshItems(false);
            }
        }
        
        private void RefreshItems(bool free)
        {
            _currentItems.Clear();
            
            if (_availableWeapons == null || _availableWeapons.Count == 0) return;
            
            for (int i = 0; i < _itemsPerShop; i++)
            {
                WeaponData randomWeapon = _availableWeapons[UnityEngine.Random.Range(0, _availableWeapons.Count)];
                
                // Base cost logic placeholder - ideally configurable per weapon
                int baseCost = 50 + (int)randomWeapon.Rarity * 50; 
                
                _currentItems.Add(new ShopItem(randomWeapon, baseCost));
            }
            
            OnItemsRefreshed?.Invoke();
        }
        
        public bool TryBuyItem(int index)
        {
            if (index < 0 || index >= _currentItems.Count) return false;
            
            ShopItem item = _currentItems[index];
            if (item.IsPurchased) return false;
            
            // Get player inventory to check owned count (for price scaling)
            // For MVP, just use BaseCost
            int cost = item.BaseCost;
            
            if (CurrencyManager.Instance.SpendGold(cost))
            {
                // Add to inventory
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    var inventory = player.GetComponent<WeaponInventory>();
                    if (inventory != null)
                    {
                        if (inventory.TryAddWeapon(item.Weapon))
                        {
                            item.IsPurchased = true;
                            // Optionally remove from list or mark sold
                            return true;
                        }
                        else
                        {
                            // Refund if inventory full
                            CurrencyManager.Instance.AddGold(cost); 
                            Debug.Log("[Shop] Inventory full, refunded.");
                        }
                    }
                }
            }
            
            return false;
        }
    }
}
