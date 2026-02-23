using UnityEngine;
using System;
using Overun.Combat;

namespace Overun.Combat
{
    public enum ChaosModifierType
    {
        None,
        VolatileAtmosphere, // Fire x2
        ZeroKelvin,         // Ice slow increased
        ShortCircuit,       // Lightning chains +2
        ToxicCloud          // Poison duration x2
    }

    /// <summary>
    /// Manages global chaos modifiers that affect elemental rules each wave.
    /// </summary>
    public class ChaosModifierSystem : MonoBehaviour
    {
        public static ChaosModifierSystem Instance { get; private set; }
        
        [Header("Debug")]
        [SerializeField] private ChaosModifierType _activeModifier = ChaosModifierType.None;
        [SerializeField] private bool _autoRandomize = false;

        public event Action<ChaosModifierType> OnModifierChanged;

        public event Action<string, Color> OnNotification;
        
        public ChaosModifierType ActiveModifier => _activeModifier;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        private void Start()
        {
            if (_autoRandomize)
            {
                RandomizeModifier();
            }
            else if(_activeModifier != ChaosModifierType.None)
            {
                SetModifier(_activeModifier);
            }
        }
        
        [ContextMenu("Randomize Modifier")]
        public void RandomizeModifier()
        {
            var values = Enum.GetValues(typeof(ChaosModifierType));
            _activeModifier = (ChaosModifierType)values.GetValue(UnityEngine.Random.Range(0, values.Length));
            
            Debug.Log($"[Chaos] Active Modifier: {_activeModifier}");
            OnModifierChanged?.Invoke(_activeModifier);
            switch(_activeModifier)
            {
                case ChaosModifierType.None:
                    OnNotification?.Invoke($"Chaos Modifier: None", Color.white);
                    break;
                case ChaosModifierType.VolatileAtmosphere:
                    OnNotification?.Invoke($"Chaos Modifier: Volatile Atmosphere", Color.red);
                    break;
                case ChaosModifierType.ZeroKelvin:
                    OnNotification?.Invoke($"Chaos Modifier: Zero Kelvin", Color.cyan);
                    break;
                case ChaosModifierType.ShortCircuit:
                    OnNotification?.Invoke($"Chaos Modifier: Short Circuit", Color.yellow);
                    break;
                case ChaosModifierType.ToxicCloud:
                    OnNotification?.Invoke($"Chaos Modifier: Toxic Cloud", Color.green);
                    break;
            }
        }
        
        public void SetModifier(ChaosModifierType type)
        {
            _activeModifier = type;
            Debug.Log($"[Chaos] Set Modifier: {_activeModifier}");
            OnModifierChanged?.Invoke(_activeModifier);
            switch(_activeModifier)
            {
                case ChaosModifierType.None:
                    OnNotification?.Invoke($"Chaos Modifier: None", Color.white);
                    break;
                case ChaosModifierType.VolatileAtmosphere:
                    OnNotification?.Invoke($"Chaos Modifier: Volatile Atmosphere", Color.red);
                    break;
                case ChaosModifierType.ZeroKelvin:
                    OnNotification?.Invoke($"Chaos Modifier: Zero Kelvin", Color.cyan);
                    break;
                case ChaosModifierType.ShortCircuit:
                    OnNotification?.Invoke($"Chaos Modifier: Short Circuit", Color.yellow);
                    break;
                case ChaosModifierType.ToxicCloud:
                    OnNotification?.Invoke($"Chaos Modifier: Toxic Cloud", Color.green);
                    break;
            }
        }
        
        // Multiplier Getters
        
        public float GetFireDamageMultiplier()
        {
            return _activeModifier == ChaosModifierType.VolatileAtmosphere ? 2.0f : 1.0f;
        }
        
        public float GetIceSlowMultiplier()
        {
            // E.g. 1.5x stronger slow
            return _activeModifier == ChaosModifierType.ZeroKelvin ? 1.5f : 1.0f;
        }
        
        public int GetLightningChainBonus()
        {
            return _activeModifier == ChaosModifierType.ShortCircuit ? 2 : 0;
        }
        
        public float GetPoisonDurationMultiplier()
        {
            return _activeModifier == ChaosModifierType.ToxicCloud ? 2.0f : 1.0f;
        }
    }
}
