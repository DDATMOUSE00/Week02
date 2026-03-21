using DG.Tweening;
using UnityEngine;

public class TrainMove : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Transform _startingPoint;
    [SerializeField] private Transform _endPoint;
    [SerializeField] private GameTimer _gameTimer;

    [Header("Speed")]
    [SerializeField] private float _rushDurationEach = 1.0f;

    [Header("Point 오프셋")]
    [SerializeField] private float _transformOffset = 10f;

    private Sequence _seq;
    private Tween _finalRunTween;

    private float startX;
    private float endX;


    private void Awake()
    {
        startX = _startingPoint.position.x - _transformOffset;
        endX = _endPoint.position.x - _transformOffset;

        transform.position = new Vector3(startX, transform.position.y, transform.position.z);
    }

    private void OnEnable()
    {
        EventManager.Instance.AddListener(MEventType.StageStarted, OnStageStarted);
        EventManager.Instance.AddListener(MEventType.StageCleared, OnStageGameClear);
        EventManager.Instance.AddListener(MEventType.StageFailed, OnStageGameOver);
    }

    private void OnDisable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.RemoveListener(MEventType.StageStarted, this);
            EventManager.Instance.RemoveListener(MEventType.StageCleared, this);
            EventManager.Instance.RemoveListener(MEventType.StageFailed, this);
        }

        KillTweens();
    }

    private void OnStageStarted(MEventType type, Component sender, System.EventArgs args)
    {
        PlayTrainMove();
    }
    private void OnStageGameOver(MEventType type, Component sender, System.EventArgs args)
    {
        KillTweens();
    }

    private void OnStageGameClear(MEventType type, Component sender, System.EventArgs args)
    {
        KillTweens();
        //게임 클리어시 기차는 어떻게..?
    }

    private void PlayTrainMove()
    {
        KillTweens();

        _seq = DOTween.Sequence();

        _seq.AppendCallback(() =>
        {
            float runDuration = Mathf.Max(0.01f, _gameTimer.RemainingTime);
            _finalRunTween = transform.DOMoveX(endX, runDuration).SetEase(Ease.Linear);
        });
    }

    private void KillTweens()
    {
        if (_seq != null && _seq.IsActive()) _seq.Kill();
        if (_finalRunTween != null && _finalRunTween.IsActive()) _finalRunTween.Kill();
        _seq = null;
        _finalRunTween = null;
    }
}
