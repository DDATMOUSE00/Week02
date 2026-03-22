using DG.Tweening.Core.Easing;
using System.Net;
using UnityEngine;

public class RemainDistance_sj : MonoBehaviour
{
    [Header("GameStartPoint")]
    [SerializeField] private GameObject _startpoint;
    [SerializeField] private GameObject _endpoint;
    [SerializeField] private GameObject _playerpoint;
    

    [Header("Slider Icon StartPoinot")]
     private float _iconStart;
     private float _iconEnd;
    [SerializeField] private GameObject _uiStartPoint;
    [SerializeField] private GameObject _uiEndPoint;

    private float _startPosition;
    private float _endPosition;
    private float _nowPosition;
    private float _iconPosition;

    private float _remainTime;
    private float _totalTime;

    //private float _totalLenth;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _iconStart = _uiStartPoint.transform.localPosition.x;
        _iconEnd = _uiEndPoint.transform.localPosition.x;
        //Debug.Log(_iconStart);
        //Debug.Log(_iconEnd);
    }

    public float UiPosition()
    {      
        _iconPosition = (_iconEnd - _iconStart ) * Mathf.Clamp((_nowPosition - _startPosition) / (_endPosition - _startPosition), 0, 1);
        return (_iconStart + _iconPosition) ;
    }


    
    public float RemainDistance()
    {
        _startPosition = _startpoint.transform.localPosition.x;
        _endPosition = _endpoint.transform.localPosition.x;
        _nowPosition = _playerpoint.transform.localPosition.x;

        float a = Mathf.Clamp((_nowPosition - _startPosition) / (_endPosition - _startPosition), 0, 1);

        return a;
    }
}
