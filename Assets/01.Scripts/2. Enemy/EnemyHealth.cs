using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    private bool isDead;
    private Enemy enemy;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    public void Init()
    {
        isDead = false;
    }

    public void Kill()
    {
        if (isDead)
            return;

        isDead = true;
        enemy.OnDeath();
    }
}
