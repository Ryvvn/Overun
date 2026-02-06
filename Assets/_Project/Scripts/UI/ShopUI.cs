using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Overun.Shop
{
    public class ShopUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject _shopPanel;
        [SerializeField] private Transform _itemsContainer;
        [SerializeField] private GameObject _itemTemplate;
        [SerializeField] private Button _rerollButton;
        [SerializeField] private TextMeshProUGUI _rerollCostText;
        [SerializeField] private Button _continueButton;
        
        private List<GameObject> _spawnedItems = new List<GameObject>();
        
        private void Start()
        {
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.OnShopOpened += OpenShop;
                ShopManager.Instance.OnShopClosed += CloseShop;
                ShopManager.Instance.OnItemsRefreshed += RefreshDisplay;
                
                // Hide initially
                _shopPanel.SetActive(false);
            }
            
            _rerollButton.onClick.AddListener(OnRerollClicked);
            _continueButton.onClick.AddListener(OnContinueClicked);
            
            if (_itemTemplate != null) _itemTemplate.SetActive(false);
        }
        
        private void OnDestroy()
        {
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.OnShopOpened -= OpenShop;
                ShopManager.Instance.OnShopClosed -= CloseShop;
                ShopManager.Instance.OnItemsRefreshed -= RefreshDisplay;
            }
        }
        
        private void OpenShop()
        {
            _shopPanel.SetActive(true);
            RefreshDisplay();
        }
        
        private void CloseShop()
        {
            _shopPanel.SetActive(false);
        }
        
        private void RefreshDisplay()
        {
            // Update Reroll Text
            if (_rerollCostText != null && ShopManager.Instance != null)
            {
                _rerollCostText.text = $"Reroll ({ShopManager.Instance.RerollCost}g)";
            }
            
            // Clear old items
            foreach (var item in _spawnedItems)
            {
                Destroy(item);
            }
            _spawnedItems.Clear();
            
            if (ShopManager.Instance == null) return;
            
            List<ShopItem> items = ShopManager.Instance.CurrentItems;
            
            for (int i = 0; i < items.Count; i++)
            {
                int index = i;
                ShopItem itemData = items[i];
                
                GameObject itemObj = Instantiate(_itemTemplate, _itemsContainer);
                itemObj.SetActive(true);
                
                // Populate UI (Assuming template has standard children layouts)
                // In a real project, we'd have a ShopItemUI component
                
                var texts = itemObj.GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length >= 2)
                {
                    texts[0].text = itemData.Weapon.WeaponName; // Name
                    texts[1].text = itemData.IsPurchased ? "SOLD" : $"{itemData.BaseCost}g"; // Cost
                }
                
                var img = itemObj.GetComponentInChildren<Image>(); // Icon
                 // Ideally get specific icon image comp
                
                Button btn = itemObj.GetComponentInChildren<Button>();
                if (btn != null)
                {
                    btn.interactable = !itemData.IsPurchased;
                    btn.onClick.AddListener(() => OnBuyClicked(index));
                }
                
                _spawnedItems.Add(itemObj);
            }
        }
        
        private void OnRerollClicked()
        {
            ShopManager.Instance.RerollItems();
        }
        
        private void OnBuyClicked(int index)
        {
            if (ShopManager.Instance.TryBuyItem(index))
            {
                RefreshDisplay(); // Update buttons/sold state
            }
        }
        
        private void OnContinueClicked()
        {
            ShopManager.Instance.CloseShop();
        }
    }
}
