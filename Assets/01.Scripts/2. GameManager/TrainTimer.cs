using UnityEngine;

public class TrainTimer : MonoBehaviour
{

    public float RemainingTime { get; private set; }
    public bool IsRunning  { get; private set; } = false;

    private float totalTime;

    private void Awake()
    {
    }

    private void Update()
    {
        if (!IsRunning) return;

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
