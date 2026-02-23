using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Overun.Shop;
using Overun.Currency;
using Overun.Weapons;

namespace Overun.Tests.PlayMode
{
    /// <summary>
    /// Story 6-7: Weapon Pool Integration tests.
    /// Covers weighted selection, duplicate prevention, glitch uniform selection.
    /// </summary>
    public class WeaponPoolTests
    {
        private WeaponPoolConfig _pool;
        private WeaponData _common1;
        private WeaponData _common2;
        private WeaponData _rare;
        private WeaponData _legendary;

        [SetUp]
        public void Setup()
        {
            _pool = ScriptableObject.CreateInstance<WeaponPoolConfig>();
            
            _common1 = ScriptableObject.CreateInstance<WeaponData>();
            _common2 = ScriptableObject.CreateInstance<WeaponData>();
            _rare = ScriptableObject.CreateInstance<WeaponData>();
            _legendary = ScriptableObject.CreateInstance<WeaponData>();
            
            // Set rarities via reflection (private fields)
            SetRarity(_common1, WeaponRarity.Common);
            SetRarity(_common2, WeaponRarity.Common);
            SetRarity(_rare, WeaponRarity.Rare);
            SetRarity(_legendary, WeaponRarity.Legendary);
            
            // Set pool weapons via reflection
            var field = typeof(WeaponPoolConfig).GetField("_weapons",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(_pool, new List<WeaponData> { _common1, _common2, _rare, _legendary });
        }

        [TearDown]
        public void Teardown()
        {
            if (_pool) Object.Destroy(_pool);
            if (_common1) Object.Destroy(_common1);
            if (_common2) Object.Destroy(_common2);
            if (_rare) Object.Destroy(_rare);
            if (_legendary) Object.Destroy(_legendary);
        }

        private void SetRarity(WeaponData weapon, WeaponRarity rarity)
        {
            var field = typeof(WeaponData).GetField("_rarity",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(weapon, rarity);
        }

        [Test]
        public void SelectWeaponWeighted_ReturnsNonNull()
        {
            WeaponData result = _pool.SelectWeaponWeighted();
            Assert.IsNotNull(result);
        }

        [Test]
        public void SelectWeaponWeighted_FavorsCommonWeapons()
        {
            // Over many samples, common weapons should appear more often
            int commonCount = 0;
            int legendaryCount = 0;
            int samples = 1000;
            
            for (int i = 0; i < samples; i++)
            {
                WeaponData result = _pool.SelectWeaponWeighted();
                if (result == _common1 || result == _common2) commonCount++;
                if (result == _legendary) legendaryCount++;
            }
            
            // Common (2 weapons at 40 weight each = 80) should vastly exceed Legendary (1 at 2 weight)
            Assert.Greater(commonCount, legendaryCount,
                $"Common ({commonCount}) should be picked more often than Legendary ({legendaryCount})");
            Assert.Greater(commonCount, samples / 4,
                $"Common should appear in at least 25% of samples, got {commonCount}/{samples}");
        }

        [Test]
        public void SelectWeaponWeighted_RespectsExcludeSet()
        {
            var exclude = new HashSet<WeaponData> { _common1, _common2, _rare };
            
            // With all but legendary excluded, should always get legendary
            for (int i = 0; i < 50; i++)
            {
                WeaponData result = _pool.SelectWeaponWeighted(exclude);
                Assert.AreEqual(_legendary, result, "Should only return non-excluded weapon");
            }
        }

        [Test]
        public void SelectWeaponWeighted_FallsBackWhenAllExcluded()
        {
            var exclude = new HashSet<WeaponData> { _common1, _common2, _rare, _legendary };
            
            // When all excluded, should fallback to full pool
            WeaponData result = _pool.SelectWeaponWeighted(exclude);
            Assert.IsNotNull(result, "Should fallback to full pool when all excluded");
        }

        [Test]
        public void SelectWeaponUniform_ReturnsNonNull()
        {
            WeaponData result = _pool.SelectWeaponUniform();
            Assert.IsNotNull(result);
        }

        [Test]
        public void SelectWeaponUniform_AllWeaponsCanBeSelected()
        {
            HashSet<WeaponData> seen = new HashSet<WeaponData>();
            
            // Over many samples, all weapons should appear at least once
            for (int i = 0; i < 500; i++)
            {
                seen.Add(_pool.SelectWeaponUniform());
            }
            
            Assert.AreEqual(4, seen.Count, "All 4 weapons should be selectable with uniform distribution");
        }

        [Test]
        public void GetRarityWeight_ReturnsExpectedValues()
        {
            Assert.AreEqual(40f, _pool.GetRarityWeight(WeaponRarity.Common));
            Assert.AreEqual(30f, _pool.GetRarityWeight(WeaponRarity.Uncommon));
            Assert.AreEqual(20f, _pool.GetRarityWeight(WeaponRarity.Rare));
            Assert.AreEqual(8f, _pool.GetRarityWeight(WeaponRarity.Epic));
            Assert.AreEqual(2f, _pool.GetRarityWeight(WeaponRarity.Legendary));
        }

        [Test]
        public void Pool_Count_MatchesWeaponCount()
        {
            Assert.AreEqual(4, _pool.Count);
        }
    }
}
