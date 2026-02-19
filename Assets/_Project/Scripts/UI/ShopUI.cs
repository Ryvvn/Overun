using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Overun.UI;
using System;
using Overun.Currency;

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
        
        [Header("Feedback Colors")]
        [SerializeField] private Color _successColor = new Color(0.2f, 0.9f, 0.2f);
        [SerializeField] private Color _upgradeColor = new Color(1f, 0.8f, 0.2f);
        [SerializeField] private Color _errorColor = new Color(0.9f, 0.3f, 0.3f);
        [SerializeField] private Color _warningColor = new Color(0.9f, 0.5f, 0.2f);
        [SerializeField] private Color _neutralColor = new Color(0.7f, 0.5f, 0.9f);
        [SerializeField] private Color _panicColor = new Color(1f, 0f, 1f);
        
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
                ShopManager.Instance.OnShopRefreshed += RefreshDisplay;
                ShopManager.Instance.OnPurchaseAttempt += HandlePurchaseResult;
                ShopManager.Instance.OnPanicMarket += OnPanicMarket;
                ShopManager.Instance.OnRerollCostChanged += UpdateRerollButton;
                
                // Subscribe to currency changes to update button interactability
                if (CurrencyManager.Instance != null)
                {
                    CurrencyManager.Instance.OnGoldChanged += OnGoldChanged;
                }
                
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
                ShopManager.Instance.OnShopRefreshed -= RefreshDisplay;
                ShopManager.Instance.OnPurchaseAttempt -= HandlePurchaseResult;
                ShopManager.Instance.OnPanicMarket -= OnPanicMarket;
                ShopManager.Instance.OnRerollCostChanged -= UpdateRerollButton;
            }
            
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnGoldChanged -= OnGoldChanged;
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
                UpdateRerollButton(ShopManager.Instance.CurrentRerollCost);
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
            ShopManager.Instance.TryRerollItems();
        }

        private void UpdateRerollButton(int cost)
        {
            if (_rerollCostText != null)
            {
                _rerollCostText.text = $"Reroll ({cost}g)";
            }
            
            UpdateRerollInteractability();
        }
        
        private void OnGoldChanged(int newAmount, int changeAmount)
        {
            UpdateRerollInteractability();
        }
        
        private void UpdateRerollInteractability()
        {
            if (_rerollButton != null && ShopManager.Instance != null && CurrencyManager.Instance != null)
            {
                bool canAfford = CurrencyManager.Instance.CanAfford(ShopManager.Instance.CurrentRerollCost);
                _rerollButton.interactable = canAfford;
                
                // Visual feedback for disabled state (optional, if button transition isn't enough)
                if (_rerollCostText != null)
                {
                    _rerollCostText.color = canAfford ? Color.white : _errorColor;
                }
            }
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
                    color = _successColor;
                    break;
                case PurchaseResult.Upgraded:
                    message = $"UPGRADED {item.Weapon.WeaponName}!";
                    color = _upgradeColor;
                    break;
                case PurchaseResult.InsufficientGold:
                    message = "Not enough gold!";
                    color = _errorColor;
                    break;
                case PurchaseResult.InventoryFull:
                    message = "Inventory full!";
                    color = _warningColor;
                    break;
                case PurchaseResult.AlreadyMaxTier:
                    message = "Already at MAX tier!";
                    color = _neutralColor;
                    break;
            }
            
            ShowFeedback(message, color);
        }
        
        private void OnPanicMarket()
        {
            ShowFeedback("PANIC MARKET!", _panicColor);
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
