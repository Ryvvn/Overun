using UnityEngine;
using UnityEngine.AI;

namespace Overun.Enemies
{
    /// <summary>
    /// Tank enemy - slow but lots of health.
    /// Component that modifies the base Enemy stats on Awake.
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class EnemyTank : MonoBehaviour
    {
        [Header("Tank Settings")]
        [SerializeField] private float _speedMultiplier = 0.5f;
        [SerializeField] private float _healthMultiplier = 3.0f;
        [SerializeField] private float _damageMultiplier = 1.5f;
        
        private Enemy _enemy;
        private NavMeshAgent _agent;
        
        private void Awake()
        {
            _enemy = GetComponent<Enemy>();
            _agent = GetComponent<NavMeshAgent>();
            
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
