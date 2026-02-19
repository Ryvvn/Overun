using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Overun.Shop;
using Overun.Weapons;
using System.Collections;

namespace Overun.UI
{
    /// <summary>
    /// UI Component for a single item in the Shop.
    /// Handles Rarity colors and Glitch effects.
    /// </summary>
    public class ShopItemUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _borderImage;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _priceText;
        [SerializeField] private TextMeshProUGUI _typeText;
        [SerializeField] private GameObject _purchasedOverlay;
        
        [Header("Lock")]
        [SerializeField] private Button _lockButton;
        [SerializeField] private Image _lockIcon;
        [SerializeField] private GameObject _lockOverlay;
        [SerializeField] private Color _lockedBorderColor = new Color(1f, 0.85f, 0.2f); // Gold
        
        [Header("Glitch Effect")]
        [SerializeField] private GameObject _glitchOverlay;
        [SerializeField] private TextMeshProUGUI _glitchText;
        
        private Button _button;
        private ShopItem _currentItem;
        private int _index;
        private Coroutine _glitchCoroutine;
        private Color _originalBorderColor;

        private ShopUI _shopUI;

        
        private void Awake()
        {
            _button = GetComponentInChildren<Button>();
            if (_button != null)
            {
                _button.onClick.AddListener(OnClicked);
            }
            
            if (_lockButton != null)
            {
                _lockButton.onClick.AddListener(OnLockClicked);
            }
            
            _shopUI = FindObjectOfType<ShopUI>();
        }

        private void OnEnable()
        {
            if(_shopUI != null)
            {
                _shopUI.OnShopUIOpen += OpenShopUI;
                _shopUI.OnShopUIClose += CloseShopUI;
            }
        }

        private void OnDisable()
        {
            if(_shopUI != null)
            {
                _shopUI.OnShopUIOpen -= OpenShopUI;
                _shopUI.OnShopUIClose -= CloseShopUI;
            }
        }
      
        
        public void Setup(ShopItem item, int index)
        {
            _currentItem = item;
            _index = index;
            
            if (_glitchCoroutine != null) StopCoroutine(_glitchCoroutine);
            
            if (item.IsGlitch)
            {
                SetupGlitchMode(item);
            }
            else
            {
                SetupNormalMode(item);
            }
            
            // Buttons state
            if (_button != null)
            {
                _button.interactable = !item.IsPurchased;
            }
            
            if (_purchasedOverlay != null)
            {
                _purchasedOverlay.SetActive(item.IsPurchased);
            }
            
            // Lock state
            UpdateLockVisuals(item);
        }
        
        private void UpdateLockVisuals(ShopItem item)
        {
            if (_lockOverlay != null)
            {
                _lockOverlay.SetActive(item.IsLocked);
            }
            
            if (_lockIcon != null)
            {
                _lockIcon.color = item.IsLocked ? _lockedBorderColor : Color.gray;
            }
            
            if (_lockButton != null)
            {
                // Can't lock purchased items
                _lockButton.gameObject.SetActive(!item.IsPurchased);
            }
            
            // Subtle border glow when locked
            if (item.IsLocked && _borderImage != null)
            {
                _originalBorderColor = _borderImage.color;
                _borderImage.color = _lockedBorderColor;
            }
        }
        
        private void SetupNormalMode(ShopItem item)
        {
            if (_glitchOverlay) _glitchOverlay.SetActive(false);
             
            if (_nameText) _nameText.text = item.Weapon.WeaponName;
            if (_typeText) _typeText.text = item.Weapon.Type.ToString();
             
            // Price with Panic Market color coding
            if (_priceText)
            {
                _priceText.text = $"{item.CurrentCost}g";
                
                // Color based on price change from Panic Market
                if (item.CurrentCost < item.BaseCost)
                {
                    // Cheaper - Green
                    _priceText.color = new Color(0.2f, 0.9f, 0.2f);
                }
                else if (item.CurrentCost > item.BaseCost)
                {
                    // More expensive - Red
                    _priceText.color = new Color(0.9f, 0.3f, 0.3f);
                }
                else
                {
                    // Normal - White
                    _priceText.color = Color.white;
                }
            }
             
            if (_iconImage)
            {
                _iconImage.sprite = item.Weapon.Icon;
                _iconImage.color = Color.white;
            }
             
            // Color logic
            Color rarityColor = WeaponData.GetRarityColor(item.Weapon.Rarity);
             
            if (_borderImage) _borderImage.color = rarityColor;
            if (_nameText) _nameText.color = rarityColor;
             
            // Background tint (subtle)
            if (_backgroundImage) _backgroundImage.color = Color.Lerp(Color.black, rarityColor, 0.1f);
        }
        
        private void SetupGlitchMode(ShopItem item)
        {
            if (_glitchOverlay) _glitchOverlay.SetActive(true);
            
            // Hide normal details
            if (_iconImage) _iconImage.color = Color.black; 
            if (_nameText) _nameText.text = "???";
            if (_typeText) _typeText.text = "UNKNOWN";
            
            // Glitch text price
            if (_priceText) _priceText.text = "50g"; // Flat fee for glitch currently
            
            Color glitchColor = new Color(1f, 0f, 1f); // Magenta
            if (_borderImage) _borderImage.color = glitchColor;
            if (_backgroundImage) _backgroundImage.color = Color.Lerp(Color.black, glitchColor, 0.2f);
            
        }

        private void OpenShopUI()
        {
            _glitchCoroutine = StartCoroutine(GlitchAnimation());
        }

        private void CloseShopUI()
        {
         
        }
        
        private IEnumerator GlitchAnimation()
        {
            if (_glitchText == null) yield break;
            
            string chars = "!@#$%^&*?<>01";
            string text = "";
            bool increase = true;
            while (true)
            {
                char c = chars[UnityEngine.Random.Range(0, chars.Length)];
                // Animation to increase the length of the text to 8 characters and then decrease it from 8 to 0
               if(increase)
                {
                    if(text.Length < 8)
                    {
                        text += c;
                    }
                    else 
                    {
                        increase = false;
                    }
                }
                else 
                {
                    if(text.Length > 0)
                    {
                        text = text.Remove(text.Length - 1, 1);
                    }
                    else
                    {
                        increase = true;
                    }
                }
              
                _glitchText.text = text;
                _glitchText.rectTransform.anchoredPosition = new Vector2(UnityEngine.Random.Range(-5f, 5f), UnityEngine.Random.Range(-5f, 5f));
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        private void OnClicked()
        {
            if (ShopManager.Instance != null && !(_currentItem?.IsPurchased ?? true))
            {
                ShopManager.Instance.TryBuyItem(_index);
                // ShopUI handles all feedback via OnPurchaseAttempt event
            }
        }
        
        private void OnLockClicked()
        {
            if (ShopManager.Instance != null && _currentItem != null && !_currentItem.IsPurchased)
            {
                ShopManager.Instance.TryLockItem(_index);
            }
        }
    }
}
