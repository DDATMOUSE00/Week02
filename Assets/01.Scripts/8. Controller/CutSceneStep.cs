using System;
using DG.Tweening;
using UnityEngine;

public enum CutSceneStepType
{
    FadeIn,
    FadeInWithMove,
    FadeInWithScale,
    ActivateObject,
    PageFadeIn,
    End
}

[Serializable]
public class CutSceneStep
{
    public CutSceneStepType StepType = CutSceneStepType.FadeIn;

    [Header("Target")]
    public GameObject TargetObject;
    public CanvasGroup CanvasGroup;

    [Header("Fade")]
    public float FadeDuration = 0.45f;
    public Ease FadeEase = Ease.OutCubic;

    [Header("Move")]
    public RectTransform MoveTarget;
    public Vector2 MoveFrom;
    public Vector2 MoveTo;
    public float MoveDuration = 0.45f;
    public Ease MoveEase = Ease.OutCubic;

    [Header("Scale")]
    public RectTransform ScaleTarget;
    public Vector3 ScaleFrom = new Vector3(1f, 1f, 1f);
    public Vector3 ScaleTo = new Vector3(1.06f, 1.06f, 1f);
    public float ScaleDuration = 0.45f;
    public Ease ScaleEase = Ease.OutCubic;

    public GameObject[] HideObjects; 
    public GameObject ShowObject;    
    public CanvasGroup ShowCanvasGroup;

    
}
