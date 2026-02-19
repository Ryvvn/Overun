using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Overun.Shop;
using Overun.Currency;
using Overun.Weapons;

namespace Overun.Tests.PlayMode
{
    /// <summary>
    /// Story 6-4: Purchasing Logic & Economics tests.
    /// Covers buy flow, duplicate upgrade, insufficient gold, and Panic Market.
    /// </summary>
    public class PurchasingLogicTests
    {
        private GameObject _managersObj;
        private GameObject _playerObj;
        private ShopManager _shopManager;
        private CurrencyManager _currencyManager;
        private WeaponInventory _weaponInventory;
        private WeaponData _testWeaponA;
        private WeaponData _testWeaponB;

        [SetUp]
        public void Setup()
        {
            _managersObj = new GameObject("Managers");
            
            // Currency
            _currencyManager = _managersObj.AddComponent<CurrencyManager>();
            
            // Shop
            _shopManager = _managersObj.AddComponent<ShopManager>();
            
            // Player with inventory
            _playerObj = new GameObject("Player");
            _playerObj.tag = "Player";
            _weaponInventory = _playerObj.AddComponent<WeaponInventory>();
            
            // Create test weapons
            _testWeaponA = ScriptableObject.CreateInstance<WeaponData>();
            _testWeaponB = ScriptableObject.CreateInstance<WeaponData>();
            
            // Inject available weapons via reflection
            var weaponsField = typeof(ShopManager).GetField("_availableWeapons", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var weapons = new System.Collections.Generic.List<WeaponData> { _testWeaponA, _testWeaponB };
            weaponsField.SetValue(_shopManager, weapons);
        }

        [TearDown]
        public void Teardown()
        {
            if (_managersObj) Object.Destroy(_managersObj);
            if (_playerObj) Object.Destroy(_playerObj);
            if (_testWeaponA) Object.Destroy(_testWeaponA);
            if (_testWeaponB) Object.Destroy(_testWeaponB);
        }

        /// <summary>
        /// Test 1: Buy new item → gold deducted, item in inventory.
        /// </summary>
        [UnityTest]
        public IEnumerator BuyNewItem_DeductsGold_AddsToInventory()
        {
            yield return null; // Let Awake run
            
            // Give player gold
            _currencyManager.AddGold(500);
            
            // Manually add a shop item (bypass RefreshItems RNG)
            var currentItems = _shopManager.CurrentItems;
            currentItems.Clear();
            currentItems.Add(new ShopItem(_testWeaponA, 100));
            
            int goldBefore = _currencyManager.CurrentGold;
            
            PurchaseResult result = _shopManager.TryBuyItem(0);
            
            Assert.AreEqual(PurchaseResult.Success, result, "Purchase should succeed");
            Assert.AreEqual(goldBefore - 100, _currencyManager.CurrentGold, "Gold should be deducted by item cost");
            Assert.IsTrue(currentItems[0].IsPurchased, "Item should be marked as purchased");
            Assert.AreEqual(1, _weaponInventory.WeaponCount, "Weapon should be in inventory");
        }

        /// <summary>
        /// Test 2: Buy duplicate → weapon tier upgrades.
        /// </summary>
        [UnityTest]
        public IEnumerator BuyDuplicate_UpgradesExistingWeapon()
        {
            yield return null;
            
            _currencyManager.AddGold(500);
            
            // First purchase
            var currentItems = _shopManager.CurrentItems;
            currentItems.Clear();
            currentItems.Add(new ShopItem(_testWeaponA, 100));
            _shopManager.TryBuyItem(0);
            
            // Second purchase of same weapon type
            currentItems.Add(new ShopItem(_testWeaponA, 100));
            PurchaseResult result = _shopManager.TryBuyItem(1);
            
            Assert.AreEqual(PurchaseResult.Upgraded, result, "Should detect duplicate and upgrade");
            Assert.AreEqual(1, _weaponInventory.WeaponCount, "Should still have 1 weapon (upgraded, not added)");
            
            WeaponInstance weapon = _weaponInventory.GetWeaponByData(_testWeaponA);
            Assert.AreEqual(2, weapon.StackCount, "Weapon should be tier 2 after upgrade");
        }

        /// <summary>
        /// Test 3: Buy with 0 gold → purchase fails, no gold lost.
        /// </summary>
        [UnityTest]
        public IEnumerator BuyWithNoGold_Fails_NoGoldLost()
        {
            yield return null;
            
            // No gold added — player has 0
            var currentItems = _shopManager.CurrentItems;
            currentItems.Clear();
            currentItems.Add(new ShopItem(_testWeaponA, 100));
            
            PurchaseResult result = _shopManager.TryBuyItem(0);
            
            Assert.AreEqual(PurchaseResult.InsufficientGold, result, "Should fail with insufficient gold");
            Assert.AreEqual(0, _currencyManager.CurrentGold, "Gold should remain 0");
            Assert.IsFalse(currentItems[0].IsPurchased, "Item should NOT be marked as purchased");
            Assert.AreEqual(0, _weaponInventory.WeaponCount, "Nothing should be in inventory");
        }

        /// <summary>
        /// Test 4: Panic Market → buying Item A changes Item B's price.
        /// </summary>
        [UnityTest]
        public IEnumerator PanicMarket_ChangesPricesAfterPurchase()
        {
            yield return null;
            
            _currencyManager.AddGold(500);
            
            var currentItems = _shopManager.CurrentItems;
            currentItems.Clear();
            currentItems.Add(new ShopItem(_testWeaponA, 100));
            currentItems.Add(new ShopItem(_testWeaponB, 200));
            
            int itemBBaseCost = currentItems[1].BaseCost;
            
            // Buy item A — triggers Panic Market
            _shopManager.TryBuyItem(0);
            
            // Item B's CurrentCost should have changed (Panic Market +/- 50%)
            // Because of randomness, we verify the range rather than exact value
            int newCost = currentItems[1].CurrentCost;
            int minExpected = Mathf.RoundToInt(itemBBaseCost * 0.5f);
            int maxExpected = Mathf.RoundToInt(itemBBaseCost * 1.5f);
            
            // CurrentCost should be within Panic Market range (or at minimum 1)
            Assert.GreaterOrEqual(newCost, Mathf.Max(1, minExpected), 
                $"Panic Market price should be >= {Mathf.Max(1, minExpected)} (got {newCost})");
            Assert.LessOrEqual(newCost, maxExpected, 
                $"Panic Market price should be <= {maxExpected} (got {newCost})");
        }

        /// <summary>
        /// Test 5: Glitch slot purchase reveals item and adds to inventory.
        /// </summary>
        [UnityTest]
        public IEnumerator BuyGlitchSlot_RevealsAndAddsToInventory()
        {
            yield return null;
            
            _currencyManager.AddGold(500);
            
            var currentItems = _shopManager.CurrentItems;
            currentItems.Clear();
            
            ShopItem glitchItem = new ShopItem(_testWeaponA, 50);
            glitchItem.IsGlitch = true;
            currentItems.Add(glitchItem);
            
            PurchaseResult result = _shopManager.TryBuyItem(0);
            
            Assert.AreEqual(PurchaseResult.Success, result, "Glitch purchase should succeed");
            Assert.IsTrue(currentItems[0].IsPurchased, "Glitch item should be marked purchased");
            Assert.AreEqual(1, _weaponInventory.WeaponCount, "Glitch weapon should be in inventory");
        }

        /// <summary>
        /// Test 6: Already purchased item returns AlreadyPurchased.
        /// </summary>
        [UnityTest]
        public IEnumerator BuyAlreadyPurchased_ReturnsAlreadyPurchased()
        {
            yield return null;
            
            _currencyManager.AddGold(500);
            
            var currentItems = _shopManager.CurrentItems;
            currentItems.Clear();
            currentItems.Add(new ShopItem(_testWeaponA, 100));
            
            // Buy once
            _shopManager.TryBuyItem(0);
            
            // Try to buy again
            PurchaseResult result = _shopManager.TryBuyItem(0);
            
            Assert.AreEqual(PurchaseResult.AlreadyPurchased, result, "Should return AlreadyPurchased for already bought item");
        }
    }
}
