using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Overun.Currency;
using Overun.Enemies;
using Overun.Core;

namespace Overun.Tests.PlayMode
{
    public class CurrencySystemTests
    {
        private GameObject _currencyManagerObj;
        private CurrencyManager _currencyManager;

        [SetUp]
        public void Setup()
        {
            _currencyManagerObj = new GameObject("CurrencyManager");
            _currencyManager = _currencyManagerObj.AddComponent<CurrencyManager>();
        }

        [TearDown]
        public void Teardown()
        {
            if (_currencyManagerObj) Object.Destroy(_currencyManagerObj);
        }

        [Test]
        public void AddGold_IncreasesTotal()
        {
            _currencyManager.AddGold(10);
            Assert.AreEqual(10, _currencyManager.CurrentGold);
        }

        [Test]
        public void SpendGold_ReducesTotal_IfAffordable()
        {
            _currencyManager.AddGold(50);
            bool success = _currencyManager.SpendGold(20);
            
            Assert.IsTrue(success);
            Assert.AreEqual(30, _currencyManager.CurrentGold);
        }

        [Test]
        public void SpendGold_Fails_IfInsufficient()
        {
            _currencyManager.AddGold(10);
            bool success = _currencyManager.SpendGold(20);
            
            Assert.IsFalse(success);
            Assert.AreEqual(10, _currencyManager.CurrentGold);
        }

        [UnityTest]
        public IEnumerator GoldDropper_SpawnsPickup_OnDeath()
        {
            // Setup Enemy with GoldDropper
            GameObject enemyObj = new GameObject("Enemy");
            var enemy = enemyObj.AddComponent<Enemy>();
            var dropper = enemyObj.AddComponent<GoldDropper>();
            
            // Need a dummy gold prefab
            GameObject goldPrefab = new GameObject("GoldPickup");
            goldPrefab.AddComponent<BoxCollider>().isTrigger = true;
            goldPrefab.AddComponent<GoldPickup>();
            
            // Assign prefab via reflection
            var prefabField = typeof(GoldDropper).GetField("_goldPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            prefabField.SetValue(dropper, goldPrefab);
            
            // Kill enemy
            enemy.TakeDamage(9999);
            
            yield return null; // Wait for frame
            
            // Check if pickup spawned
            GoldPickup spawnedPickup = Object.FindObjectOfType<GoldPickup>();
            
            // Note: The original prefab is in the scene, so we expect 2 if spawned, or 1 if we only count the new one. 
            // Better: active count. Original is active. Spawned is active.
            // Let's rely on finding one that ISNT the prefab
            var pickups = Object.FindObjectsOfType<GoldPickup>();
            bool foundSpawned = false;
            foreach(var p in pickups)
            {
                if (p.gameObject != goldPrefab) foundSpawned = true;
            }
            
            Assert.IsTrue(foundSpawned, "GoldDropper should spawn a pickup on death");
            
            Object.Destroy(enemyObj);
            Object.Destroy(goldPrefab);
            foreach(var p in pickups) if(p) Object.Destroy(p.gameObject);
        }
    }
}
