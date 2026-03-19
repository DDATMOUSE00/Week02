using System;
using UnityEngine;

public class PlayerMove_sj : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private bool _stageStart = false; // 스테이지 시작 여부
    [SerializeField] private float _speed = 5f;        // 플레이어 이동 속도

    private void OnEnable()
    {
        if (EventManager.Instance == null) return;

        // 스테이지 시작 및 실패 이벤트 구독
        EventManager.Instance.AddListener(MEventType.StageStarted, OnGameStart);
        EventManager.Instance.AddListener(MEventType.StageFailed, OnGameFail);
    }

    private void OnDisable()
    {
        if (EventManager.Instance == null) return;

        // 이벤트 구독 해제 (메모리 누수 방지)
        EventManager.Instance.RemoveListener(MEventType.StageStarted, this);
        EventManager.Instance.RemoveListener(MEventType.StageFailed, this);
    }

    private void Update()
    {
        // 스테이지가 시작되지 않았으면 이동 로직을 실행하지 않음
        if (!_stageStart) return;

        // D키: 오른쪽 이동, A키: 왼쪽 이동
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += Vector3.right * (_speed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.A))
        {
            transform.position += Vector3.left * (_speed * Time.deltaTime);
        }
    }

    // 이벤트: 게임 시작 시 호출
    private void OnGameStart(MEventType eventType, Component sender, EventArgs args)
    {
        _stageStart = true;
    }

    // 이벤트: 게임 실패 시 호출
    private void OnGameFail(MEventType eventType, Component sender, EventArgs args)
    {
        _stageStart = false;
    }
}