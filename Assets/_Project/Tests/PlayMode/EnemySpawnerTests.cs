// using System.Collections;
// using NUnit.Framework;
// using UnityEngine;
// using UnityEngine.TestTools;
// using Overun.Waves;
// using Overun.Core;
// using System.Collections.Generic;

// namespace Overun.Tests.Waves
// {
//     public class EnemySpawnerTests
//     {
//         private GameObject _spawnerObj;
//         private EnemySpawner _spawner;
//         private GameObject _waveManagerObj;
//         private WaveManager _waveManager;
//         private WaveConfig _config;

//         [SetUp]
//         public void Setup()
//         {
//             _waveManagerObj = new GameObject("WaveManager");
//             _waveManager = _waveManagerObj.AddComponent<WaveManager>();
            
//             _config = ScriptableObject.CreateInstance<WaveConfig>();
//             // Use reflection or serialized property to set config if needed, or if public setter exists.
//             // WaveManager._waveConfig is private serialized.
//             // For this test, we might mock dependencies or just test public methods if possible.
//             // However, EnemySpawner depends on WaveManager singleton.
            
//             _spawnerObj = new GameObject("EnemySpawner");
//             _spawner = _spawnerObj.AddComponent<EnemySpawner>();
//         }

//         [TearDown]
//         public void Teardown()
//         {
//             if (_spawnerObj) Object.Destroy(_spawnerObj);
//             if (_waveManagerObj) Object.Destroy(_waveManagerObj);
//             Object.Destroy(_config);
//         }

//         [UnityTest]
//         public IEnumerator SpawnNextEnemy_SpawnsEnemy_WhenCalled()
//         {
//             // Setup Prefab
//             var prefab = new GameObject("TestEnemy");
//             prefab.AddComponent<Enemy>(); // Ensure it has Enemy component
            
//             // Assign to Spawner via reflection since fields are private serialized
//             var basicField = typeof(EnemySpawner).GetField("_basicEnemyPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//             basicField.SetValue(_spawner, prefab);
            
//             // Setup WaveManager Config
//             // We need to inject the config into WaveManager
//             var configField = typeof(WaveManager).GetField("_waveConfig", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//             configField.SetValue(_waveManager, _config);
            
//             // Start a wave
//             _waveManager.StartNextWave();
//             yield return null; // Wait for coroutine
            
//             // Wait for wave start delay (1s default)
//             // We can modify config to 0 delay
//             var delayField = typeof(WaveConfig).GetField("_waveStartDelay", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//             delayField.SetValue(_config, 0f);
            
//             // Now actually start logic
//             // WaveManager should trigger OnReadyToSpawn eventually
//             // But we can manually call spawner.SpawnNextEnemy() if it's public.
//             // It is public!
            
//             _spawner.SpawnNextEnemy();
            
//             Assert.AreEqual(1, _spawner.ActiveEnemyCount);
//             Assert.IsNotNull(_spawner.ActiveEnemies[0]);
            
//             // Cleanup
//             Object.Destroy(prefab);
//         }
//     }
// }
