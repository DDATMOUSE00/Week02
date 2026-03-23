using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    public override void Init() { }

    //모든 UI 레퍼런스가 여기에 들어가야함.

    [SerializeField] private GameObject ClearUI;
    [SerializeField] private GameObject GameOverUI;


    [Header("InGame-Point")]
    [SerializeField] private GameObject _startPoint;
    [SerializeField] private GameObject _endPoint;
    [SerializeField] private GameObject _playerPoint;
    private float _startPosition;
    private float _endPosition;
    
    [Header("SliderIcon-Point")]
    [SerializeField] private GameObject _uiStartPoint;
    [SerializeField] private GameObject _uiEndPoint;
    private float _iconStart;
    private float _iconEnd;

    [Header("SliderIcon")]
    [SerializeField] private Image _fillBarImage;
    [SerializeField] private GameObject _playerIcon;
    [SerializeField] private float _playerIconY;
    [SerializeField] private GameObject _trainIcon;
    [SerializeField] private float _trainIconY;

    [Header("GameTimer")]
    [SerializeField] private GameTimer _gameTimer;

    [Header("페이드에 사용할 검은 패널 (Image)")]
    [SerializeField] private CanvasGroup _targetCanvasGroup;
    [SerializeField] private float _duration = 0.5f;

    [Header("입력 액션 레퍼런스")]
    public InputActionReference NavigateActionReference;
    public InputActionReference SpaceActionReference;


    [Header("키보드 자판 아이콘")]
    public Image A;
    public Image D;
    public Image Spacebar;

    [Header("패드 UI 세트 (Image 객체)")]
    public Image GamepadLeft;
    public Image GamepadRight;
    public Image GamepadJump;

    [Header("입력키 그룹")]
    public RectTransform ManualRect;
    public Vector3 ManualOffset;

    [Header("점멸 할 텍스트,점멸방식")]
    [SerializeField] private TextMeshProUGUI[] _blinkingText;
    public LoopType Looptype;

    [Header("Restart 버튼,Exit버튼")]
   

    [Header("다른 스크립트가 참조하는 플레이어")]
    public GameObject Player;

    private float _totalTime;
    private float _remainingTime;

    private void OnEnable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.AddListener(MEventType.StartingCutScene,OnCutSceneStart);
            EventManager.Instance.AddListener(MEventType.TutorialStarted, OnTutorialStart);
            EventManager.Instance.AddListener(MEventType.StageStarted, OnGameStart);
            EventManager.Instance.AddListener(MEventType.StageCleared, OnGameCleared);
            EventManager.Instance.AddListener(MEventType.StageFailed, OnGameFailed);
        }

    }
    private void OnDisable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.RemoveListener(MEventType.StartingCutScene, this);
            EventManager.Instance.RemoveListener(MEventType.TutorialStarted, this);
            EventManager.Instance.RemoveListener(MEventType.StageStarted, this);
            EventManager.Instance.RemoveListener(MEventType.StageCleared, this);
            EventManager.Instance.RemoveListener(MEventType.StageFailed, this);
        }

    }
    private void OnCutSceneStart(MEventType type, Component sender, System.EventArgs args)
    {
        if (_targetCanvasGroup == null) return;

        //_targetCanvasGroup.DOKill();
        _targetCanvasGroup.gameObject.SetActive(true);
        _targetCanvasGroup.alpha =1; //컷씬 시작은 보이게
    }
    private void OnTutorialStart(MEventType type, Component sender, System.EventArgs args)
    {   
        FadeOut();
    }
    private void OnGameStart(MEventType type, Component sender, System.EventArgs args)
    {
       InGameDistance();
       
    }
    private void OnGameCleared(MEventType type, Component sender, System.EventArgs args)
    {
        FadeIn();
    }
    private void OnGameFailed(MEventType type, Component sender, System.EventArgs args)
    {
        FadeIn();
       
    }



    private void Start()
    {

        //if (GameManager.Instance.CurrentState == GameState.Play)
        //    InGameDistance();
        BlinkController();
        SliderDistance();
        _totalTime = _gameTimer._totalTime;
      
    }
    private void Update()
    {   if (GameManager.Instance.CurrentState == GameState.Play)
        {
            PlayerIconTransform();
            TrainIconTransform();
            Fill_Bar();
        }

    }

    public void InGameDistance()
    {
        _startPosition = _playerPoint.transform.localPosition.x;
        _endPosition = _endPoint.transform.localPosition.x;
   

    }
    public void SliderDistance()
    {
        _iconStart = _uiStartPoint.transform.localPosition.x;
        _iconEnd = _uiEndPoint.transform.localPosition.x;
    }
    public float PlayerDistance() {
        return (_iconStart + (_iconEnd - _iconStart) * Mathf.Clamp((_playerPoint.transform.localPosition.x - _startPosition) / (_endPosition - _startPosition), 0, 1));
    }
    public float TrainDistance(float _remainingTime, float _totalTime)
    {
        return (_iconEnd+(_iconStart-_iconEnd) * Mathf.Clamp(_remainingTime / _totalTime, 0, 1));

    }
    public void PlayerIconTransform()
    {
        _playerIcon.transform.localPosition = new Vector2(PlayerDistance(), _playerIconY);
    }
    public void TrainIconTransform()
    {
        
        _trainIcon.transform.localPosition = new Vector2(TrainDistance(_gameTimer.RemainingTime, _totalTime), _trainIconY);
    }
    public void Fill_Bar()
    {
        _fillBarImage.fillAmount=Mathf.Clamp((_playerPoint.transform.localPosition.x - _startPosition) / (_endPosition - _startPosition), 0, 1);
    }

    public void FadeOut(Action onComplete = null)
    {
        if (_targetCanvasGroup == null) return;
        _targetCanvasGroup.gameObject.SetActive(true);
        _targetCanvasGroup.DOFade(0f, _duration).OnComplete(() => {
            _targetCanvasGroup.gameObject.SetActive(false); // 다 어두워지면 클릭 방해 안되게 끔
            onComplete?.Invoke();
        });
    }

    public void FadeIn(Action onComplete = null)
    {
        if (_targetCanvasGroup == null) return;
        _targetCanvasGroup.gameObject.SetActive(true);
        _targetCanvasGroup.alpha = 0; // 시작은 투명하게
        _targetCanvasGroup.DOFade(1f, _duration).OnComplete(() => {
            onComplete?.Invoke();
        });
    }

    public void BlinkController()
    {
        for(int i=0; i<_blinkingText.Length; i++)
            _blinkingText[i].DOFade(0.3f, 1f).SetLoops(-1, Looptype);
    }




    public void GameClearUIActivate()
    {
        ClearUI.SetActive(true);
    }

    public void GameOverUIActivate()
    {
        GameOverUI.SetActive(true);
    }
    
}
