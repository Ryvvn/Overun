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
    /// Story 6-6: Lock Mechanic tests.
    /// Covers lock toggle, cost, reroll interaction, and state reset.
    /// </summary>
    public class LockMechanicTests
    {
        private GameObject _managersObj;
        private ShopManager _shopManager;
        private CurrencyManager _currencyManager;
        private WeaponData _testWeapon;
        private WeaponData _testWeapon2;

        [SetUp]
        public void Setup()
        {
            _managersObj = new GameObject("Managers");
            _currencyManager = _managersObj.AddComponent<CurrencyManager>();
            _shopManager = _managersObj.AddComponent<ShopManager>();
            
            var playerObj = new GameObject("Player");
            playerObj.tag = "Player";
            playerObj.AddComponent<WeaponInventory>();
            
            _testWeapon = ScriptableObject.CreateInstance<WeaponData>();
            _testWeapon2 = ScriptableObject.CreateInstance<WeaponData>();
            
            // Inject available weapons
            var weaponsField = typeof(ShopManager).GetField("_availableWeapons", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var weapons = new System.Collections.Generic.List<WeaponData> { _testWeapon, _testWeapon2 };
            weaponsField.SetValue(_shopManager, weapons);
        }

        [TearDown]
        public void Teardown()
        {
            if (_managersObj) Object.Destroy(_managersObj);
            if (_testWeapon) Object.Destroy(_testWeapon);
            if (_testWeapon2) Object.Destroy(_testWeapon2);
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player) Object.Destroy(player);
        }

        [UnityTest]
        public IEnumerator Lock_Costs2Gold()
        {
            yield return null;
            
            _currencyManager.AddGold(100);
            _shopManager.GenerateShopItems();
            
            bool result = _shopManager.TryLockItem(0);
            
            Assert.IsTrue(result, "Lock should succeed");
            Assert.AreEqual(98, _currencyManager.CurrentGold, "Should spend 2 gold");
            Assert.IsTrue(_shopManager.CurrentItems[0].IsLocked, "Item should be locked");
        }

        [UnityTest]
        public IEnumerator Unlock_IsFree()
        {
            yield return null;
            
            _currencyManager.AddGold(100);
            _shopManager.GenerateShopItems();
            
            // Lock first
            _shopManager.TryLockItem(0);
            Assert.AreEqual(98, _currencyManager.CurrentGold);
            
            // Unlock
            _shopManager.TryLockItem(0);
            Assert.AreEqual(98, _currencyManager.CurrentGold, "Unlock should be free");
            Assert.IsFalse(_shopManager.CurrentItems[0].IsLocked, "Item should be unlocked");
        }

        [UnityTest]
        public IEnumerator Lock_Fails_IfInsufficientGold()
        {
            yield return null;
            
            _currencyManager.AddGold(1); // Less than 2
            _shopManager.GenerateShopItems();
            
            bool result = _shopManager.TryLockItem(0);
            
            Assert.IsFalse(result, "Lock should fail");
            Assert.AreEqual(1, _currencyManager.CurrentGold, "Gold should not change");
            Assert.IsFalse(_shopManager.CurrentItems[0].IsLocked, "Item should not be locked");
        }

        [UnityTest]
        public IEnumerator LockedItem_SurvivesReroll()
        {
            yield return null;
            
            _currencyManager.AddGold(100);
            _shopManager.GenerateShopItems();
            
            // Lock the first item
            _shopManager.TryLockItem(0);
            ShopItem lockedItem = _shopManager.CurrentItems[0];
            WeaponData lockedWeapon = lockedItem.Weapon;
            
            // Reroll
            _shopManager.TryRerollItems();
            
            // The locked item should still be the same
            Assert.AreSame(lockedWeapon, _shopManager.CurrentItems[0].Weapon, "Locked weapon should persist through reroll");
            Assert.IsTrue(_shopManager.CurrentItems[0].IsLocked, "Item should still be locked after reroll");
        }

        [UnityTest]
        public IEnumerator AllLocked_RerollDoesNothing()
        {
            yield return null;
            
            _currencyManager.AddGold(100);
            _shopManager.GenerateShopItems();
            
            // Lock ALL items
            for (int i = 0; i < _shopManager.CurrentItems.Count; i++)
            {
                _shopManager.TryLockItem(i);
            }
            
            int goldBefore = _currencyManager.CurrentGold;
            int costBefore = _shopManager.CurrentRerollCost;
            
            _shopManager.TryRerollItems();
            
            Assert.AreEqual(goldBefore, _currencyManager.CurrentGold, "No gold should be spent");
            Assert.AreEqual(costBefore, _shopManager.CurrentRerollCost, "Reroll cost should not increase");
        }

        [UnityTest]
        public IEnumerator ResetShopState_ClearsAllLocks()
        {
            yield return null;
            
            _currencyManager.AddGold(100);
            _shopManager.GenerateShopItems();
            
            _shopManager.TryLockItem(0);
            _shopManager.TryLockItem(1);
            
            Assert.IsTrue(_shopManager.CurrentItems[0].IsLocked);
            Assert.IsTrue(_shopManager.CurrentItems[1].IsLocked);
            
            _shopManager.ResetShopState();
            
            foreach (var item in _shopManager.CurrentItems)
            {
                Assert.IsFalse(item.IsLocked, "All locks should be cleared after reset");
            }
        }

        [UnityTest]
        public IEnumerator CannotLock_PurchasedItem()
        {
            yield return null;
            
            _currencyManager.AddGold(500);
            _shopManager.GenerateShopItems();
            
            // Buy item first
            _shopManager.TryBuyItem(0);
            
            // Try to lock it
            bool result = _shopManager.TryLockItem(0);
            
            Assert.IsFalse(result, "Should not be able to lock purchased item");
        }
    }
}
