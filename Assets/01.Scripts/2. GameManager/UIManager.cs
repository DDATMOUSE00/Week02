using UnityEngine;

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

    [SerializeField] private GameObject _playerIcon;
    [SerializeField] private float _playerIconY;


    [SerializeField] private GameObject _trainIcon;
    [SerializeField] private float _trainIconY;



    [SerializeField] private GameTimer _gameTimer;

    private float _totalTime;
    private float _remainingTime;

    private void Start()
    {
        InGameDistance();
        SliderDistance();
        _totalTime = _gameTimer._totalTime;
    }
    private void Update()
    {   PlayerIconTransform();
        TrainIconTransform();
    }

    public void InGameDistance()
    {
        _startPosition = _startPoint.transform.localPosition.x;
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
        return ( 1-(_iconStart+(_iconEnd - _iconStart) * Mathf.Clamp(_remainingTime / _totalTime, 0, 1)));

    }
    public void PlayerIconTransform()
    {
        _playerIcon.transform.localPosition = new Vector2(PlayerDistance(), _trainIconY);
    }
    public void TrainIconTransform()
    {   
        _trainIcon.transform.localPosition = new Vector2(TrainDistance(_gameTimer.RemainingTime, _totalTime), _trainIconY);
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
