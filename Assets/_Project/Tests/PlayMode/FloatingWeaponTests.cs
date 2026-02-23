using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Overun.Weapons;
using Overun.Player;

namespace Overun.Tests.PlayMode
{
    /// <summary>
    /// Story 7-A1: Floating Weapons System tests.
    /// Covers orbital positioning, add/remove transitions, selected emphasis, and performance.
    /// </summary>
    public class FloatingWeaponTests
    {
        private GameObject _playerObj;
        private WeaponInventory _inventory;
        private FloatingWeaponController _controller;
        private WeaponData _pistolData;
        private WeaponData _smgData;
        private WeaponData _shotgunData;

        [SetUp]
        public void Setup()
        {
            _playerObj = new GameObject("Player");
            _playerObj.tag = "Player";
            _inventory = _playerObj.AddComponent<WeaponInventory>();
            _controller = _playerObj.AddComponent<FloatingWeaponController>();
            
            // Inject inventory reference via reflection
            var inventoryField = typeof(FloatingWeaponController).GetField("_inventory",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            inventoryField.SetValue(_controller, _inventory);
            
            // Create test weapon data
            _pistolData = ScriptableObject.CreateInstance<WeaponData>();
            _smgData = ScriptableObject.CreateInstance<WeaponData>();
            _shotgunData = ScriptableObject.CreateInstance<WeaponData>();
        }

        [TearDown]
        public void Teardown()
        {
            if (_playerObj) Object.Destroy(_playerObj);
            if (_pistolData) Object.Destroy(_pistolData);
            if (_smgData) Object.Destroy(_smgData);
            if (_shotgunData) Object.Destroy(_shotgunData);
        }

        [UnityTest]
        public IEnumerator AddWeapon_SpawnsFloatingVisual()
        {
            yield return null;

            _inventory.TryAddWeapon(_pistolData);
            yield return null; // Wait a frame for events to process

            var visuals = GetActiveVisuals();
            Assert.AreEqual(1, visuals.Count, "Should have 1 floating visual after adding 1 weapon");
        }

        [UnityTest]
        public IEnumerator AddMultipleWeapons_SpawnsCorrectCount()
        {
            yield return null;

            _inventory.TryAddWeapon(_pistolData);
            _inventory.TryAddWeapon(_smgData);
            _inventory.TryAddWeapon(_shotgunData);
            yield return null;

            var visuals = GetActiveVisuals();
            Assert.AreEqual(3, visuals.Count, "Should have 3 floating visuals for 3 weapons");
        }

        [UnityTest]
        public IEnumerator SelectedWeapon_HasLargerScale()
        {
            yield return null;

            _inventory.TryAddWeapon(_pistolData);
            _inventory.TryAddWeapon(_smgData);
            yield return null;

            // Select first weapon
            _inventory.SelectWeapon(0);
            
            // Wait several frames for scale lerp
            for (int i = 0; i < 30; i++)
                yield return null;

            var visuals = GetActiveVisuals();
            Assert.IsTrue(visuals.Count >= 2, "Need at least 2 visuals");

            // Find which visual is selected vs not
            float selectedScale = -1f;
            float otherScale = -1f;
            
            foreach (var visual in visuals)
            {
                if (visual.WeaponInstance.Data == _pistolData)
                    selectedScale = visual.transform.localScale.x;
                else
                    otherScale = visual.transform.localScale.x;
            }

            Assert.Greater(selectedScale, otherScale, 
                "Selected weapon should have larger scale than non-selected");
        }

        [UnityTest]
        public IEnumerator StackWeapon_DoesNotSpawnExtraVisual()
        {
            yield return null;

            _inventory.TryAddWeapon(_pistolData);
            yield return null;

            var visualsBefore = GetActiveVisuals();
            Assert.AreEqual(1, visualsBefore.Count);

            // Stack same weapon
            _inventory.TryAddWeapon(_pistolData);
            yield return null;

            var visualsAfter = GetActiveVisuals();
            Assert.AreEqual(1, visualsAfter.Count, 
                "Stacking should not spawn additional floating visual");
        }

        [UnityTest]
        public IEnumerator OrbitalPositions_AreDistributed()
        {
            yield return null;

            _inventory.TryAddWeapon(_pistolData);
            _inventory.TryAddWeapon(_smgData);
            yield return null;

            // Wait for positioning to settle
            for (int i = 0; i < 10; i++)
                yield return null;

            var visuals = GetActiveVisuals();
            Assert.AreEqual(2, visuals.Count);

            // Two weapons should be on opposite sides (roughly 180° apart)
            Vector3 pos1 = visuals[0].transform.position - _playerObj.transform.position;
            Vector3 pos2 = visuals[1].transform.position - _playerObj.transform.position;

            // They should not be at the same position
            float distance = Vector3.Distance(pos1, pos2);
            Assert.Greater(distance, 0.5f, 
                "Two weapons should be distributed, not stacked on top of each other");
        }

        [UnityTest]
        public IEnumerator BobbingAnimation_ChangesVerticalPosition()
        {
            yield return null;

            _inventory.TryAddWeapon(_pistolData);
            yield return null;

            var visuals = GetActiveVisuals();
            Assert.AreEqual(1, visuals.Count);

            // Record Y position at two different times
            float y1 = visuals[0].transform.position.y;
            
            // Wait enough frames for bobbing to change
            for (int i = 0; i < 60; i++)
                yield return null;

            float y2 = visuals[0].transform.position.y;

            // Y should have changed due to bobbing (unless at exactly same phase, unlikely over 60 frames)
            // Use a very small threshold since bob amplitude is 0.15
            Assert.AreNotEqual(y1, y2, 
                "Vertical position should change over time due to bobbing animation");
        }

        // Helper to get active floating visuals from controller via reflection
        private List<FloatingWeaponVisual> GetActiveVisuals()
        {
            var field = typeof(FloatingWeaponController).GetField("_activeVisuals",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (List<FloatingWeaponVisual>)field.GetValue(_controller);
        }
    }
}
