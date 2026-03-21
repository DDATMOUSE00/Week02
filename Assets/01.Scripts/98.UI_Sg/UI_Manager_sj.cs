using System;
using UnityEngine;

public class UIManager_sj : MonoBehaviour
{
    [Header("하이어라키에서 묶은 그룹들 할당")]
    [SerializeField] private GameObject _lobbyGroup;
    [SerializeField] private GameObject _startCutSceneGroup;
    [SerializeField] private GameObject _tutorialGroup;

    private void OnEnable()
    {
        if (EventManager.Instance != null)
            EventManager.Instance.AddListener(MEventType.GameStateChanged, OnStateChanged);
    }

    private void OnDisable()
    {
        if (EventManager.Instance != null)
            EventManager.Instance.RemoveListener(MEventType.GameStateChanged, this);
    }
    
    // EventManager가 상태 변경 신호를 쏘면 자동으로 실행됨
    private void OnStateChanged(MEventType type, Component sender, EventArgs args)
    {
        if (args is GameStateChangedEventArgs stateArgs)
        {
            // 1. 일단 모든 UI 그룹을 끔 (배열 노가다 없이 그룹 3개만 끄면 됨)
            if (_lobbyGroup != null) _lobbyGroup.SetActive(false);
            if (_startCutSceneGroup != null) _startCutSceneGroup.SetActive(false);
            if (_tutorialGroup != null) _tutorialGroup.SetActive(false);

            // 2. 현재 상태에 맞는 그룹 딱 하나만 켬
            //엔딩컷씬 나중에 gameclear나 game over에 추가해라
            switch (stateArgs.current)
            {
                
                case GameState.StartCutScene:
                    if (_startCutSceneGroup != null) _startCutSceneGroup.SetActive(true);
                    break;
                case GameState.Tutorial:
                    if (_tutorialGroup != null) _tutorialGroup.SetActive(true);
                    break;
                case GameState.Play:
                    // 본게임 시작 시 튜토리얼 UI는 꺼지고 인게임 관련 요소 활성화
                    break;
            }
        }
    }
}