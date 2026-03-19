using UnityEngine;

public class GameManager : Singleton<GameManager>
{

    public override void Init()
    {}

    [Header("Game Flow")]

    public GameState CurrentState = GameState.Lobby;

    [SerializeField] private float traintime = 300f;

    [SerializeField] private Transform StartingPoint;
    [SerializeField] private Transform FinishPoint;

    private bool _isStageEnded = false;




    [Header("GameScript")]
    [SerializeField] private TrainTimer trainTimer;






    private void OnEnable()
    {
        EventManager.Instance.AddListener(MEventType.StageCleared, OnStageCleared);
        EventManager.Instance.AddListener(MEventType.StageFailed, OnStageFailed);
        EventManager.Instance.AddListener(MEventType.TrainTimeOver, OnTrainTimeOver);


    }

    private void OnDisable()
    {
        if( EventManager.Instance != null)
        EventManager.Instance.RemoveListener(MEventType.StageCleared, this);
        EventManager.Instance.RemoveListener(MEventType.StageFailed, this);
        EventManager.Instance.RemoveListener(MEventType.TrainTimeOver, this);
    }

    
    private void OnStageCleared(MEventType type, Component sender, System.EventArgs args)
    {
        if (_isStageEnded) return;
        _isStageEnded = true;

        ChangeState(GameState.Clear);
        //게임승리 발동 매커니즘
    }
    private void OnTrainTimeOver(MEventType type, Component sender, System.EventArgs args)
    {
        EventManager.Instance.PostNotification(MEventType.StageFailed, this);

    }
    private void OnStageFailed(MEventType type, Component sender, System.EventArgs args)
    {

        if (_isStageEnded) return;
        _isStageEnded = true;
        
        ChangeState(GameState.GameOver);

        //게임 오버시 발동 매커니즘
    }
    

    private void ChangeState(GameState nextstate)
    {
        if (CurrentState == nextstate) return;

        GameState prev = CurrentState;
        CurrentState = nextstate;
        if (nextstate == GameState.Play) 
        {
            trainTimer.StartTimer(traintime);
            Debug.LogError("게임시작");
            //Player MoveLock false

        }
        if (nextstate == GameState.Clear || nextstate == GameState.GameOver) 
        {
            trainTimer.StopTimer();
            Debug.LogError("GameStop");
            Debug.LogError("몬스터 스폰 정지");// 이건 EnemySpawner에서 이벤트 구독해서 구현해야함. 여기서 구현하는게 아님.
            //Player MoveLock true
        }
        if(nextstate == GameState.Ready)
        {
            Debug.LogError("시작컷씬 실행");
            //Player MoveLock true

        }

        EventManager.Instance.PostNotification( MEventType.GameStateChanged, this, new GameStateChangedEventArgs(prev, CurrentState));
    }
    public void DebugSetState(GameState nextstate)
    {
        ChangeState(nextstate);
    }
        
}