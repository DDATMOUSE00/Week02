using System;
using UnityEngine;
using UnityEngine.UI;
public class Sliderbar_sj : MonoBehaviour
{
    [Header("플레이어 아이콘,이미지")]
    [SerializeField] private Image _fullimage;
    [SerializeField] private Image _playerIcon;
    [SerializeField] private Image _trainIcon;

    [Header("아이콘 y축 위치 조정")]
    [SerializeField] private float _playerIconY;
    [SerializeField] private float _trainIconY;

    [Header("플레이어 기준 남은 거리")]
    private RemainDistance_sj _remainDistance;

    private bool _stageStart = false;
    private float _remain;
    private float _time;



    #region Unity Func
    private void Awake()
    {
        _remainDistance = GetComponent<RemainDistance_sj>();
    }
    private void OnEnable()
    {
        if (EventManager.Instance == null)
            return;

        EventManager.Instance.AddListener(MEventType.StageStarted, OnGameStart);
        EventManager.Instance.AddListener(MEventType.StageFailed, OnGameFail);
    }


    private void OnDisable()
    {
        if(EventManager.Instance == null)
            return;

        EventManager.Instance.RemoveListener(MEventType.StageStarted, this);
        EventManager.Instance.RemoveListener(MEventType.StageFailed, this);
    }

    void Update()
    {
        if (_stageStart == false)
        {
           
            return;
        }
  
        _remain = _remainDistance.RemainDistance();
        //_time = _remainDistance.TrainDistance();

        _trainIcon.transform.localPosition = new Vector2(1 - _time, _trainIconY);

        //_playerIcon.transform.localPosition = new Vector3(_remainDistance.UiPosition(), _playerIconY, 0);

        _fullimage.fillAmount = _remain;
        
    }
    #endregion

    private void OnGameFail(MEventType MEventType, Component Sender, EventArgs args)
    {
        _stageStart = false;
    }

    private void OnGameStart(MEventType MEventType, Component Sender, EventArgs args)
    {
        _stageStart = true;
    }
}