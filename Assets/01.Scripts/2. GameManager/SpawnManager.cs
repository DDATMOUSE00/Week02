using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private PlayerDistanceChecker _distanceChecker; //플레이어 거리 체크
    [SerializeField] private Transform _player; //플레이어 연결
    [SerializeField] private Transform[] _spawnPoints; //스폰 위치

    [Header("Distance Spawn")]
    [SerializeField] private float _spawnInterval = 50f; //마지막 스폰 거리에서 스폰 거리 설정
    [SerializeField] private int _spawnCountPerInterval = 80; //소환 될 마릿수(랜덤 될듯)
    [SerializeField] private float _minSpawnDistanceFromPlayer = 28f; //플레이어랑 가까우면 안 나오게

    [SerializeField] private Vector3 _lastSpawnPosition; //마지막 스폰 기준 위치
    [SerializeField] private bool _isSpawnActive; //거리 스폰 활성화 여부

    [SerializeField] private float _groundY = -4.8f; //바닥 높이
    [SerializeField] private float _spawnForwardDistance = 50f; //플레이어 기준 앞쪽 거리
    [SerializeField] private float _spawnXRandomRange = 60f; //X축 랜덤 퍼짐 범위


    private void OnEnable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.AddListener(MEventType.GameStateChanged, OnRespawnStart);
        }
    }
    private void OnDisable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.RemoveListener(MEventType.GameStateChanged, this);
        }

    }

    private void Start()
    {
        if (_player == null)
        {
            return;
        }

        if (_distanceChecker != null)
        {
            _distanceChecker.Init(_player);
        }

        _lastSpawnPosition = _player.position;
        _isSpawnActive = true;
    }

    private void Update()
    {
        if (_player == null)
            return;

        CheckDistanceSpawn(_player.position);
    }
    private void OnRespawnStart(MEventType type, Component sender, System.EventArgs args)
    {
        //게임 시작시 몇마리가 나올지 리스폰되는게 들어갈 자리
        if (_player == null)
            return;

        if (_distanceChecker != null)
        {
            _distanceChecker.Init(_player);
        }

        _lastSpawnPosition = _player.position;
        _isSpawnActive = true;
    }

    public void CheckDistanceSpawn(Vector3 playerPosition)
    {
        //실제 움직였을 때 적 소환
        if (!_isSpawnActive)
            return;

        if (_player == null)
            return;

        //플레이어가 마지막 스폰 기준점에서 x축으로 얼마나 이동했는지 체크
        float movedX = playerPosition.x - _lastSpawnPosition.x;

        //아직 스폰 거리만큼 못 갔으면 종료
        if (movedX < _spawnInterval)
            return;

        //플레이어가 x축으로 _spawnInterval 이상 이동했으면 무조건 스폰
        SpawnAt(_spawnCountPerInterval);

        //이번 스폰 시점을 새로운 기준 위치로 저장
        _lastSpawnPosition = playerPosition;
    }


    //spawn인데 안 쓸듯
    //public void Spawn(int count)
    //{
    //    if (_player == null)
    //    {
    //        return;
    //    }
    //    for (int i = 0; i < count; i++)
    //    {
    //        Enemy enemy = PoolManager.Instance.GetEnemy();

    //        //랜덤 위치 선택
    //        Vector3 spawnPos = _spawnPoints[Random.Range(0, _spawnPoints.Length)].position;

    //        enemy.transform.position = spawnPos;

    //        enemy.Init(_player);
    //    }
    //}

    //범위 랜덤 생성 *비쌈
    public void SpawnAt(int count)
    {
        if (_player == null)
            return;

        //무한 루프 방지(디버그)
        int spawnedCount = 0;
        int tryCount = 0;
        int maxTryCount = count * 5;


        float minX = _player.position.x + _spawnForwardDistance;
        float maxX = minX + _spawnXRandomRange;
        float centerX = (minX + maxX) * 0.5f;
        float halfRange = (maxX - minX) * 0.5f;

        while (spawnedCount < count && tryCount < maxTryCount)
        {
            tryCount++;

            Enemy enemy = PoolManager.Instance.GetEnemy();

            float offsetX = GetGaussianOffset(halfRange, 3);
            float spawnX = centerX + offsetX;

            //오른쪽 스폰 구간 밖으로 안 나가게 고정
            spawnX = Mathf.Clamp(spawnX, minX, maxX);

            Vector3 spawnPos = new Vector3(spawnX, _groundY, 0f);

            enemy.transform.position = spawnPos;

            Enemy.EnemyType randomType = (Enemy.EnemyType)Random.Range(0, 4);
            enemy.Init(_player, randomType);

            spawnedCount++;
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




    //기즈모
    private void OnDrawGizmos()
    {
        if (_player == null)
            return;

        // 플레이어 위치
        Vector3 playerPos = _player.position;

        //빨강 최소 스폰 거리 (가까우면 안되는 영역)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(playerPos, _minSpawnDistanceFromPlayer);

        //노랑 스폰 트리거 거리 (이동 기준)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(playerPos, _spawnInterval);

        //초록 실제 적이 생길 위치
        Gizmos.color = Color.green;
        Vector3 spawnBasePos = new Vector3(_player.position.x + _spawnForwardDistance, _groundY, 0f);
        Gizmos.DrawWireSphere(spawnBasePos, 1.5f);
    }
}
