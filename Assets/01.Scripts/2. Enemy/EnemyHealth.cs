using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private bool _isDead;
    [SerializeField] private Enemy _enemy;

    private void Awake()
    {
        _enemy = GetComponent<Enemy>();
    }

    public void Init()
    {
        _isDead = false;
    }

    public void Kill()
    {
        if (_isDead)
            return;

        _isDead = true;
        _enemy.OnDeath();
    }
}
