using UnityEngine;
using TMPro;
using Overun.Combat;

namespace Overun.UI
{
    /// <summary>
    /// Floating damage number that fades out.
    /// </summary>
    public class DamageNumber : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private TextMeshPro _text;
        [SerializeField] private float _lifetime = 1f;
        [SerializeField] private float _floatSpeed = 2f;
        [SerializeField] private float _fadeStartTime = 0.5f;
        [SerializeField] private AnimationCurve _scaleCurve = AnimationCurve.EaseInOut(0, 1.5f, 1, 0.5f);
        
        private float _spawnTime;
        private Color _baseColor;
        private Vector3 _velocity;
        private CanvasGroup _canvasGroup;
        
        private void Awake()
        {

            if (_text == null)
            {
                _text = GetComponent<TextMeshPro>();
            }
            
            if (_text != null)
            {
                _baseColor = _text.color;
            }
        }
        
        public void Initialize(float damage, Color color, bool isCritical = false)
        {
            _spawnTime = Time.time;
            _baseColor = color;
            
            if (_text != null)
            {
                _text.text = Mathf.RoundToInt(damage).ToString();
                _text.color = color;
                
                if (isCritical)
                {
                    _text.text += "!";
                    _text.fontSize *= 1.5f;
                }
            }
            
            // Random horizontal offset
            _velocity = new Vector3(
                Random.Range(-0.5f, 0.5f),
                _floatSpeed,
                0f
            );
            
            // Face camera
            if (Camera.main != null)
            {
                transform.rotation = Camera.main.transform.rotation;
            }
        }
        
        public void InitializeElemental(float damage, ElementType element)
        {
            Color elementColor = ElementUtils.GetElementColor(element);
            Initialize(damage, elementColor);
        }

        public void InitializeResistText(string text)
        {
            _spawnTime = Time.time;
            _baseColor = Color.white;
            
            if (_text != null)
            {
                _text.text = text;
                _text.color = _baseColor;
            }
            
            // Random horizontal offset
            _velocity = new Vector3(
                Random.Range(-0.5f, 0.5f),
                _floatSpeed,
                0f
            );
            
            // Face camera
            if (Camera.main != null)
            {
                transform.rotation = Camera.main.transform.rotation;
            }
        }
        
        private void Update()
        {
            float elapsed = Time.time - _spawnTime;
            float progress = elapsed / _lifetime;
            
            // Move upward
            transform.position += _velocity * Time.deltaTime;
            _velocity.y *= 0.95f; // Slow down
            
            // Scale
            float scale = _scaleCurve.Evaluate(progress);
            transform.localScale = Vector3.one * scale;
            
            // Fade
            if (elapsed > _fadeStartTime && _text != null)
            {
                float fadeProgress = (elapsed - _fadeStartTime) / (_lifetime - _fadeStartTime);
                Color c = _baseColor;
                c.a = 1f - fadeProgress;
                _text.color = c;
            }
            
            // Destroy when done
            if (elapsed >= _lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}
