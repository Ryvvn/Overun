using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Overun.Waves;
using Overun.Core;
using UnityEditor;

namespace Overun.Tests.PlayMode
{
    public class WaveManagerVictoryTests
    {
        private GameObject _waveManagerObj;
        private WaveManager _waveManager;
        private WaveConfig _config;

        [SetUp]
        public void Setup()
        {
            _waveManagerObj = new GameObject("WaveManager");
            _waveManager = _waveManagerObj.AddComponent<WaveManager>();
            
            _config = ScriptableObject.CreateInstance<WaveConfig>();
            
            // Inject config via reflection
            var configField = typeof(WaveManager).GetField("_waveConfig", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            configField.SetValue(_waveManager, _config);
        }

        [TearDown]
        public void Teardown()
        {
            if (_waveManagerObj) Object.Destroy(_waveManagerObj);
            Object.Destroy(_config);
        }

        [UnityTest]
        public IEnumerator StartNextWave_TriggersVictory_WhenMaxWavesExceeded()
        {
            // Set MaxWaves to 1
            var maxWavesField = typeof(WaveConfig).GetField("_maxWaves", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            maxWavesField.SetValue(_config, 1);

            bool victoryTriggered = false;
            _waveManager.OnAllWavesCompleted += () => victoryTriggered = true;

            // Start Wave 1
            _waveManager.StartNextWave(); 
            yield return null;
            
            Assert.AreEqual(1, _waveManager.CurrentWaveNumber);
            Assert.IsFalse(victoryTriggered, "Victory should not trigger on Wave 1 start");

            // Mock wave completion (usually happens when enemies die)
            // We can just forcefully start next wave to simulate progression
            // Wave 1 -> Wave 2 (which is > MaxWaves(1))
            
            // We need to bypass the "IsWaveActive" check or manually reset it
            var isActiveField = typeof(WaveManager).GetField("_isWaveActive", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            isActiveField.SetValue(_waveManager, false);
            
            _waveManager.StartNextWave();
            yield return null;

            Assert.AreEqual(2, _waveManager.CurrentWaveNumber);
            Assert.IsTrue(victoryTriggered, "Victory should trigger when starting Wave 2 (Max is 1)");
        }

        [UnityTest]
        public IEnumerator EnterEndlessMode_AllowsWaveProgression_WithoutVictory()
        {
            // Set MaxWaves to 1
            var maxWavesField = typeof(WaveConfig).GetField("_maxWaves", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            maxWavesField.SetValue(_config, 1);

            bool victoryTriggered = false;
            _waveManager.OnAllWavesCompleted += () => victoryTriggered = true;

            // Start Wave 1
            _waveManager.StartNextWave();
            
            // Manually finish wave 1
            var isActiveField = typeof(WaveManager).GetField("_isWaveActive", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            isActiveField.SetValue(_waveManager, false);
            
            // Enter Endless Mode (this should start next wave AND set flag)
            _waveManager.EnterEndlessMode();
            yield return null;

            Assert.AreEqual(2, _waveManager.CurrentWaveNumber);
            Assert.IsFalse(victoryTriggered, "Victory should NOT trigger in Endless Mode");
        }
    }
}


