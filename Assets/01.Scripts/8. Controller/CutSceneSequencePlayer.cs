// CutSceneSequencePlayer.cs
using System;
using DG.Tweening;
using UnityEngine;

public class CutSceneSequencePlayer : MonoBehaviour
{
    [SerializeField] private CutSceneStep[] _steps;
    [SerializeField] private bool _hideTargetsOnStop = true;

    private int _stepIndex;
    private bool _isRunning;
    private bool _isStepRunning;
    private Sequence _activeSequence;
    private Action _onSequenceFinished;

    public void Begin(Action onSequenceFinished)
    {
        KillActiveSequence();

        _onSequenceFinished = onSequenceFinished;
        _stepIndex = 0;
        _isRunning = true;
        _isStepRunning = false;

        ResetForBegin();
    }

    public void StopImmediate()
    {
        KillActiveSequence();

        _isRunning = false;
        _isStepRunning = false;
        _stepIndex = 0;
        _onSequenceFinished = null;

        if (_hideTargetsOnStop) HideAllTargets();
    }

    public void HandleAdvanceInput()
    {
        if (!_isRunning) return;

        if (_isStepRunning)
        {
            SkipCurrentStep();
            return;
        }

        PlayNextStep();
    }

    private void PlayNextStep()
    {
        if (_steps == null || _stepIndex >= _steps.Length)
        {
            FinishSequence();
            return;
        }

        CutSceneStep step = _steps[_stepIndex];
        _stepIndex++;

        if (step == null) return;

        if (step.StepType == CutSceneStepType.End)
        {
            FinishSequence();
            return;
        }

        if (step.StepType == CutSceneStepType.ActivateObject)
        {
            if (step.TargetObject != null) step.TargetObject.SetActive(true);
            return;
        }

        _activeSequence = CutSceneStepExecutor.Play(step);
        if (_activeSequence == null) return;

        _isStepRunning = true;
        _activeSequence.OnComplete(() =>
        {
            _isStepRunning = false;
            _activeSequence = null;
            CutSceneStepExecutor.ApplyFinalState(step);
        });
    }

    private void SkipCurrentStep()
    {
        if (!_isStepRunning) return;

        if (_activeSequence != null && _activeSequence.IsActive())
        {
            _activeSequence.Complete(true);
            return;
        }

        _isStepRunning = false;
        _activeSequence = null;
    }

    private void FinishSequence()
    {
        KillActiveSequence();

        _isRunning = false;
        _isStepRunning = false;

        Action callback = _onSequenceFinished;
        _onSequenceFinished = null;

        callback?.Invoke();
    }

    private void ResetForBegin()
    {
        if (_steps == null) return;

        for (int i = 0; i < _steps.Length; i++)
        {
            CutSceneStepExecutor.HideForReset(_steps[i]);
        }
    }

    private void HideAllTargets()
    {
        if (_steps == null) return;

        for (int i = 0; i < _steps.Length; i++)
        {
            CutSceneStep step = _steps[i];
            if (step == null) continue;
            if (step.TargetObject != null) step.TargetObject.SetActive(false);
        }
    }

    private void KillActiveSequence()
    {
        if (_activeSequence != null && _activeSequence.IsActive())
        {
            _activeSequence.Kill();
        }

        _activeSequence = null;
    }

    private void OnDisable()
    {
        KillActiveSequence();
        _isRunning = false;
        _isStepRunning = false;
    }
}
