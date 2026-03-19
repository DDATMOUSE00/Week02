using System;
using UnityEngine;
using UnityEngine.UI;
public class Sliderbar_sj : MonoBehaviour
{
    //private인데 대문자 쓰면 안됨
    [SerializeField] private Image Fullimage;
    [SerializeField] private Image PlayerIcon;
    [SerializeField] private Image TrainIcon;

    //마찬가지
    [SerializeField] private float PlayerIconY;
    [SerializeField] private float TrainIconY;

    [SerializeField] private RemainDistance_sj _remainDistance;
    [SerializeField] private bool _stageStart = false;
    [SerializeField] private float _remain;
    [SerializeField] private float _time;



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

    // Update is called once per frame
    void Update()
    {
        if (_stageStart == false)
        {
           
            return;
        }
  
        _remain = _remainDistance.RemainDistance();
        _time = _remainDistance.TrainDistance();

        TrainIcon.transform.localPosition = new Vector3(1 - _time, TrainIconY, 0);

        // Debug.Log("remain"+remain);
        PlayerIcon.transform.localPosition = new Vector3(_remainDistance.UiPosition(), PlayerIconY, 0);

        Fullimage.fillAmount = _remain;
        //if (Fullimage.fillAmount==0)
        //{
        //    GameManager.Instance.GameOver();
        //}
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