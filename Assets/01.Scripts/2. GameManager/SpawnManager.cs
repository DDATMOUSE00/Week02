using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private Transform _player; //플레이어 연결
    [SerializeField] private Transform[] _spawnPoints; //스폰 위치


    private void OnEnable()
    {
        EventManager.Instance.AddListener(MEventType.GameStateChanged, OnRespawnStart);
    }
    private void OnDisable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.RemoveListener(MEventType.GameStateChanged, this);
        }

    }
    private void OnRespawnStart(MEventType type, Component sender, System.EventArgs args)
    {
        //게임 시작시 몇마리가 나올지 리스폰되는게 들어갈 자리
    }

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

    //클릭테스트 *비쌈
    public void SpawnAt(Vector3 center, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Enemy enemy = PoolManager.Instance.GetEnemy();

            float offsetX = GetGaussianOffset(15f, 10); //(넓이, 몰리는정도)
            Vector3 spawnPos = center + new Vector3(offsetX, 0f, 0f);

            enemy.transform.position = spawnPos;
            enemy.Init(_player);
        }
    }

    //정규분포 값 계산 *비쌈
    private float GetGaussianOffset(float range, int sampleCount)
    {
        float sum = 0f;

        for (int i = 0; i < sampleCount; i++)
        {
            sum += Random.Range(-range, range);
        }

        return sum / sampleCount;
    }

}
