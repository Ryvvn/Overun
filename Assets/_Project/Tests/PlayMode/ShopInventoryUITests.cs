using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Overun.UI;
using Overun.Weapons;

namespace Overun.Tests.PlayMode
{
    public class ShopInventoryUITests
    {
        private GameObject _testObj;
        private ShopInventoryUI _shopInventoryUI;
        private WeaponInventory _weaponInventory;
        private WeaponData _testWeapon;

        [SetUp]
        public void Setup()
        {
            _testObj = new GameObject("ShopInventoryTest");
            
            // Setup Player Inventory
            var playerObj = new GameObject("Player");
            playerObj.tag = "Player";
            _weaponInventory = playerObj.AddComponent<WeaponInventory>();
            
            // Setup UI
            _shopInventoryUI = _testObj.AddComponent<ShopInventoryUI>();
            
            // Reflection to set references
            var container = new GameObject("Container");
            container.transform.SetParent(_testObj.transform);
            
            var slotPrefabObj = new GameObject("SlotPrefab");
            var slotUI = slotPrefabObj.AddComponent<InventorySlotUI>();
            // Add required child objects for SlotUI setup
            var filledState = new GameObject("Filled"); filledState.transform.SetParent(slotPrefabObj.transform);
            var emptyState = new GameObject("Empty"); emptyState.transform.SetParent(slotPrefabObj.transform);
            var icon = new GameObject("Icon").AddComponent<UnityEngine.UI.Image>(); icon.transform.SetParent(filledState.transform);
            var border = new GameObject("Border").AddComponent<UnityEngine.UI.Image>(); border.transform.SetParent(filledState.transform);

            // Inject references into SlotUI prefab using generic SerializedObject if needed or just public fields if I made them public... 
            // They are private [SerializeField]. I need to use reflection or check if I can just assume they are null and handle it (Setup might crash).
            // Actually Setup uses them.
            // Let's use reflection to assign them on the prefab
            AssignPrivateField(slotUI, "_filledState", filledState);
            AssignPrivateField(slotUI, "_emptyState", emptyState);
            AssignPrivateField(slotUI, "_iconImage", icon);
            AssignPrivateField(slotUI, "_rarityBorder", border);

            AssignPrivateField(_shopInventoryUI, "_slotsContainer", container.transform);
            AssignPrivateField(_shopInventoryUI, "_slotPrefab", slotUI);
            
            _testWeapon = ScriptableObject.CreateInstance<WeaponData>();
        }

        private void AssignPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(obj, value);
        }

        [TearDown]
        public void Teardown()
        {
            if (_testObj) Object.Destroy(_testObj);
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player) Object.Destroy(player);
            if (_testWeapon) Object.Destroy(_testWeapon);
        }

        [UnityTest]
        public IEnumerator Initializes_With_PlayerInventory()
        {
            // Act
            _shopInventoryUI.gameObject.SetActive(true); // Triggers Awake and OnEnable
            
            yield return null;

            // Trigger inventory change
            _weaponInventory.TryAddWeapon(_testWeapon);
            
            yield return null;
            
            // Assert that slots were created (Logic in Awake)
            var container = _testObj.transform.Find("Container");
            Assert.AreEqual(6, container.childCount, "Should create 6 slots");
        }
    }
}
