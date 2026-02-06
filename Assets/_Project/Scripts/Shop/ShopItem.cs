using UnityEngine;
using Overun.Weapons;

namespace Overun.Shop
{
    [System.Serializable]
    public class ShopItem
    {
        public WeaponData Weapon;
        public int BaseCost;
        public bool IsPurchased;
        
        public ShopItem(WeaponData weapon, int baseCost)
        {
            Weapon = weapon;
            BaseCost = baseCost;
            IsPurchased = false;
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
