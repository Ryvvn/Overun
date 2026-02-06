using UnityEngine;

namespace Overun.Weapons
{
    /// <summary>
    /// Defines weapon properties as a ScriptableObject.
    /// Used for weapon types, drops, and inventory.
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponData", menuName = "Overun/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _weaponName = "Pistol";
        [SerializeField] private WeaponType _weaponType = WeaponType.Pistol;
        [SerializeField] private WeaponRarity _rarity = WeaponRarity.Common;
        [SerializeField] private Overun.Combat.ElementType _element = Overun.Combat.ElementType.None;
        [SerializeField] private float _effectDuration = 3f;
        [SerializeField] private float _effectStrength= 1f;
        
        [Header("Projectile")]
        [SerializeField] private bool _isHitScan = false;
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private string _projectilePoolTag;
        [SerializeField] private float _projectileSpeed = 40f;
        [SerializeField] private float _baseDamage = 10f;
        
        [Header("Fire Rate")]
        [SerializeField] private float _fireRate = 5f; // shots per second
        [SerializeField] private int _projectilesPerShot = 1;
        [SerializeField] private float _spreadAngle = 0f;
        [SerializeField] private float _spreadIncreasePerShot = 5f;
        
        [Header("Auto-Fire")]
        [SerializeField] private float _autoFireRange = 15f;
        [SerializeField] private float _autoFireRateMultiplier = 0.7f; // Slower than manual
        
        [Header("Stacking")]
        [SerializeField] private float _stackDamageBonus = 0.25f; // +25% per stack
        [SerializeField] private float _stackFireRateBonus = 0.1f; // +10% per stack
        [SerializeField] private int _maxStacks = 5;
        
        [Header("Visuals")]
        [SerializeField] private Sprite _icon;
        [SerializeField] private Color _rarityColor = Color.white;
        
        // Properties
        public string WeaponName => _weaponName;
        public WeaponType Type => _weaponType;
        public WeaponRarity Rarity => _rarity;
        public Overun.Combat.ElementType Element => _element;
        public bool IsHitScan => _isHitScan;
        public GameObject ProjectilePrefab => _projectilePrefab;
        public string ProjectilePoolTag => _projectilePoolTag;
        public float ProjectileSpeed => _projectileSpeed;
        public float BaseDamage => _baseDamage;
        public float FireRate => _fireRate;
        public int ProjectilesPerShot => _projectilesPerShot;
        public float SpreadAngle => _spreadAngle;
        public float SpreadIncreasePerShot => _spreadIncreasePerShot;
        public float AutoFireRange => _autoFireRange;
        public float AutoFireRateMultiplier => _autoFireRateMultiplier;
        public float StackDamageBonus => _stackDamageBonus;
        public float StackFireRateBonus => _stackFireRateBonus;
        public int MaxStacks => _maxStacks;
        public Sprite Icon => _icon;
        public Color RarityColor => _rarityColor;
        
        /// <summary>
        /// Get damage for this weapon at a given stack level.
        /// </summary>
        public float GetDamage(int stackCount)
        {
            float stackMultiplier = 1f + (_stackDamageBonus * Mathf.Max(0, stackCount - 1));
            return _baseDamage * stackMultiplier;
        }
        
        /// <summary>
        /// Get fire rate at a given stack level.
        /// </summary>
        public float GetFireRate(int stackCount)
        {
            float stackMultiplier = 1f + (_stackFireRateBonus * Mathf.Max(0, stackCount - 1));
            return _fireRate * stackMultiplier;
        }
        
        /// <summary>
        /// Get time between shots.
        /// </summary>
        public float GetFireInterval(int stackCount = 1)
        {
            return 1f / GetFireRate(stackCount);
        }
        
        /// <summary>
        /// Get the color associated with this weapon's rarity.
        /// </summary>
        public static Color GetRarityColor(WeaponRarity rarity)
        {
            return rarity switch
            {
                WeaponRarity.Common => new Color(0.7f, 0.7f, 0.7f),      // Gray
                WeaponRarity.Uncommon => new Color(0.2f, 0.8f, 0.2f),    // Green
                WeaponRarity.Rare => new Color(0.2f, 0.4f, 0.9f),        // Blue
                WeaponRarity.Epic => new Color(0.6f, 0.2f, 0.8f),        // Purple
                WeaponRarity.Legendary => new Color(1f, 0.7f, 0.1f),     // Orange/Gold
                _ => Color.white
            };
        }
    }
    
    public enum WeaponType
    {
        Pistol,
        SMG,
        Shotgun,
        Rifle,
        RocketLauncher
    }
    
    public enum WeaponRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
}
