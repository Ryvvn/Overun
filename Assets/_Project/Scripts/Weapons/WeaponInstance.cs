using UnityEngine;

namespace Overun.Weapons
{
    /// <summary>
    /// Runtime instance of a weapon with stack count.
    /// Used in player inventory.
    /// </summary>
    [System.Serializable]
    public class WeaponInstance
    {
        [SerializeField] private WeaponData _data;
        [SerializeField] private int _stackCount = 1;
        
        private float _lastFireTime;
        
        public WeaponData Data => _data;
        public int StackCount => _stackCount;
        public float LastFireTime => _lastFireTime;
        
        public float Damage => _data.GetDamage(_stackCount);
        public float FireRate => _data.GetFireRate(_stackCount);
        public float FireInterval => _data.GetFireInterval(_stackCount);
        
        public WeaponInstance(WeaponData data)
        {
            _data = data;
            _stackCount = 1;
            _lastFireTime = 0f;
        }
        
        public bool CanFire()
        {
            return Time.time - _lastFireTime >= FireInterval;
        }
        
        public bool CanAutoFire()
        {
            float autoInterval = FireInterval / _data.AutoFireRateMultiplier;
            return Time.time - _lastFireTime >= autoInterval;
        }
        
        public void RecordFire()
        {
            _lastFireTime = Time.time;
        }
        
        public bool TryStack()
        {
            if (_stackCount >= _data.MaxStacks)
            {
                return false;
            }
            
            _stackCount++;
            return true;
        }
        
        public bool CanStack()
        {
            return _stackCount < _data.MaxStacks;
        }

        public float GetLastFireTime()
        {
            return _lastFireTime;
        }
    }
}
