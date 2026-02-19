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
    /// Story 6-5: Reroll & Inventory Panel tests.
    /// Covers reroll cost scaling, reset on close, and money deduction.
    /// </summary>
    public class RerollMechanicTests
    {
        private GameObject _managersObj;
        private ShopManager _shopManager;
        private CurrencyManager _currencyManager;
        private WeaponInventory _weaponInventory;
        private WeaponData _testWeapon;

        [SetUp]
        public void Setup()
        {
            _managersObj = new GameObject("Managers");
            
            // Currency
            _currencyManager = _managersObj.AddComponent<CurrencyManager>();
            
            // Shop
            _shopManager = _managersObj.AddComponent<ShopManager>();
            
            // Player with inventory (needed for shop init sometimes)
            var playerObj = new GameObject("Player");
            playerObj.tag = "Player";
            _weaponInventory = playerObj.AddComponent<WeaponInventory>();
            
            // Create test weapon
            _testWeapon = ScriptableObject.CreateInstance<WeaponData>();
            
            // Inject available weapons
            var weaponsField = typeof(ShopManager).GetField("_availableWeapons", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var weapons = new System.Collections.Generic.List<WeaponData> { _testWeapon };
            weaponsField.SetValue(_shopManager, weapons);
        }

        [TearDown]
        public void Teardown()
        {
            // Objects destroyed by scene unload usually, but good practice
            if (_managersObj) Object.Destroy(_managersObj);
            if (_testWeapon) Object.Destroy(_testWeapon);
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player) Object.Destroy(player);
        }

        [UnityTest]
        public IEnumerator Reroll_IncreasesCost_By5Gold()
        {
            yield return null;
            
            _currencyManager.AddGold(100);
            _shopManager.GenerateShopItems(); // Resets cost to 5
            
            Assert.AreEqual(5, _shopManager.CurrentRerollCost, "Initial cost should be 5");
            
            // First Reroll (5g)
            _shopManager.TryRerollItems();
            Assert.AreEqual(95, _currencyManager.CurrentGold, "Should spend 5 gold");
            Assert.AreEqual(10, _shopManager.CurrentRerollCost, "Cost should increase to 10");
            
            // Second Reroll (10g)
            _shopManager.TryRerollItems();
            Assert.AreEqual(85, _currencyManager.CurrentGold, "Should spend 10 gold");
            Assert.AreEqual(15, _shopManager.CurrentRerollCost, "Cost should increase to 15");
        }

        [UnityTest]
        public IEnumerator Reroll_Fails_IfInsufficientGold()
        {
            yield return null;
            
            _currencyManager.AddGold(3); // Less than 5
            _shopManager.GenerateShopItems();
            
            int initialCost = _shopManager.CurrentRerollCost;
            
            _shopManager.TryRerollItems();
            
            Assert.AreEqual(3, _currencyManager.CurrentGold, "Gold should not change");
            Assert.AreEqual(initialCost, _shopManager.CurrentRerollCost, "Cost should not increase");
        }

        [UnityTest]
        public IEnumerator ResetShopState_ResetsCost_ToBase()
        {
            yield return null;
            
            _currencyManager.AddGold(100);
            _shopManager.GenerateShopItems();
            
            // Increase cost
            _shopManager.TryRerollItems();
            Assert.AreEqual(10, _shopManager.CurrentRerollCost);
            
            // Reset (simulates new wave)
            _shopManager.ResetShopState();
            
            Assert.AreEqual(5, _shopManager.CurrentRerollCost, "Cost should reset to 5");
        }
        
        [UnityTest]
        public IEnumerator NewShopGeneration_ResetsCost()
        {
            yield return null;
             _currencyManager.AddGold(100);
            _shopManager.GenerateShopItems();
             _shopManager.TryRerollItems(); // Cost -> 10
             
             // Open shop again (GenerateShopItems called)
             _shopManager.GenerateShopItems();
             
             Assert.AreEqual(5, _shopManager.CurrentRerollCost, "Generating new shop should reset reroll cost");
        }
    }
}
