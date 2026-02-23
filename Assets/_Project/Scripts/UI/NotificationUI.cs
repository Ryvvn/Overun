using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Overun.UI
{
    /// <summary>
    /// Manages global notifications displayed to the player.
    /// </summary>
    public class NotificationUI : MonoBehaviour
    {
        [Header("Notification Settings")]
        [SerializeField] private float _displayTime = 3f;
        [SerializeField] private float _fadeTime = 0.5f;
        [SerializeField] private Color _defaultColor = Color.white;
        [SerializeField] private Color _warningColor = Color.yellow;
        [SerializeField] private Color _errorColor = Color.red;
        
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _text;
        private Coroutine _currentCoroutine;

        private void OnEnable()
        {
            Overun.Combat.ChaosModifierSystem.Instance.OnNotification += ShowNotification;
        }
        
        private void OnDisable()
        {
            Overun.Combat.ChaosModifierSystem.Instance.OnNotification -= ShowNotification;
        }

        private void Awake()
        {
            if(_text != null)
            {
                _text.text = "";
            }
        }
        public void ShowNotification(string message, Color color)
        {
            if (_currentCoroutine != null)
            {
                StopCoroutine(_currentCoroutine);
            }
            
            _currentCoroutine = StartCoroutine(ShowNotificationCoroutine(message, color));
        }
        
        private IEnumerator ShowNotificationCoroutine(string message, Color color)
        {
            _text.text = message;
            _text.color = color;
            
            // Fade in
            float timer = 0f;
            while (timer < _fadeTime)
            {
                timer += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, timer / _fadeTime);
                yield return null;
            }

           // Animated text with smooth pulse
            timer = 0f;
            while (timer < _displayTime)
            {
                timer += Time.deltaTime;
                float scale = 1f + Mathf.Sin(timer * 2f) * 0.1f; // Sine wave for smooth pulse
                _text.transform.localScale = new Vector3(scale, scale, scale);
                yield return null;
            }   
                        
            // Fade out
            timer = 0f;
            while (timer < _fadeTime)
            {
                timer += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, timer / _fadeTime);
                _text.color = new Color(color.r, color.g, color.b, alpha);
                yield return null;
            }
            
            _currentCoroutine = null;
        }
    }
}
