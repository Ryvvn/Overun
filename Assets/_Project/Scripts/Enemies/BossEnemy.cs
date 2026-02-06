using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Overun.Player;

namespace Overun.Enemies
{
    /// <summary>
    /// Boss enemy with multiple attack patterns and phases.
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class BossEnemy : MonoBehaviour
    {
        [Header("Boss Stats")]
        [SerializeField] private float _healthMultiplier = 10f;
        [SerializeField] private float _damageMultiplier = 2f;
        
        [Header("Phases")]
        [SerializeField] private float _phase2HealthPercent = 0.6f;
        [SerializeField] private float _phase3HealthPercent = 0.3f;
        
        [Header("Attacks")]
        [SerializeField] private float _chargeSpeed = 15f;
        [SerializeField] private float _chargeCooldown = 5f;
        [SerializeField] private float _slamRadius = 5f;
        [SerializeField] private float _slamDamage = 30f;
        [SerializeField] private float _slamCooldown = 8f;
        
        [Header("Spawn Minions")]
        [SerializeField] private GameObject _minionPrefab;
        [SerializeField] private int _minionsPerSpawn = 3;
        [SerializeField] private float _spawnCooldown = 12f;
        
        private Enemy _enemy;
        private NavMeshAgent _agent;
        private Transform _target;
        
        private int _currentPhase = 1;
        private float _lastChargeTime;
        private float _lastSlamTime;
        private float _lastSpawnTime;
        private bool _isCharging = false;
        
        // Events
        public event System.Action<int> OnPhaseChanged;
        
        #region Unity Lifecycle
        private void Awake()
        {
            _enemy = GetComponent<Enemy>();
            _agent = GetComponent<NavMeshAgent>();
            
            // Apply boss stats
            _enemy.SetStats(
                _enemy.MaxHealth * _healthMultiplier,
                _enemy.ContactDamage * _damageMultiplier,
                _enemy.MoveSpeed * 0.7f
            );
            
            // Subscribe to health changes
            _enemy.OnHealthChanged += CheckPhaseTransition;
        }
        
        private void OnDestroy()
        {
            if (_enemy != null)
            {
                _enemy.OnHealthChanged -= CheckPhaseTransition;
            }
        }
        
        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _target = player.transform;
            }
        }
        
        private void Update()
        {
            if (_enemy.IsDead || _target == null) return;
            
            float dist = Vector3.Distance(transform.position, _target.position);
            
            // Choose attack based on phase and cooldowns

            // Spawn minions if in phase 2 or higher and cooldown has passed
            if (_currentPhase >= 2 && Time.time - _lastSpawnTime > _spawnCooldown)
            {
                SpawnMinions();
            }
            
            // Charge if distance is greater than 8f and cooldown has passed and not charging   
            if (dist > 8f && Time.time - _lastChargeTime > _chargeCooldown && !_isCharging)
            {
                StartCharge();
            }
            // Ground slam if distance is less than 4f and cooldown has passed
            else if (dist < 4f && Time.time - _lastSlamTime > _slamCooldown)
            {
                GroundSlam();
            }
        }
        #endregion

        #region Phase Transition
        private void CheckPhaseTransition(float current, float max)
        {
            float percent = current / max;
            
            if (_currentPhase == 1 && percent <= _phase2HealthPercent)
            {
                TransitionToPhase(2);
            }
            else if (_currentPhase == 2 && percent <= _phase3HealthPercent)
            {
                TransitionToPhase(3);
            }
        }
        
        private void TransitionToPhase(int phase)
        {
            _currentPhase = phase;
            OnPhaseChanged?.Invoke(phase);
            
            Debug.Log($"[Boss] Entered Phase {phase}!");
            
            // Phase buffs
            if (_agent != null)
            {
                _agent.speed *= 1.2f;
            }
        }
        #endregion

        #region Attacks
        /// <summary>
        /// Starts the charge attack.
        /// </summary>
        private void StartCharge()
        {
            _lastChargeTime = Time.time;
            _isCharging = true;
            
            if (_agent != null)
            {
                _agent.speed = _chargeSpeed;
                _agent.SetDestination(_target.position);
            }
            
            Invoke(nameof(EndCharge), 2f);
        }
        
        private void EndCharge()
        {
            _isCharging = false;
            
            if (_agent != null)
            {
                _agent.speed = _enemy.MoveSpeed;
            }
        }
        
        /// <summary>
        /// Performs the ground slam attack.
        /// </summary>
        private void GroundSlam()
        {
            _lastSlamTime = Time.time;
            
            Debug.Log("[Boss] Ground Slam!");
            
            // Damage nearby
            Collider[] colliders = Physics.OverlapSphere(transform.position, _slamRadius);
            foreach (Collider col in colliders)
            {
                if (col.CompareTag("Player"))
                {
                    PlayerHealth health = col.GetComponent<PlayerHealth>();
                    if (health != null)
                    {
                        float dist = Vector3.Distance(transform.position, col.transform.position);
                        float falloff = 1f - (dist / _slamRadius);
                        health.TakeDamage(_slamDamage * Mathf.Max(0.3f, falloff));
                    }
                }
            }
        }

        private void JumpTowardTarget()
        {
           //To do: Add animation and make the boss jump toward the target
           // Perform this action before the boss does the ground slam
           
        }
        
        private void SpawnMinions()
        {
            if (_minionPrefab == null) return;
            
            _lastSpawnTime = Time.time;
            
            Debug.Log($"[Boss] Spawning {_minionsPerSpawn} minions!");
            
            for (int i = 0; i < _minionsPerSpawn; i++)
            {
                Vector3 spawnPos = transform.position + Random.insideUnitSphere * 3f;
                spawnPos.y = transform.position.y;
                
                Instantiate(_minionPrefab, spawnPos, Quaternion.identity);
            }
        }
        #endregion

        #region Gizmos
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _slamRadius);
        }
        #endregion
    }
}
