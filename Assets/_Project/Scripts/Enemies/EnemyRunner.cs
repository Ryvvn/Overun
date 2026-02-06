using UnityEngine;
using UnityEngine.AI;
using Overun.Player;

namespace Overun.Enemies
{
    /// <summary>
    /// Fast runner enemy - moves quickly, low health.
    /// Component that modifies the base Enemy stats on Awake.
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class EnemyRunner : MonoBehaviour
    {
        [Header("Runner Settings")]
        [SerializeField] private float _speedMultiplier = 1.8f;
        [SerializeField] private float _healthMultiplier = 0.5f;
        [SerializeField] private float _damageMultiplier = 0.7f;
        
        private Enemy _enemy;
        private NavMeshAgent _agent;
        
        private void Awake()
        {
            _enemy = GetComponent<Enemy>();
            _agent = GetComponent<NavMeshAgent>();
            
            // Apply multipliers to base stats
            if (_enemy != null)
            {
                _enemy.SetStats(
                    _enemy.MaxHealth * _healthMultiplier,
                    _enemy.ContactDamage * _damageMultiplier,
                    _enemy.MoveSpeed * _speedMultiplier
                );
                
                if (_agent != null)
                {
                    _agent.speed = _enemy.MoveSpeed;
                }
            }
        }
    }
}
