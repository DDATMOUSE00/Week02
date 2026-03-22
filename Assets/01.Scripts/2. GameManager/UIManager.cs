using UnityEngine;
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



    [SerializeField] private GameTimer _gameTimer;

    private float _totalTime;
    private float _remainingTime;

    private void OnEnable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.AddListener(MEventType.StageStarted, OnSliderStart);
            EventManager.Instance.AddListener(MEventType.StageCleared, OnSliderStop);
        }

    }
    private void OnDisable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.RemoveListener(MEventType.StageStarted, this);
            EventManager.Instance.RemoveListener(MEventType.StageCleared, this);
        }

    }
    private void OnSliderStart(MEventType type, Component sender, System.EventArgs args)
    {
       InGameDistance();
    }
    private void OnSliderStop(MEventType type, Component sender, System.EventArgs args)
    {
        
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

    public void GameClearUIActivate()
    {
        ClearUI.SetActive(true);
    }

    public void GameOverUIActivate()
    {
        GameOverUI.SetActive(true);
    }
    
}
