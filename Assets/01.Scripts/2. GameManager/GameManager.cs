using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{

    public override void Init()
    {
        CurrentState = GameState.Lobby;
        _isStageEnded = false;
    }

    [Header("Game Flow")]

    public GameState CurrentState = GameState.Lobby;
    private bool _isStageEnded = false;

    
    public void GameStart()
        {
            ChangeState(GameState.Play);
            EventManager.Instance.PostNotification(MEventType.StageStarted, this);
        }
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
            Debug.Log("GameStart");
        }
        if (nextstate == GameState.Clear || nextstate == GameState.GameOver) 
        {
            Debug.Log("GameStop");
            Debug.Log("몬스터 스폰 정지");
            // 이건 EnemySpawner에서 이벤트 구독해서 구현해야함. 여기서 구현하는게 아님.
            //Player MoveLock true
        }
        if(nextstate == GameState.Ready)
        {
            Debug.Log("시작컷씬 실행");
            //Player MoveLock true

        }

        EventManager.Instance.PostNotification( MEventType.GameStateChanged, this, new GameStateChangedEventArgs(prev, CurrentState));
    }
    public void DebugSetState(GameState nextstate)
    {
        ChangeState(nextstate);
    }
        
}