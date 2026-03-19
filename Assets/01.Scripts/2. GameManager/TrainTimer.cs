using UnityEngine;

public class TrainTimer : MonoBehaviour
{

    public float RemainingTime { get; private set; }
    public bool IsRunning  { get; private set; } = false;

    public float totalTime = 300f;

    private void OnEnable()
    {
        EventManager.Instance.AddListener(MEventType.GameStateChanged, OnGameStateChanged);
    }
    private void OnDisable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.RemoveListener(MEventType.GameStateChanged, this);
        }

    }
    private void Update()
    {
        if (IsRunning == false) return;

        RemainingTime -= Time.deltaTime;
        Debug.Log(RemainingTime);
        if (RemainingTime < 0f) 
        {
            RemainingTime = 0f;
        }
        EventManager.Instance.PostNotification( MEventType.TrainTimeChanged, this, new TimerEventArgs(RemainingTime, totalTime));

        if (RemainingTime <= 0f)
        {
            IsRunning = false;
            EventManager.Instance.PostNotification(MEventType.TrainTimeOver, this);
            GameManager.Instance.GameOver();
        }
    }
    private void OnGameStateChanged(MEventType type, Component sender, System.EventArgs args)
    {
        if (args is not GameStateChangedEventArgs e) return;

        if (e.current == GameState.Play)
        {
            StartTimer(totalTime);
        }
        else if (e.current == GameState.Clear || e.current == GameState.GameOver)
        {
            StopTimer();
        }
    }
    public void StartTimer(float seconds)
    {
        totalTime = seconds;
        RemainingTime = seconds;
        IsRunning = true;

        EventManager.Instance.PostNotification( MEventType.TrainTimeChanged, this, new TimerEventArgs(RemainingTime, totalTime));
    }

    public void StopTimer() => IsRunning = false;

   
}
