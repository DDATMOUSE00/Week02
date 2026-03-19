using UnityEngine;

public class GameManager : Singleton<GameManager>
{

    public override void Init() { }

    [Header("Game Flow")]
    [SerializeField] private bool startFromOpeningCutscene = true;
    [SerializeField] private float trainArrivalTime = 90f;

    public GameState CurrentState { get; private set; } = GameState.None;


    private void OnEnable()
    {
        EventManager.Instance.AddListener(MEventType.PlayerDied, OnPlayerDied);
        EventManager.Instance.AddListener(MEventType.StageCleared, OnStageCleared);
    }

    private void OnDisable()
    {
        if (EventManager.Instance == null)
            return;
            
        EventManager.Instance.RemoveListener(MEventType.PlayerDied, this);
        EventManager.Instance.RemoveListener(MEventType.StageCleared, this);
    }

    private void OnPlayerDied(MEventType type, Component sender, System.EventArgs args)
    {
        Debug.Log("GameOver");
        ChangeState(GameState.GameOver);

    }

    private void OnStageCleared(MEventType type, Component sender, System.EventArgs args)
    {
        Debug.Log("GameClear");
    }

    
}
