using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    [Header("Pool Setting")]
    [SerializeField] private Enemy _enemyPrefab;
    [SerializeField] private int _initialSize = 200;
    [SerializeField] private Transform _poolParent;

    private Queue<Enemy> enemyPool = new Queue<Enemy>();

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this)
            return;

        for (int i = 0; i < _initialSize; i++)
        {
            CreateEnemy();
        }
    }

    private Enemy CreateEnemy()
    {
        Enemy newEnemy = Instantiate(_enemyPrefab, _poolParent);
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
        enemy.transform.SetParent(_poolParent);
        enemyPool.Enqueue(enemy);
    }

    public override void Init()
    {
    }
}
