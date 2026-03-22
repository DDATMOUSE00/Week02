using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System;

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
    [SerializeField] private Image _targetPanel;
    [SerializeField] private float _duration = 0.5f;


    private float _totalTime;
    private float _remainingTime;

    private void OnEnable()
    {
        if (EventManager.Instance != null)
        {
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
            EventManager.Instance.RemoveListener(MEventType.TutorialStarted, this);
            EventManager.Instance.RemoveListener(MEventType.StageStarted, this);
            EventManager.Instance.RemoveListener(MEventType.StageCleared, this);
        }

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
        if (_targetPanel == null) return;
        _targetPanel.gameObject.SetActive(true);
        _targetPanel.DOFade(0f, _duration).OnComplete(() => {
            _targetPanel.gameObject.SetActive(false); // 다 밝아지면 클릭 방해 안되게 끔
            onComplete?.Invoke();
        });
    }

    public void FadeIn(Action onComplete = null)
    {
        if (_targetPanel == null) return;
        _targetPanel.gameObject.SetActive(true);
        _targetPanel.color = new Color(0, 0, 0, 0); // 시작은 투명하게
        _targetPanel.DOFade(1f, _duration).OnComplete(() => {
            onComplete?.Invoke();
        });
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
