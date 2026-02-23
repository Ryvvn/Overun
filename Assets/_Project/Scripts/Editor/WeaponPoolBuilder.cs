using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Overun.Weapons;
using Overun.Shop;
using Overun.Combat;

namespace Overun.Editor
{
    /// <summary>
    /// Editor utility to create starter WeaponData assets and a WeaponPoolConfig.
    /// Access via Overun > Create Weapon Pool.
    /// </summary>
    public class WeaponPoolBuilder : EditorWindow
    {
        [MenuItem("Overun/Create Weapon Pool")]
        public static void ShowWindow()
        {
            GetWindow<WeaponPoolBuilder>("Weapon Pool Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("Weapon Pool Asset Creator", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Creates 5 starter weapons (one per type) and a WeaponPoolConfig asset.\n" +
                "Assets are saved to Assets/_Project/Data/Weapons/",
                MessageType.Info);
            
            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("Generate Starter Weapons + Pool"))
            {
                GenerateWeaponsAndPool();
            }
        }

        private static void GenerateWeaponsAndPool()
        {
            string weaponDir = "Assets/_Project/Data/Weapons";
            string poolDir = "Assets/_Project/Data";
            
            // Ensure directories exist
            if (!AssetDatabase.IsValidFolder("Assets/_Project"))
                AssetDatabase.CreateFolder("Assets", "_Project");
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Data"))
                AssetDatabase.CreateFolder("Assets/_Project", "Data");
            if (!AssetDatabase.IsValidFolder(weaponDir))
                AssetDatabase.CreateFolder("Assets/_Project/Data", "Weapons");

            List<WeaponData> createdWeapons = new List<WeaponData>();

            // Define starter weapons
            var weaponDefs = new[]
            {
                new WeaponDef("Peashooter",    WeaponType.Pistol,         WeaponRarity.Common,    ElementType.None,  10f, 4f,  1, 0f),
                new WeaponDef("Buzzsaw",       WeaponType.SMG,            WeaponRarity.Uncommon,   ElementType.None,  6f,  12f, 1, 5f),
                new WeaponDef("Thunderblast",  WeaponType.Shotgun,        WeaponRarity.Rare,       ElementType.Lightning, 25f, 1.5f, 5, 15f),
                new WeaponDef("Frostbite",     WeaponType.Rifle,          WeaponRarity.Epic,       ElementType.Ice,   35f, 2f,  1, 2f),
                new WeaponDef("Hellfire MK-9", WeaponType.RocketLauncher, WeaponRarity.Legendary,  ElementType.Fire,  80f, 0.5f, 1, 0f),
            };

            foreach (var def in weaponDefs)
            {
                string assetPath = $"{weaponDir}/{def.Name.Replace(" ", "_")}.asset";
                
                // Skip if already exists
                if (AssetDatabase.LoadAssetAtPath<WeaponData>(assetPath) != null)
                {
                    WeaponData existing = AssetDatabase.LoadAssetAtPath<WeaponData>(assetPath);
                    createdWeapons.Add(existing);
                    Debug.Log($"[WeaponPool] Weapon '{def.Name}' already exists, skipping.");
                    continue;
                }

                WeaponData weapon = ScriptableObject.CreateInstance<WeaponData>();
                
                // Use SerializedObject to set private fields
                AssetDatabase.CreateAsset(weapon, assetPath);
                SerializedObject so = new SerializedObject(weapon);
                so.FindProperty("_weaponName").stringValue = def.Name;
                so.FindProperty("_weaponType").enumValueIndex = (int)def.Type;
                so.FindProperty("_rarity").enumValueIndex = (int)def.Rarity;
                so.FindProperty("_element").enumValueIndex = (int)def.Element;
                so.FindProperty("_baseDamage").floatValue = def.Damage;
                so.FindProperty("_fireRate").floatValue = def.FireRate;
                so.FindProperty("_projectilesPerShot").intValue = def.Projectiles;
                so.FindProperty("_spreadAngle").floatValue = def.Spread;
                so.FindProperty("_rarityColor").colorValue = WeaponData.GetRarityColor(def.Rarity);
                so.ApplyModifiedProperties();
                
                createdWeapons.Add(weapon);
                Debug.Log($"[WeaponPool] Created weapon: {def.Name} ({def.Rarity})");
            }

            // Create Pool Config
            string poolPath = $"{poolDir}/WeaponPool.asset";
            WeaponPoolConfig pool = AssetDatabase.LoadAssetAtPath<WeaponPoolConfig>(poolPath);
            
            if (pool == null)
            {
                pool = ScriptableObject.CreateInstance<WeaponPoolConfig>();
                AssetDatabase.CreateAsset(pool, poolPath);
            }
            
            SerializedObject poolSO = new SerializedObject(pool);
            SerializedProperty weaponsList = poolSO.FindProperty("_weapons");
            weaponsList.ClearArray();
            
            for (int i = 0; i < createdWeapons.Count; i++)
            {
                weaponsList.InsertArrayElementAtIndex(i);
                weaponsList.GetArrayElementAtIndex(i).objectReferenceValue = createdWeapons[i];
            }
            
            poolSO.ApplyModifiedProperties();
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"[WeaponPool] Created pool with {createdWeapons.Count} weapons at {poolPath}");
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = pool;
        }

        private struct WeaponDef
        {
            public string Name;
            public WeaponType Type;
            public WeaponRarity Rarity;
            public ElementType Element;
            public float Damage;
            public float FireRate;
            public int Projectiles;
            public float Spread;

            public WeaponDef(string name, WeaponType type, WeaponRarity rarity, ElementType element, float damage, float fireRate, int projectiles, float spread)
            {
                Name = name;
                Type = type;
                Rarity = rarity;
                Element = element;
                Damage = damage;
                FireRate = fireRate;
                Projectiles = projectiles;
                Spread = spread;
            }
        }
    }
}
