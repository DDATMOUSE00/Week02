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

    [SerializeField] private PlayerControllerVersionTwo _player;

    void Start()
    {
        CurrentState = GameState.Lobby;
        StartingCutScene();
    }

    
    public void ScoreIncreaseEnemy()    => EventManager.Instance.PostNotification(MEventType.DestroyEnemy, this);
    public void ScoreIncreaseBread()    => EventManager.Instance.PostNotification(MEventType.DestroyBread, this);
    public void ScoreIncreaseBuilding() => EventManager.Instance.PostNotification(MEventType.DestroyBuilding, this);


    public void StartingCutScene()
    {
        ChangeState(GameState.StartCutScene);
        EventManager.Instance.PostNotification(MEventType.StartingCutScene, this);
        //이벤트 구독해서 게임 시작 컷씬 실행

    }

    public void TutorialStart()
    {
        ChangeState(GameState.Tutorial);
        EventManager.Instance.PostNotification(MEventType.TutorialStarted, this);
        TutorialManager.Instance.StartTutorial();
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
        Debug.LogError("GameOver");

        ChangeState(GameState.GameOver);
        EventManager.Instance.PostNotification(MEventType.StageFailed, this);
    }
    public void ClearCutSceneFinished()
    {
        UIManager.Instance.GameClearUIActivate();
    }

    public void GameOverCutSceneFinished()
    {
        UIManager.Instance.GameOverUIActivate();
    }
    private void ChangeState(GameState nextstate)
    {
        if (CurrentState == nextstate) return;

        GameState prev = CurrentState;
        CurrentState = nextstate;
    }
    public void DebugSetState(GameState nextstate)
    {
        ChangeState(nextstate);
    }
    
    public void LockPlayer()
    {
        if (_player != null)
            _player.PlayerInputLock(true);
    }

    public void UnlockPlayer()
    {
        if (_player != null)
            _player.PlayerInputLock(false);
    }
    
    public void RestartGame()
    {
        EventManager.Instance?.PostNotification(MEventType.StageExited, this);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitGame()
    {
        EventManager.Instance?.PostNotification(MEventType.StageExited, this);

    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }
        
}
