using UnityEngine;
using Overun.Weapons;

namespace Overun.Shop
{
    [System.Serializable]
    public class ShopItem
    {
        public WeaponData Weapon;
        public int BaseCost;
        public int CurrentCost; // Modified by Panic Market
        public bool IsPurchased;
        public bool IsGlitch;
        
        public ShopItem(WeaponData weapon, int cost)
        {
            Weapon = weapon;
            BaseCost = cost;
            CurrentCost = cost; // Initially same as base
            IsPurchased = false;
            IsGlitch = false;
        }
        
        public int GetCost(int ownedCount)
        {
            // Simple inflation: +50% per owned copy
            // e.g. 100 -> 150 -> 225
            float multiplier = Mathf.Pow(1.5f, ownedCount);
            return Mathf.RoundToInt(BaseCost * multiplier);
        }
    }
}
