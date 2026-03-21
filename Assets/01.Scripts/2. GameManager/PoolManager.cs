using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    [Header("Pool Setting")]
    [SerializeField] private Enemy _enemyPrefab;
    [SerializeField] private int _initialSize = 300;
    [SerializeField] private Transform _poolParent;

    [SerializeField] private Queue<Enemy> _enemyPool = new Queue<Enemy>();

    //현재 활성화되어 있는 적들 관리
    [SerializeField] private HashSet<Enemy> _activeEnemies = new HashSet<Enemy>();

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
        _enemyPool.Enqueue(newEnemy);
        return newEnemy;
    }

    public Enemy GetEnemy()
    {
        if (_enemyPool.Count == 0)
        {
            CreateEnemy();
        }

        Enemy enemy = _enemyPool.Dequeue();
        enemy.gameObject.SetActive(true);

        //활성 적 목록에 추가
        _activeEnemies.Add(enemy);

        return enemy;
    }

    public void ReturnEnemy(Enemy enemy)
    {
        if (enemy == null)
            return;

        //이미 풀에 들어간 적이면 중복 반환 방지
        if (!_activeEnemies.Remove(enemy))
            return;

        enemy.gameObject.SetActive(false);
        enemy.transform.SetParent(_poolParent);
        _enemyPool.Enqueue(enemy);
    }

    public void ReturnAllEnemies()  //끝날 때 다 풀로 돌아오기
    {
        if (_activeEnemies.Count == 0)
            return;

        //반복 중 변경 방지 복사본
        Enemy[] activeEnemies = new Enemy[_activeEnemies.Count];
        _activeEnemies.CopyTo(activeEnemies);

        foreach (Enemy enemy in activeEnemies)
        {
            ReturnEnemy(enemy);
        }
    }
    public override void Init()
    {
    }
}
