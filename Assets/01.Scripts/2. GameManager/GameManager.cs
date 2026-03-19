using UnityEngine;

public class GameManager : Singleton<GameManager>
{

    public override void Init(){} // Init 에 어떤거?

    [Header("Game Flow")]

    public GameState CurrentState = GameState.Lobby;
    private bool _isStageEnded = false;

    public void GameClear()
    {
        if (_isStageEnded) return;
        _isStageEnded = true;

        ChangeState(GameState.Clear);
        EventManager.Instance.PostNotification(MEventType.StageCleared, this);
    }

    public void GameOver()
    {
        if (_isStageEnded) return;
        _isStageEnded = true;

        ChangeState(GameState.GameOver);
        EventManager.Instance.PostNotification(MEventType.StageFailed, this);

    }

    private void ChangeState(GameState nextstate)
    {
        if (CurrentState == nextstate) return;

        GameState prev = CurrentState;
        CurrentState = nextstate;
        if (nextstate == GameState.Play) 
        {
            Debug.LogError("게임시작");

        }
        if (nextstate == GameState.Clear || nextstate == GameState.GameOver) 
        {
            Debug.LogError("GameStop");
            Debug.LogError("몬스터 스폰 정지");
            // 이건 EnemySpawner에서 이벤트 구독해서 구현해야함. 여기서 구현하는게 아님.
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