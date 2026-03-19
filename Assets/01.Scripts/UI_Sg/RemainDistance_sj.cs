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

    private float StartPosition;
    private float EndPosition;
    private float NowPosition;
    private float IconPosition;

    //private float _totalLenth;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {




    }

// Update is called once per frame
void Update()
    {
    }
    public float UiPosition()
    {

      
        IconPosition = (_iconEnd - _iconStart ) * Mathf.Clamp((NowPosition - StartPosition) / (EndPosition - StartPosition), 0, 1);
        return (_iconStart + IconPosition) ;
    }



    public float RemainDistance()
    {
        StartPosition = _startpoint.transform.position.x;
        EndPosition = _endpoint.transform.position.x;
        NowPosition = _playerpoint.transform.position.x;
        //Debug.Log("nowposition" + NowPosition);

        float a = Mathf.Clamp((NowPosition - StartPosition) / (EndPosition - StartPosition), 0, 1);
       
        //Debug.Log("(NowPosition - StartPosition)"+(NowPosition - StartPosition));
        //Debug.Log("(EndPosition - StartPosition)"+(EndPosition - StartPosition));
        //Debug.Log("a"+a);
        return a;
    }

    //public float PlayerIconPosition()
    //{
    //    StartPosition = GameManager_sj.Instance.StartPoint.transform.position.x;
    //    EndPosition = GameManager_sj.Instance.EndPoint.transform.position.x;
    //    NowPosition = GameManager_sj.Instance.PlayerPoint.transform.position.x;

    //    Debug.Log("StartPosition : " + StartPosition);
    //    Debug.Log("EndPosition : " + EndPosition);
    //    Debug.Log("nowPosition : " + NowPosition);
    //    float a = Mathf.Clamp((StartPosition - EndPosition) * ((NowPosition - StartPosition) / (EndPosition - StartPosition)), 0, 1);
    //    return a;
    //}
}
