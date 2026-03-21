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
    public void StartCutScene()
    {
        ChangeState(GameState.StartCutScene);
        EventManager.Instance.PostNotification(MEventType.StartCutScene, this);
    }

    public void TutorialStart()
    {
        ChangeState(GameState.Tutorial);
        EventManager.Instance.PostNotification(MEventType.TutorialStarted, this);
    }
        
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
        EventManager.Instance.PostNotification(MEventType.GameStateChanged, this, new GameStateChangedEventArgs(prev, CurrentState));

       
    }
    public void DebugSetState(GameState nextstate)
    {
        ChangeState(nextstate);
    }
        
}