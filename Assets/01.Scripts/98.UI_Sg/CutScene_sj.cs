using System;
using UnityEngine;
using UnityEngine.UI;

// 1. Singleton<T>을 상속받도록 수정합니다.
public class CutScene_sj : Singleton<CutScene_sj>
{
    [Header("카툰 내용물")]
    [SerializeField] private Image[] _start_cutSceneObjects;
    [SerializeField] private Image[] _ending_cutSceneObjects;

    [Header("컷수 숫자")]
    [SerializeField] private int _start_cutSceneNumber = 5;
    [SerializeField] private int _ending_cutSceneNumber = 5;

    [Header("뒤의 배경")]
    [SerializeField] private GameObject _cutScene_Background;

    [Header("임시)시작 컷씬인지 끝 컷씬인지 true면 엔딩")]
    [SerializeField] private bool _isEndingCutScene = false;

    

    private int _currentCutScene = 0;

    // [추가1] 컷씬이 완전히 끝나고 Update가 1초에 60번씩 튜토리얼을 부르는 걸 막는 자물쇠
    private bool _isCutSceneFinished = false;

    // 2. Singleton 클래스에서 요구하는 추상 메서드 Init을 구현합니다.
    public override void Init()
    {
        Debug.Log("CutScene Manager Initialized.");
    }

    private void OnEnable()
    {
        if (EventManager.Instance == null) return;
        EventManager.Instance.AddListener(MEventType.GameStateChanged, OnCutsceneEnd);
        EventManager.Instance.AddListener(MEventType.StartCutScene, OnCutsceneStart);
    }

    private void OnDisable()
    {
        if (EventManager.Instance == null) return;
        EventManager.Instance.RemoveListener(MEventType.GameStateChanged, this);
        EventManager.Instance.RemoveListener(MEventType.StartCutScene, this);
    }

    void Start() {
        _cutScene_Background.SetActive(true);
    }

    void Update()
    {   // [추가] 현재 게임 상태가 컷씬 상태가 아니면 이 스크립트를 작동시키지 않음
        if (GameManager.Instance.CurrentState != GameState.StartCutScene) return;
        // [추가2] 컷씬이 다 끝난 상태면 Update 로직을 완전히 정지시킴
        if (_isCutSceneFinished) return;

        CutSceneUpdate();
        PressAnyButton();
    }

    private void PressAnyButton()
    {
        if (Input.anyKeyDown)
        {
            _currentCutScene++;
        }
    }

    private void CutSceneUpdate()
    {
        // [수정1] = 를 == 로 고침 (비교 연산)
        if (_isEndingCutScene == false && _currentCutScene < _start_cutSceneNumber)
        {
            _start_cutSceneObjects[_currentCutScene].gameObject.SetActive(true);
        }
        else if (_isEndingCutScene == true && _currentCutScene < _ending_cutSceneNumber)
        {
            _ending_cutSceneObjects[_currentCutScene].gameObject.SetActive(true);
        }
        else
        {
            // 모든 컷씬이 끝난 후 한 번 더 눌러서 else로 넘어왔을 때
            // 자물쇠를 걸어 무한 반복 실행을 막음
            _isCutSceneFinished = true;

            if (_isEndingCutScene)
            {
                for (int i = 0; i < _ending_cutSceneNumber; i++)
                {
                    _ending_cutSceneObjects[i].gameObject.SetActive(false);
                }

                //엔딩 컷씬이 끝났을 때 게임 종료
                //GameManager.Instance.GameClear();
            }
            //스타트 컷씬이 끝났을 때 튜토리얼 진입 및 페이드 아웃은 여기서 처리
            else
            {
                for (int i = 0; i < _start_cutSceneNumber; i++)
                {
                    _start_cutSceneObjects[i].gameObject.SetActive(false);
                }

                // [추가3] 컷씬이 끝났을 때 페이드 아웃 및 튜토리얼 진입
                FadeController_sj fade = FindObjectOfType<FadeController_sj>();
                if (fade != null) fade.FadeOut(_cutScene_Background);

                if (GameManager.Instance != null)
                {   Debug.Log("컷씬 끝, 튜토리얼 진입");
                    GameManager.Instance.TutorialStart();
                    TutorialManager.Instance.StartTutorial();
                }

                _currentCutScene = 0; // 컷씬 초기화
            }
        }
    }

    private void OnCutsceneStart(MEventType MEventType, Component Sender, EventArgs args)
    {
        _currentCutScene = 0;
        _isCutSceneFinished = false; // 컷씬 새로 시작할 때 자물쇠 풀기

        // 엔딩컷씬이면 컷씬 상태 들어갈때 페이드인
        if (_isEndingCutScene == true)
        {
            FadeController_sj fade = FindObjectOfType<FadeController_sj>();
            if (fade != null) fade.FadeIn(_cutScene_Background);
        }

        foreach (var img in _start_cutSceneObjects) if (img != null) img.gameObject.SetActive(false);
        foreach (var img in _ending_cutSceneObjects) if (img != null) img.gameObject.SetActive(false);

        CutSceneUpdate();
    }

    private void OnCutsceneEnd(MEventType MEventType, Component Sender, EventArgs args)
    {
        _currentCutScene = 0;

        //// 스타트 컷씬이면 컷씬 상태 나갈 때 페이드 아웃
        //if (_isEndingCutScene == false)
        //{
        //    // 여기서 FadeOut을 또 부르면 Update 문이랑 중복 실행되므로 
        //    // 가장 안전하게 위쪽 CutSceneUpdate의 else 안에서만 처리하도록 비워둠
        //}

        foreach (var img in _start_cutSceneObjects) if (img != null) img.gameObject.SetActive(false);
        foreach (var img in _ending_cutSceneObjects) if (img != null) img.gameObject.SetActive(false);
    }

    // [추가4] StartScene_sj 에서 버튼으로 컷씬을 부르기 위한 다리 역할
  
}