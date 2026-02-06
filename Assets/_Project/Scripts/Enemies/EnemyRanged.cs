using UnityEngine;
using UnityEngine.AI;
using Overun.Player;

namespace Overun.Enemies
{
    /// <summary>
    /// Ranged enemy - moves to range and shoots projectiles.
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class EnemyRanged : MonoBehaviour
    {
        [Header("Ranged Settings")]
        [SerializeField] private float _attackRange = 10f;
        [SerializeField] private float _attackCooldown = 2f;
        [SerializeField] private float _projectileDamage = 15f;
        [SerializeField] private float _projectileSpeed = 15f;
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private Transform _firePoint;
        
        private Enemy _enemy;
        private NavMeshAgent _agent;
        private Transform _target;
        private float _lastAttackTime;
        
        private void Awake()
        {
            _enemy = GetComponent<Enemy>();
            _agent = GetComponent<NavMeshAgent>();
            
            if (_agent != null)
            {
                _agent.stoppingDistance = _attackRange * 0.8f;
            }
        }
        
        private void Start()
        {
            // Find player
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
            
            // Look at target if within range
            if (dist <= _attackRange)
            {
                Vector3 lookPos = _target.position;
                lookPos.y = transform.position.y;
                transform.LookAt(lookPos);
                
                if (Time.time - _lastAttackTime >= _attackCooldown)
                {
                    Attack();
                }
            }
        }
        
        private void Attack()
        {
            _lastAttackTime = Time.time;
            
            if (_projectilePrefab != null)
            {
                Vector3 spawnPos = _firePoint != null ? _firePoint.position : transform.position + Vector3.up;
                Vector3 direction = (_target.position - spawnPos).normalized;
                
                GameObject proj = Instantiate(_projectilePrefab, spawnPos, Quaternion.LookRotation(direction));
                
                // Configure projectile
                EnemyProjectile ep = proj.GetComponent<EnemyProjectile>();
                if (ep != null)
                {
                    ep.Initialize(_projectileDamage, _projectileSpeed);
                }
            }
        }
    }
}
