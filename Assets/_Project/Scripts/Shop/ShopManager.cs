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

        
        [Header("State")]
        [SerializeField] private bool _isShopOpen = false;
        [SerializeField] private List<ShopItem> _currentItems = new List<ShopItem>();
        
        private const int GLITCH_COST = 50;
        private const float PANIC_MARKET_MIN = 0.5f;
        private const float PANIC_MARKET_MAX = 1.5f;
        private const int BASE_COST_MULTIPLIER = 50;
        
        private WeaponInventory _playerInventory;
        
        // Events
        public event Action OnShopOpened;
        public event Action OnShopClosed;
        public int CurrentRerollCost { get; private set; } = 5;
        private const int BASE_REROLL_COST = 5;
        private const int REROLL_COST_INCREMENT = 5;
        private const int LOCK_COST = 2;

        public event Action OnShopRefreshed;
        public event Action<int> OnRerollCostChanged;
        public event Action<int, bool> OnItemLocked; // index, isLocked
        public event Action<PurchaseResult, ShopItem> OnPurchaseAttempt; // UI feedback
        public event Action OnPanicMarket; // Panic Market triggered
        
        public bool IsShopOpen => _isShopOpen;
        public List<ShopItem> CurrentItems => _currentItems;
        
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
            
            // Cache player inventory if not already found
            if (_playerInventory == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    _playerInventory = player.GetComponent<WeaponInventory>();
                }
            }
            
            // Wait for the clean up the wave to finish, wait for few seconds
            StartCoroutine(WaitToOpenShop());
        }
        
        private IEnumerator WaitToOpenShop()
        {
            yield return new WaitForSeconds(2f);
            _isShopOpen = true;
            //Time.timeScale = 0f; // Pause game
            GenerateShopItems(); // Free refresh on open
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
        
        public void GenerateShopItems()
        {
            // Reset reroll cost on new wave/shop generation
            ResetShopState();
            
            _currentItems.Clear();

            // Normal Slots based on configuration
            for (int i = 0; i < _itemsPerShop; i++)
            {
                if (_availableWeapons.Count > 0)
                {
                    WeaponData randomWeapon = _availableWeapons[UnityEngine.Random.Range(0, _availableWeapons.Count)];
                    int baseCost = CalculateCost(randomWeapon);
                    ShopItem item = new ShopItem(randomWeapon, baseCost);
                    _currentItems.Add(item);
                }
            }
            
            // 1 Glitch Slot (Index after normal slots)
            if (_availableWeapons.Count > 0)
            {
                WeaponData glitchWeapon = _availableWeapons[UnityEngine.Random.Range(0, _availableWeapons.Count)];
                ShopItem glitchItem = new ShopItem(glitchWeapon, GLITCH_COST); 
                glitchItem.IsGlitch = true;
                _currentItems.Add(glitchItem);
            }
            
            OnShopRefreshed?.Invoke();
        }

        public void ResetShopState()
        {
            CurrentRerollCost = BASE_REROLL_COST;
            OnRerollCostChanged?.Invoke(CurrentRerollCost);
            
            // Clear all locks
            foreach (var item in _currentItems)
            {
                item.IsLocked = false;
            }
        }
        
        /// <summary>
        /// Toggle lock on a shop item. Locking costs LOCK_COST gold, unlocking is free.
        /// </summary>
        public bool TryLockItem(int index)
        {
            if (index < 0 || index >= _currentItems.Count) return false;
            
            ShopItem item = _currentItems[index];
            
            // Can't lock purchased items
            if (item.IsPurchased) return false;
            
            if (item.IsLocked)
            {
                // Unlock is free
                item.IsLocked = false;
                OnItemLocked?.Invoke(index, false);
                OnShopRefreshed?.Invoke();
                Debug.Log($"[Shop] Unlocked {item.Weapon.WeaponName}");
                return true;
            }
            else
            {
                // Lock costs gold
                if (!CurrencyManager.Instance.CanAfford(LOCK_COST))
                {
                    Debug.Log($"[Shop] Cannot afford to lock! Need {LOCK_COST}g");
                    return false;
                }
                
                CurrencyManager.Instance.SpendGold(LOCK_COST);
                item.IsLocked = true;
                OnItemLocked?.Invoke(index, true);
                OnShopRefreshed?.Invoke();
                Debug.Log($"[Shop] Locked {item.Weapon.WeaponName} for {LOCK_COST}g");
                return true;
            }
        }

        public void TryRerollItems()
        {
            // Check if ALL remaining items are locked/purchased (nothing to reroll)
            bool hasRerollableItems = false;
            foreach (var item in _currentItems)
            {
                if (!item.IsLocked && !item.IsPurchased)
                {
                    hasRerollableItems = true;
                    break;
                }
            }
            
            if (!hasRerollableItems)
            {
                Debug.Log("[Shop] All items are locked/purchased, nothing to reroll.");
                return;
            }
            
            if (CurrencyManager.Instance.CanAfford(CurrentRerollCost))
            {
                CurrencyManager.Instance.SpendGold(CurrentRerollCost);
                
                // Increase cost for next time
                CurrentRerollCost += REROLL_COST_INCREMENT;
                OnRerollCostChanged?.Invoke(CurrentRerollCost);
                
                // Only replace unlocked, unpurchased items
                for (int i = 0; i < _currentItems.Count; i++)
                {
                    ShopItem existing = _currentItems[i];
                    
                    // Skip locked and purchased items
                    if (existing.IsLocked || existing.IsPurchased) continue;
                    
                    if (_availableWeapons.Count > 0)
                    {
                        if (existing.IsGlitch)
                        {
                            // Regenerate glitch slot
                            WeaponData glitchWeapon = _availableWeapons[UnityEngine.Random.Range(0, _availableWeapons.Count)];
                            ShopItem glitchItem = new ShopItem(glitchWeapon, GLITCH_COST);
                            glitchItem.IsGlitch = true;
                            _currentItems[i] = glitchItem;
                        }
                        else
                        {
                            // Regenerate normal slot
                            WeaponData randomWeapon = _availableWeapons[UnityEngine.Random.Range(0, _availableWeapons.Count)];
                            int baseCost = CalculateCost(randomWeapon);
                            ShopItem newItem = new ShopItem(randomWeapon, baseCost);
                            _currentItems[i] = newItem;
                        }
                    }
                }
                
                OnShopRefreshed?.Invoke();
            }
        }
        
        private int CalculateCost(WeaponData weapon)
        {
             // Base cost + Rarity scaling
             int rarityMult = (int)weapon.Rarity + 1; // 1 to 5
             return BASE_COST_MULTIPLIER * rarityMult;
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
            if (_playerInventory == null)
            {
                // Try one last time to find it (in case player was spawned late)
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) _playerInventory = player.GetComponent<WeaponInventory>();
                
                if (_playerInventory == null)
                {
                    Debug.LogError("[Shop] WeaponInventory not found!");
                    return PurchaseResult.InventoryFull;
                }
            }
            
            AddWeaponResult addResult = _playerInventory.TryAddWeapon(item.Weapon);
            
            switch (addResult)
            {
                case AddWeaponResult.Added:
                    CurrencyManager.Instance.SpendGold(cost);
                    item.IsPurchased = true;
                    HandleGlitchReveal(item);
                    TriggerPanicMarket();
                    OnPurchaseAttempt?.Invoke(PurchaseResult.Success, item);
                    OnShopRefreshed?.Invoke();
                    Debug.Log($"[Shop] Purchased {item.Weapon.WeaponName} for {cost}g");
                    return PurchaseResult.Success;
                    
                case AddWeaponResult.Upgraded:
                    CurrencyManager.Instance.SpendGold(cost);
                    item.IsPurchased = true;
                    HandleGlitchReveal(item);
                    TriggerPanicMarket();
                    OnPurchaseAttempt?.Invoke(PurchaseResult.Upgraded, item);
                    OnShopRefreshed?.Invoke();
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
                    float variance = UnityEngine.Random.Range(PANIC_MARKET_MIN, PANIC_MARKET_MAX);
                    int newCost = Mathf.RoundToInt(item.BaseCost * variance);
                    item.CurrentCost = Mathf.Max(1, newCost); // Minimum 1 gold
                    anyChanged = true;
                }
            }
            
            if (anyChanged)
            {
                Debug.Log("[Shop] PANIC MARKET! Prices shuffled!");
                OnPanicMarket?.Invoke();
                OnShopRefreshed?.Invoke();
            }
        }
    }
    
}
