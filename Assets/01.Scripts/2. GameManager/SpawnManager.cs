using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private Transform player; //플레이어 연결
    [SerializeField] private Transform[] spawnPoints; //스폰 위치

    private void Update()
    {
        
    }

    public void Spawn(int count) //public 테스트 끝나면 빼기
    {
        if (player == null)
        {
            return;
        }
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
