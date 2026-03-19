using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    [Header("Pool Setting")]
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private int initialSize = 200;
    [SerializeField] private Transform poolParent;

    private Queue<Enemy> enemyPool = new Queue<Enemy>();

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this)
            return;

        for (int i = 0; i < initialSize; i++)
        {
            CreateEnemy();
        }
    }

    private Enemy CreateEnemy()
    {
        Enemy newEnemy = Instantiate(enemyPrefab, poolParent);
        newEnemy.gameObject.SetActive(false);
        enemyPool.Enqueue(newEnemy);
        return newEnemy;
    }

    public Enemy GetEnemy()
    {
        if (enemyPool.Count == 0)
        {
            CreateEnemy();
        }

        Enemy enemy = enemyPool.Dequeue();
        enemy.gameObject.SetActive(true);
        return enemy;
    }

    public void ReturnEnemy(Enemy enemy)
    {
        enemy.gameObject.SetActive(false);
        enemy.transform.SetParent(poolParent);
        enemyPool.Enqueue(enemy);
    }

    public override void Init()
    {
    }
}
