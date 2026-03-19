using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private Transform player; // 플레이어 연결
    [SerializeField] private Transform[] spawnPoints; // 스폰 위치 (여러 개면 좋음)

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Spawn(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Spawn(10);
        }
    }

    private void Spawn(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Enemy enemy = PoolManager.Instance.GetEnemy();

            //랜덤 위치 선택
            Vector3 spawnPos = spawnPoints[Random.Range(0, spawnPoints.Length)].position;

            enemy.transform.position = spawnPos;

            enemy.Init(player);
        }
    }
}
