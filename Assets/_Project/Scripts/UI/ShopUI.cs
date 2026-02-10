using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Overun.UI;
using System;

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
        
        [Header("Feedback UI")]
        [SerializeField] private TextMeshProUGUI _feedbackText;
        [SerializeField] private float _feedbackDuration = 1.5f;
        
        [Header("Stats")]
        [SerializeField] private PlayerStatsUI _statsUI;
        
        private List<ShopItemUI> _spawnedItems = new List<ShopItemUI>();
        private Coroutine _feedbackCoroutine;

        public event Action OnShopUIOpen;
        public event Action OnShopUIClose;
        
        private void Start()
        {
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.OnShopOpened += OpenShop;
                ShopManager.Instance.OnShopClosed += CloseShop;
                ShopManager.Instance.OnItemsRefreshed += RefreshDisplay;
                ShopManager.Instance.OnPurchaseAttempt += HandlePurchaseResult;
                ShopManager.Instance.OnPanicMarket += OnPanicMarket;
                
                // Hide initially
                _shopPanel.SetActive(false);
            }
            
            _rerollButton.onClick.AddListener(OnRerollClicked);
            _continueButton.onClick.AddListener(OnContinueClicked);
            
            if (_itemTemplate != null) _itemTemplate.SetActive(false);
            if (_feedbackText != null) _feedbackText.gameObject.SetActive(false);
        }
        
        private void OnDestroy()
        {
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.OnShopOpened -= OpenShop;
                ShopManager.Instance.OnShopClosed -= CloseShop;
                ShopManager.Instance.OnItemsRefreshed -= RefreshDisplay;
                ShopManager.Instance.OnPurchaseAttempt -= HandlePurchaseResult;
                ShopManager.Instance.OnPanicMarket -= OnPanicMarket;
            }
        }
        
        private void OpenShop()
        {
            _shopPanel.SetActive(true);
            RefreshDisplay();
            OnShopUIOpen?.Invoke();
        }
        
        private void CloseShop()
        {
            _shopPanel.SetActive(false);
            OnShopUIClose?.Invoke();
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
                if (item != null) Destroy(item.gameObject);
            }
            _spawnedItems.Clear();
            
            if (ShopManager.Instance == null) return;
            
            List<ShopItem> items = ShopManager.Instance.CurrentItems;
            
            for (int i = 0; i < items.Count; i++)
            {
                ShopItem itemData = items[i];
                
                GameObject itemObj = Instantiate(_itemTemplate, _itemsContainer);
                itemObj.SetActive(true);
                
                // Use ShopItemUI component
                ShopItemUI ui = itemObj.GetComponent<ShopItemUI>();
                if (ui == null) ui = itemObj.AddComponent<ShopItemUI>(); // Auto-add if missing
                
                ui.Setup(itemData, i);
                
                _spawnedItems.Add(ui);
            }
        }
        
        private void OnRerollClicked()
        {
            ShopManager.Instance.RerollItems();
        }
        
        private void OnContinueClicked()
        {
            ShopManager.Instance.CloseShop();
        }
        
        private void HandlePurchaseResult(PurchaseResult result, ShopItem item)
        {
            string message = "";
            Color color = Color.white;
            
            switch (result)
            {
                case PurchaseResult.Success:
                    message = $"Purchased {item.Weapon.WeaponName}!";
                    color = new Color(0.2f, 0.9f, 0.2f); // Green
                    break;
                case PurchaseResult.Upgraded:
                    message = $"UPGRADED {item.Weapon.WeaponName}!";
                    color = new Color(1f, 0.8f, 0.2f); // Gold
                    break;
                case PurchaseResult.InsufficientGold:
                    message = "Not enough gold!";
                    color = new Color(0.9f, 0.3f, 0.3f); // Red
                    break;
                case PurchaseResult.InventoryFull:
                    message = "Inventory full!";
                    color = new Color(0.9f, 0.5f, 0.2f); // Orange
                    break;
                case PurchaseResult.AlreadyMaxTier:
                    message = "Already at MAX tier!";
                    color = new Color(0.7f, 0.5f, 0.9f); // Purple
                    break;
            }
            
            ShowFeedback(message, color);
        }
        
        private void OnPanicMarket()
        {
            ShowFeedback("PANIC MARKET!", new Color(1f, 0f, 1f)); // Magenta
        }
        
        private void ShowFeedback(string message, Color color)
        {
            if (_feedbackText == null) return;
            
            if (_feedbackCoroutine != null)
            {
                StopCoroutine(_feedbackCoroutine);
            }
            
            _feedbackCoroutine = StartCoroutine(FeedbackRoutine(message, color));
        }
        
        private IEnumerator FeedbackRoutine(string message, Color color)
        {
            _feedbackText.text = message;
            _feedbackText.color = color;
            _feedbackText.gameObject.SetActive(true);
            
            // Simple scale animation
            _feedbackText.transform.localScale = Vector3.one * 1.2f;
            
            float elapsed = 0f;
            while (elapsed < _feedbackDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / _feedbackDuration;
                
                // Scale down and fade
                _feedbackText.transform.localScale = Vector3.Lerp(Vector3.one * 1.2f, Vector3.one, t);
                
                // Fade out in last 0.5 seconds
                if (t > 0.6f)
                {
                    float fadeT = (t - 0.6f) / 0.4f;
                    _feedbackText.color = new Color(color.r, color.g, color.b, 1f - fadeT);
                }
                
                yield return null;
            }
            
            _feedbackText.gameObject.SetActive(false);
            _feedbackCoroutine = null;
        }
    }
}
