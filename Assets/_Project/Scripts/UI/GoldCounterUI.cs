using UnityEngine;
using TMPro;
using Overun.Currency;
using System.Collections;

namespace Overun.UI
{
    /// <summary>
    /// Displays player's current gold.
    /// </summary>
    public class GoldCounterUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI _text;
        
        [Header("Animation")]
        [SerializeField] private float _punchScale = 1.2f;
        [SerializeField] private float _animDuration = 0.2f;
        [SerializeField] private Color _gainColor = Color.yellow;
        private Color _originalColor;
        private Vector3 _originalScale;
        
        private void Awake()
        {
            if (_text != null)
            {
                _originalColor = _text.color;
                _originalScale = _text.transform.localScale;
            }
        }
        
        private void Start()
        {
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnGoldChanged += UpdateDisplay;
                UpdateDisplay(CurrencyManager.Instance.CurrentGold, 0);
            }
            else
            {
                Debug.LogWarning("[GoldCounterUI] CurrencyManager not found!");
            }
        }
        
        private void OnDestroy()
        {
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.OnGoldChanged -= UpdateDisplay;
            }
        }
        
        private void UpdateDisplay(int current, int change)
        {
            if (_text == null) return;
            
            _text.text = $"Gold: {current}";
            
            if (change != 0)
            {
                StopAllCoroutines();
                StartCoroutine(AnimateChange());
            }
        }
        
        private IEnumerator AnimateChange()
        {
            float elapsed = 0f;
            Transform t = _text.transform;
            
            _text.color = _gainColor;
            
            while (elapsed < _animDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float p = elapsed / _animDuration;
                
                // Scale ping-pong
                float scale = 1f;
                if (p < 0.5f)
                    scale = Mathf.Lerp(1f, _punchScale, p * 2f);
                else
                    scale = Mathf.Lerp(_punchScale, 1f, (p - 0.5f) * 2f);
                    
                t.localScale = _originalScale * scale;
                yield return null;
            }
            
            t.localScale = _originalScale;
            _text.color = _originalColor;
        }
    }
}
