// CutSceneController.cs
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CutSceneController : MonoBehaviour
{
    private enum CutSceneType { None, Start, EndingA, EndingB, EndingC }

    [Header("Shared")]
    [SerializeField] private GameObject _background;
    [SerializeField] private GameScoreController _gameScoreController;

    [Header("Sequences")]
    [SerializeField] private CutSceneSequencePlayer _startSequence;
    [SerializeField] private CutSceneSequencePlayer _endingASequence;
    [SerializeField] private CutSceneSequencePlayer _endingBSequence;
    [SerializeField] private CutSceneSequencePlayer _endingCSequence;

    private CutSceneType _currentType = CutSceneType.None;
    private CutSceneSequencePlayer _currentSequence;
    private bool _isPlaying;

    private void OnEnable()
    {
        if (EventManager.Instance == null) return;

        EventManager.Instance.AddListener(MEventType.StartingCutScene, OnStartingCutScene);
        EventManager.Instance.AddListener(MEventType.StageCleared, OnStageCleared);
        EventManager.Instance.AddListener(MEventType.StageFailed, OnStageFailed);
    }

    private void OnDisable()
    {
        if (EventManager.Instance == null) return;

        EventManager.Instance.RemoveListener(MEventType.StartingCutScene, this);
        EventManager.Instance.RemoveListener(MEventType.StageCleared, this);
        EventManager.Instance.RemoveListener(MEventType.StageFailed, this);
    }

    private void Update()
    {
        if (!_isPlaying) return;
        if (!IsAdvancePressedThisFrame()) return;

        if (_currentSequence == null)
        {
            EndCutScene();
            return;
        }

        _currentSequence.HandleAdvanceInput();
    }

    private void OnStartingCutScene(MEventType t, Component s, EventArgs a)
    {
        StartCutScene(CutSceneType.Start);
    }

    private void OnStageCleared(MEventType t, Component s, EventArgs a)
    {
        bool isEndingC = _gameScoreController != null && _gameScoreController.BreadScore >= 3;
        StartCutScene(isEndingC ? CutSceneType.EndingC : CutSceneType.EndingA);
    }

    private void OnStageFailed(MEventType t, Component s, EventArgs a)
    {
        StartCutScene(CutSceneType.EndingB);
    }

    private void StartCutScene(CutSceneType type)
    {
        StopAllSequences();

        _currentType = type;
        _currentSequence = GetSequence(type);
        _isPlaying = true;

        if (_background != null) _background.SetActive(true);

        if (_currentSequence == null)
        {
            EndCutScene();
            return;
        }

        _currentSequence.Begin(OnSequenceFinished);
    }

    private void OnSequenceFinished()
    {
        EndCutScene();
    }

    private void EndCutScene()
    {
        _isPlaying = false;

        if (_currentSequence != null)
        {
            _currentSequence.StopImmediate();
            _currentSequence = null;
        }

        if (_background != null) _background.SetActive(false);

        if (GameManager.Instance != null)
        {
            if (_currentType == CutSceneType.Start) GameManager.Instance.TutorialStart();
            else if (_currentType == CutSceneType.EndingA) GameManager.Instance.ClearCutSceneFinished();
            else if (_currentType == CutSceneType.EndingB) GameManager.Instance.GameOverCutSceneFinished();
            else if (_currentType == CutSceneType.EndingC) GameManager.Instance.ClearCutSceneFinished();
        }

        _currentType = CutSceneType.None;
    }

    private CutSceneSequencePlayer GetSequence(CutSceneType type)
    {
        if (type == CutSceneType.Start) return _startSequence;
        if (type == CutSceneType.EndingA) return _endingASequence;
        if (type == CutSceneType.EndingB) return _endingBSequence;
        if (type == CutSceneType.EndingC) return _endingCSequence;
        return null;
    }

    private void StopAllSequences()
    {
        if (_startSequence != null) _startSequence.StopImmediate();
        if (_endingASequence != null) _endingASequence.StopImmediate();
        if (_endingBSequence != null) _endingBSequence.StopImmediate();
        if (_endingCSequence != null) _endingCSequence.StopImmediate();
    }

    private bool IsAdvancePressedThisFrame()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) return true;
        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame) return true;
        return false;
    }
}
