using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private Transform _player; //플레이어 연결
    [SerializeField] private Transform[] _spawnPoints; //스폰 위치

    public void Spawn(int count)
    {
        if (_player == null)
        {
            return;
        }
        for (int i = 0; i < count; i++)
        {
            Enemy enemy = PoolManager.Instance.GetEnemy();

            //랜덤 위치 선택
            Vector3 spawnPos = _spawnPoints[Random.Range(0, _spawnPoints.Length)].position;

            enemy.transform.position = spawnPos;

            enemy.Init(_player);
        }
    }
}
