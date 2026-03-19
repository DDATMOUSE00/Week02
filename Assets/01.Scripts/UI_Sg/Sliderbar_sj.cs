using System;
using UnityEngine;
using UnityEngine.UI;
public class Sliderbar_sj : MonoBehaviour
{
    //[SerializeField] private Slider slider;
    [SerializeField] private Image Fullimage;
    [SerializeField] private Image PlayerIcon;


    [SerializeField] private float PlayerIconY;
    private RemainDistance_sj _remainDistance;
    private bool _stageStart = false;
    private float _remain;




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




        // Debug.Log("remain"+remain);
        PlayerIcon.transform.localPosition =  new Vector3(_remainDistance.UiPosition(),PlayerIconY,0);

        Fullimage.fillAmount = _remain;
        if (Fullimage.fillAmount==0)
        {
            GameManager.Instance.GameOver();
        }
        //Debug.Log("fillAmount" + Fullimage.fillAmount);
        //PlayerIcon.transform.localPosition = playerpo;

        //remain = GameManager.GetComponent<GameManager_sj>().remainDistance();
        //slider.value = remain;
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