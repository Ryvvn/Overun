using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Overun.Core;
using UnityEngine.SceneManagement;

namespace Overun.Tests.PlayMode
{
    public class GameFlowTests
    {
        private GameObject _gameManagerObj;
        private GameManager _gameManager;

        [SetUp]
        public void Setup()
        {
            if (GameManager.Instance != null)
            {
                Object.DestroyImmediate(GameManager.Instance.gameObject);
            }
            
            _gameManagerObj = new GameObject("GameManager");
            _gameManager = _gameManagerObj.AddComponent<GameManager>();
        }

        [TearDown]
        public void Teardown()
        {
            if (_gameManagerObj) Object.DestroyImmediate(_gameManagerObj);
        }

        [Test]
        public void Singleton_IsInitialized()
        {
            Assert.IsNotNull(GameManager.Instance);
            Assert.AreEqual(_gameManager, GameManager.Instance);
        }

        [Test]
        public void InitialState_IsPlaying()
        {
            Assert.AreEqual(GameManager.GameState.Playing, _gameManager.CurrentState);
        }

        [Test]
        public void PauseGame_SetsStateToPaused_AndStopsTime()
        {
            _gameManager.PauseGame();
            
            Assert.AreEqual(GameManager.GameState.Paused, _gameManager.CurrentState);
            Assert.AreEqual(0f, Time.timeScale);
        }

        [Test]
        public void ResumeGame_SetsStateToPlaying_AndResumesTime()
        {
            _gameManager.PauseGame();
            _gameManager.ResumeGame();
            
            Assert.AreEqual(GameManager.GameState.Playing, _gameManager.CurrentState);
            Assert.AreNotEqual(0f, Time.timeScale);
        }
    }
}
