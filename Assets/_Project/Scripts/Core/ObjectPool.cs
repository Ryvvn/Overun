using System.Collections.Generic;
using UnityEngine;

namespace Overun.Core
{
    /// <summary>
    /// Generic object pool for reusing GameObjects.
    /// Use for enemies, projectiles, VFX, etc.
    /// Copied from StayAlive project.
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        #region Singleton
        public static ObjectPool Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        #endregion

        [System.Serializable]
        public class Pool
        {
            public string Tag;
            public GameObject Prefab;
            public int InitialSize = 10;
        }

        [Header("Pools")]
        [SerializeField] private List<Pool> _pools = new();
        
        private Dictionary<string, Queue<GameObject>> _poolDictionary = new();
        private Dictionary<string, Pool> _poolSettings = new();
        private Dictionary<GameObject, string> _activeObjects = new();

        private void Start()
        {
            InitializePools();
        }

        private void InitializePools()
        {
            foreach (var pool in _pools)
            {
                Queue<GameObject> objectPool = new();
                _poolSettings[pool.Tag] = pool;
                
                for (int i = 0; i < pool.InitialSize; i++)
                {
                    GameObject obj = CreateNewPoolObject(pool);
                    objectPool.Enqueue(obj);
                }
                
                _poolDictionary[pool.Tag] = objectPool;
            }
            
            Debug.Log($"[ObjectPool] Initialized {_pools.Count} pools");
        }

        private GameObject CreateNewPoolObject(Pool pool)
        {
            GameObject obj = Instantiate(pool.Prefab, transform);
            obj.SetActive(false);
            return obj;
        }

        /// <summary>
        /// Get an object from the pool.
        /// </summary>
        public GameObject Spawn(string tag, Vector3 position, Quaternion rotation)
        {
            if (!_poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"[ObjectPool] Pool with tag '{tag}' doesn't exist!");
                return null;
            }
            
            Queue<GameObject> pool = _poolDictionary[tag];
            GameObject obj;
            
            if (pool.Count > 0)
            {
                obj = pool.Dequeue();
            }
            else
            {
                // Pool exhausted, create new object
                obj = CreateNewPoolObject(_poolSettings[tag]);
                Debug.Log($"[ObjectPool] Pool '{tag}' exhausted, created new object");
            }

            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
            
            _activeObjects[obj] = tag;
            
            // Call OnSpawnFromPool if script implements it
            var poolable = obj.GetComponent<IPoolable>();
            poolable?.OnSpawnFromPool();
            
            return obj;
        }

        /// <summary>
        /// Return an object to the pool.
        /// </summary>
        public void Despawn(GameObject obj)
        {
            if (obj == null) return;
            
            if (!_activeObjects.TryGetValue(obj, out string tag))
            {
                Debug.LogWarning($"[ObjectPool] Object {obj.name} is not from a pool!");
                Destroy(obj);
                return;
            }
            
            // Call OnDespawnToPool if script implements it
            var poolable = obj.GetComponent<IPoolable>();
            poolable?.OnDespawnToPool();
            
            obj.SetActive(false);
            obj.transform.SetParent(transform);
            
            _activeObjects.Remove(obj);
            _poolDictionary[tag].Enqueue(obj);
        }

        /// <summary>
        /// Despawn all active objects of a specific tag.
        /// </summary>
        public void DespawnAll(string tag)
        {
            var toRemove = new List<GameObject>();
            
            foreach (var kvp in _activeObjects)
            {
                if (kvp.Value == tag)
                {
                    toRemove.Add(kvp.Key);
                }
            }
            
            foreach (var obj in toRemove)
            {
                Despawn(obj);
            }
        }

        /// <summary>
        /// Get count of active objects for a tag.
        /// </summary>
        public int GetActiveCount(string tag)
        {
            int count = 0;
            foreach (var kvp in _activeObjects)
            {
                if (kvp.Value == tag) count++;
            }
            return count;
        }
    }

    /// <summary>
    /// Interface for objects that can be pooled.
    /// </summary>
    public interface IPoolable
    {
        void OnSpawnFromPool();
        void OnDespawnToPool();
    }
}
