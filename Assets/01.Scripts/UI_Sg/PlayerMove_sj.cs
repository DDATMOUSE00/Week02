using System;
using UnityEngine;

public class PlayerMove_sj : MonoBehaviour
{
    private bool _stageStart = false;
    private void OnEnable()
    {
        if (EventManager.Instance == null)
            return;
        EventManager.Instance.AddListener(MEventType.StageStarted, OnGameStart);
        EventManager.Instance.AddListener(MEventType.StageFailed, OnGameFail);
    }


    private void OnDisable()
    {
        if (EventManager.Instance == null)
            return;
        EventManager.Instance.RemoveListener(MEventType.StageStarted, this);
        EventManager.Instance.RemoveListener(MEventType.StageFailed, this);
    }
    
  
  
    [SerializeField] private float speed = 5f; // 이동 속도

    void Update()
    {
        if (_stageStart == false)
        {
            //Debug.Log("플레이어 멈춤");
            return;
        }
        //Debug.Log("플레이어 움직임");
        // 현재 위치에서 오른쪽(Vector3.right) 방향으로 
        // (속도 * 프레임 시간)만큼 더한 새로운 위치를 대입합니다.

        if(Input.GetKey(KeyCode.D))
        {

            transform.position += Vector3.right * speed * Time.deltaTime;
        }
        if(Input.GetKey(KeyCode.A))
        {
            transform.position += Vector3.left * speed * Time.deltaTime;
        }
    }

    private void OnGameFail(MEventType MEventType, Component Sender, EventArgs args)
    {
        _stageStart = false;
    }

    private void OnGameStart(MEventType MEventType, Component Sender, EventArgs args)
    {
        _stageStart = true;
    }
}