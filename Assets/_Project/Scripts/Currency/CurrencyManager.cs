using UnityEngine;
using System;

namespace Overun.Currency
{
    /// <summary>
    /// Manages player currency (Gold).
    /// </summary>
    public class CurrencyManager : MonoBehaviour
    {
        public static CurrencyManager Instance { get; private set; }
        
        [SerializeField] private int _currentGold = 0;
        [SerializeField] private bool _debugLogs = false;
        
        // Events: current, change amount
        public event Action<int, int> OnGoldChanged;
        
        public int CurrentGold => _currentGold;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        /// <summary>
        /// Add gold to player wallet.
        /// </summary>
        public void AddGold(int amount)
        {
            if (amount <= 0) return;
            
            _currentGold += amount;
            OnGoldChanged?.Invoke(_currentGold, amount);
            
            if (_debugLogs) Debug.Log($"[Currency] Added {amount} gold. Total: {_currentGold}");
        }
        
        /// <summary>
        /// Spend gold if affordable. Returns true if successful.
        /// </summary>
        public bool SpendGold(int amount)
        {
            if (amount <= 0) return true; // Free stuff is free
            
            if (_currentGold >= amount)
            {
                _currentGold -= amount;
                OnGoldChanged?.Invoke(_currentGold, -amount);
                
                if (_debugLogs) Debug.Log($"[Currency] Spent {amount} gold. Remaining: {_currentGold}");
                return true;
            }
            
            if (_debugLogs) Debug.Log($"[Currency] Not enough gold! Need {amount}, have {_currentGold}");
            return false;
        }
        
        /// <summary>
        /// Check if player can afford cost.
        /// </summary>
        public bool CanAfford(int amount)
        {
            return _currentGold >= amount;
        }
        
        /// <summary>
        /// Reset gold (for restart).
        /// </summary>
        public void ResetCurrency()
        {
            _currentGold = 0;
            OnGoldChanged?.Invoke(0, 0);
        }
    }
}
