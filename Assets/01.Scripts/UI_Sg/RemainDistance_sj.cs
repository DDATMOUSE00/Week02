using DG.Tweening.Core.Easing;
using System.Net;
using UnityEngine;

public class RemainDistance_sj : MonoBehaviour
{
    [Header("게임의 시작과 끝 좌표")]
    [SerializeField] private GameObject _startpoint;
    [SerializeField] private GameObject _endpoint;
    [SerializeField] private GameObject _playerpoint;
    

    [Header("상단 UI에 필요한 좌표")]
    [SerializeField] private float _iconStart;
    [SerializeField] private float _iconEnd;
    [SerializeField] private GameObject UiStartPoint;
    [SerializeField] private GameObject UiEndPoint;

    [SerializeField] private float StartPosition;
    [SerializeField] private float EndPosition;
    [SerializeField] private float NowPosition;
    [SerializeField] private float IconPosition;

    [SerializeField] private float RemainTime;
    [SerializeField] private float TotalTime;

    //private float _totalLenth;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _iconStart = UiStartPoint.transform.localPosition.x;
        _iconEnd = UiEndPoint.transform.localPosition.x;
        //Debug.Log(_iconStart);
        //Debug.Log(_iconEnd);
    }

    public float UiPosition()
    {      
        IconPosition = (_iconEnd - _iconStart ) * Mathf.Clamp((NowPosition - StartPosition) / (EndPosition - StartPosition), 0, 1);
        return (_iconStart + IconPosition) ;
    }


    public float TrainDistance()
    {
        RemainTime = GetComponent<TrainTimer>().RemainingTime;
        TotalTime = GetComponent<TrainTimer>().totalTime;
        
        return  _iconStart+(_iconEnd - _iconStart) * Mathf.Clamp(RemainTime / TotalTime, 0, 1);
    }
    public float RemainDistance()
    {
        StartPosition = _startpoint.transform.localPosition.x;
        EndPosition = _endpoint.transform.localPosition.x;
        NowPosition = _playerpoint.transform.localPosition.x;

        float a = Mathf.Clamp((NowPosition - StartPosition) / (EndPosition - StartPosition), 0, 1);

        return a;
    }
}
