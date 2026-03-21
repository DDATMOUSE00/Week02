using UnityEngine;
using System.Collections;
using TMPro.EditorUtilities;
public class GameTimer : MonoBehaviour
{
    [Header("TimeSetting")]
    [SerializeField] private float _totalTime = 300f;
    [SerializeField] bool _isRunning = false;
    [SerializeField] public float RemainingTime;


    [Header("UI Reference")]

    //[SerializeField] private GameObject _uiStartPoint;
    //[SerializeField] private GameObject _uiEndPoint;

    private float _iconStart;
    private float _iconEnd;

    private Coroutine _timerCoroutine;

    void Awake()
    {
        //_iconStart = _uiStartPoint.transform.localPosition.x;
        //_iconEnd = _uiEndPoint.transform.localPosition.x;
    }
    private void OnEnable()
    {
        if (EventManager.Instance != null)
        EventManager.Instance.AddListener(MEventType.StageStarted, OnTimerStart);
    }
    private void OnDisable()
    {
        if (EventManager.Instance != null)
            EventManager.Instance.RemoveListener(MEventType.StageStarted, this);
        StopTimer();
    }
    
    private void OnTimerStart(MEventType type, Component sender, System.EventArgs args)
    {
        StartTimer(_totalTime);
    }

    //UI Manager 처리과정
    /*
    public float TrainDistance(float _remainingTime, float _totalTime)
    {
        return  _iconStart+(_iconEnd - _iconStart) * Mathf.Clamp(_remainingTime / _totalTime, 0, 1);
    }
    */

   
    public void StartTimer(float seconds)
    {
        _totalTime = seconds;
        RemainingTime = seconds;
        _isRunning = true;

        if (_timerCoroutine != null)
            StopCoroutine(_timerCoroutine);

        _timerCoroutine = StartCoroutine(RunTimer());
    }
    public void StopTimer()
    {
        _isRunning = false;

        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;
        }
    }

    private IEnumerator RunTimer()
    {
        

        while (_isRunning && RemainingTime > 0f)
        {
            RemainingTime -= Time.deltaTime;
            if (RemainingTime <= 0f)
            {
                _isRunning = false;
                GameManager.Instance.GameOver();
            }
            yield return null;
        }


        _timerCoroutine = null;
    }

    
}
