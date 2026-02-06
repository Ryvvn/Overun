using UnityEngine;
using UnityEngine.AI;
using Overun.Combat;

namespace Overun.Enemies
{
    /// <summary>
    /// Enemy AI that chases the player and deals contact damage.
    /// Uses NavMeshAgent for pathfinding.
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class EnemyChaseAI : MonoBehaviour, IStunnable
    {
        [Header("AI Settings")]
        [SerializeField] private float _detectionRange = 50f;
        [SerializeField] private float _attackRange = 1.5f;
        [SerializeField] private float _updateTargetInterval = 0.25f;
        
        [Header("Attack")]
        [SerializeField] private float _attackCooldown = 1f;
        [SerializeField] private float _contactDamageMultiplier = 1f;
        
        [Header("References")]
        [SerializeField] private NavMeshAgent _agent;
        
        private Enemy _enemy;
        private Transform _target;
        private float _lastTargetUpdateTime;
        private float _lastAttackTime;
        private StatusEffectManager _statusEffectManager;

        private float _originalMoveSpeed;


        private void OnEnable()
        {
            _statusEffectManager.OnEffectApplied += OnEffectAdded;
            _statusEffectManager.OnEffectRemoved += OnEffectRemoved;
        }
        
        private void OnDisable()
        {
            _statusEffectManager.OnEffectApplied -= OnEffectAdded;
            _statusEffectManager.OnEffectRemoved -= OnEffectRemoved;
        }

        public void Stun(float duration)
        {
            _agent.speed = 0f;
            Invoke("Unstun", duration);
        }

        private void Unstun()
        {
            _agent.speed = _originalMoveSpeed;
        }
        
        private void OnEffectAdded(ElementType effect)
        {
            if(effect == ElementType.Ice)
            {
                // Get slow percent from status effect manager
                float slowPercent = _statusEffectManager.GetSlowPercent();
                if (slowPercent > 0)
                {
                    _agent.speed = _originalMoveSpeed * (1 - slowPercent);
                }
            }

            if(effect == ElementType.Lightning)
            {
                Debug.Log("[EnemyChaseAI] Enemy is stunned");
                _agent.speed = 0f;
            }
           
        }

        private void OnEffectRemoved(ElementType effect)
        {
            if(effect == ElementType.Ice)
            {
                _agent.speed = _originalMoveSpeed;
            }

            if(effect == ElementType.Lightning)
            {
                _agent.speed = _originalMoveSpeed;
            }
        }

        private void Awake()
        {
            _enemy = GetComponent<Enemy>();
            _statusEffectManager = _enemy.GetComponent<StatusEffectManager>();
            _originalMoveSpeed = _enemy.MoveSpeed;
            
            if (_agent == null)
            {
                _agent = GetComponent<NavMeshAgent>();
            }
            
            // Create NavMeshAgent if not present
            if (_agent == null)
            {
                _agent = gameObject.AddComponent<NavMeshAgent>();
            }
        }

        
        private void Start()
        {
            // Configure agent from enemy stats
            if (_agent != null)
            {
                _agent.speed = _enemy.MoveSpeed;
                _agent.stoppingDistance = _attackRange * 0.8f;
            }
            
            // Find player
            FindTarget();
        }
        
        private void Update()
        {
            if (_enemy.IsDead)
            {
                if (_agent != null) _agent.enabled = false;
                return;
            }

          
            
            // Periodically update target position
            if (Time.time - _lastTargetUpdateTime > _updateTargetInterval)
            {
                UpdateTargetPosition();
                _lastTargetUpdateTime = Time.time;
            }
            
            // Check for attack opportunity
            if (_target != null)
            {
                float distanceToTarget = Vector3.Distance(transform.position, _target.position);
                
                if (distanceToTarget <= _attackRange && CanAttack())
                {
                    Attack();
                }
            }
        }
        
        private void FindTarget()
        {
            // Find player by tag or component
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _target = player.transform;
            }
            else
            {
                // Fallback: find PlayerHealth component
                var playerHealth = FindObjectOfType<Player.PlayerHealth>();
                if (playerHealth != null)
                {
                    _target = playerHealth.transform;
                }
            }
            
            if (_target == null)
            {
                Debug.LogWarning("[EnemyChaseAI] No player found!");
            }
        }
        
        private void UpdateTargetPosition()
        {
            if (_target == null)
            {
                FindTarget();
                return;
            }
            
            float distanceToTarget = Vector3.Distance(transform.position, _target.position);
            
            // Only chase if within detection range
            if (distanceToTarget <= _detectionRange)
            {
                if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
                {
                    _agent.SetDestination(_target.position);
                }
            }
        }
        
        private bool CanAttack()
        {
            return Time.time - _lastAttackTime >= _attackCooldown;
        }
        
        private void Attack()
        {
            if (_target == null) return;
            
            _lastAttackTime = Time.time;
            
            // Try to damage player
            var playerHealth = _target.GetComponent<Player.PlayerHealth>();
            if (playerHealth != null)
            {
                float damage = _enemy.ContactDamage * _contactDamageMultiplier;
                playerHealth.TakeDamage(damage);
                
                Debug.Log($"[EnemyChaseAI] Attacked player for {damage} damage!");
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            // Detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _detectionRange);
            
            // Attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _attackRange);
        }
    }
}
