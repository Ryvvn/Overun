using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using Overun.Currency;
using Overun.Weapons;

namespace Overun.Shop
{
    /// <summary>
    /// Result of a purchase attempt for UI feedback.
    /// </summary>
    public enum PurchaseResult
    {
        Success,           // Item purchased and added
        Upgraded,          // Duplicate purchased, weapon upgraded
        InsufficientGold,  // Can't afford
        InventoryFull,     // No slots, not a duplicate
        AlreadyMaxTier,    // Already have max tier
        AlreadyPurchased   // Item already bought
    }
    
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
        
        // Events
        public event Action OnShopOpened;
        public event Action OnShopClosed;
        public event Action OnItemsRefreshed;
        public event Action<PurchaseResult, ShopItem> OnPurchaseAttempt; // UI feedback
        public event Action OnPanicMarket; // Panic Market triggered
        
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
            
            // Wait for the clean up the wave to finish, wait for few seconds
            StartCoroutine(WaitToOpenShop());
        }
        
        private IEnumerator WaitToOpenShop()
        {
            yield return new WaitForSeconds(2f);
            _isShopOpen = true;
            //Time.timeScale = 0f; // Pause game
            RefreshItems(true); // Free refresh on open
            OnShopOpened?.Invoke();

            // Show the cursor 
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            Debug.Log("[ShopManager] Shop Opened");
        }

        public void CloseShop()
        {
            if (!_isShopOpen) return;
            
            _isShopOpen = false;
            //Time.timeScale = 1f; // Resume game
            
            OnShopClosed?.Invoke();

            // Hide the cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
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
            
            // 3 Normal Slots
            for (int i = 0; i < 3; i++)
            {
                WeaponData randomWeapon = _availableWeapons[UnityEngine.Random.Range(0, _availableWeapons.Count)];
                int baseCost = CalculateCost(randomWeapon);
                ShopItem item = new ShopItem(randomWeapon, baseCost);
                _currentItems.Add(item);
            }
            
            // 1 Glitch Slot (Index 3)
            WeaponData glitchWeapon = _availableWeapons[UnityEngine.Random.Range(0, _availableWeapons.Count)];
            ShopItem glitchItem = new ShopItem(glitchWeapon, 50); 
            glitchItem.IsGlitch = true;
            _currentItems.Add(glitchItem);
            
            OnItemsRefreshed?.Invoke();
        }
        
        private int CalculateCost(WeaponData weapon)
        {
             // Base cost + Rarity scaling
             int rarityMult = (int)weapon.Rarity + 1; // 1 to 5
             return 50 * rarityMult;
        }
        
        public PurchaseResult TryBuyItem(int index)
        {
            if (index < 0 || index >= _currentItems.Count)
                return PurchaseResult.InventoryFull;
            
            ShopItem item = _currentItems[index];
            
            if (item.IsPurchased)
            {
                OnPurchaseAttempt?.Invoke(PurchaseResult.AlreadyPurchased, item);
                return PurchaseResult.AlreadyPurchased;
            }
            
            int cost = item.CurrentCost; // Use CurrentCost (affected by Panic Market)
            
            // Check if can afford
            if (!CurrencyManager.Instance.CanAfford(cost))
            {
                Debug.Log($"[Shop] Cannot afford {item.Weapon.WeaponName}! Need {cost}g");
                OnPurchaseAttempt?.Invoke(PurchaseResult.InsufficientGold, item);
                return PurchaseResult.InsufficientGold;
            }
            
            // Try to add to inventory
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogError("[Shop] Player not found!");
                return PurchaseResult.InventoryFull;
            }
            
            var inventory = player.GetComponent<WeaponInventory>();
            if (inventory == null)
            {
                Debug.LogError("[Shop] WeaponInventory not found on player!");
                return PurchaseResult.InventoryFull;
            }
            
            AddWeaponResult addResult = inventory.TryAddWeapon(item.Weapon);
            
            switch (addResult)
            {
                case AddWeaponResult.Added:
                    CurrencyManager.Instance.SpendGold(cost);
                    item.IsPurchased = true;
                    HandleGlitchReveal(item);
                    TriggerPanicMarket();
                    OnPurchaseAttempt?.Invoke(PurchaseResult.Success, item);
                    OnItemsRefreshed?.Invoke();
                    Debug.Log($"[Shop] Purchased {item.Weapon.WeaponName} for {cost}g");
                    return PurchaseResult.Success;
                    
                case AddWeaponResult.Upgraded:
                    CurrencyManager.Instance.SpendGold(cost);
                    item.IsPurchased = true;
                    HandleGlitchReveal(item);
                    TriggerPanicMarket();
                    OnPurchaseAttempt?.Invoke(PurchaseResult.Upgraded, item);
                    OnItemsRefreshed?.Invoke();
                    Debug.Log($"[Shop] UPGRADED {item.Weapon.WeaponName} for {cost}g!");
                    return PurchaseResult.Upgraded;
                    
                case AddWeaponResult.InventoryFull:
                    Debug.Log($"[Shop] Inventory full, cannot buy {item.Weapon.WeaponName}");
                    OnPurchaseAttempt?.Invoke(PurchaseResult.InventoryFull, item);
                    return PurchaseResult.InventoryFull;
                    
                case AddWeaponResult.AlreadyMaxTier:
                    Debug.Log($"[Shop] {item.Weapon.WeaponName} already at max tier!");
                    OnPurchaseAttempt?.Invoke(PurchaseResult.AlreadyMaxTier, item);
                    return PurchaseResult.AlreadyMaxTier;
                    
                default:
                    return PurchaseResult.InventoryFull;
            }
        }
        
        private void HandleGlitchReveal(ShopItem item)
        {
            if (item.IsGlitch)
            {
                Debug.Log($"[Shop] GLITCH REVEALED! You got: {item.Weapon.WeaponName}");
                // Event could be added here for reveal animation
            }
        }
        
        /// <summary>
        /// Trigger Panic Market: Shuffle prices of all unpurchased items.
        /// </summary>
        public void TriggerPanicMarket()
        {
            bool anyChanged = false;
            
            foreach (var item in _currentItems)
            {
                if (!item.IsPurchased && !item.IsGlitch) // Don't change glitch slot price
                {
                    // +/- 50% variance
                    float variance = UnityEngine.Random.Range(0.5f, 1.5f);
                    int newCost = Mathf.RoundToInt(item.BaseCost * variance);
                    item.CurrentCost = Mathf.Max(1, newCost); // Minimum 1 gold
                    anyChanged = true;
                }
            }
            
            if (anyChanged)
            {
                Debug.Log("[Shop] PANIC MARKET! Prices shuffled!");
                OnPanicMarket?.Invoke();
                OnItemsRefreshed?.Invoke();
            }
        }
    }
    
}
