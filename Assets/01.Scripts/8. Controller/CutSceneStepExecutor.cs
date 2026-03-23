// CutSceneStepExecutor.cs
using DG.Tweening;
using UnityEngine;

public static class CutSceneStepExecutor
{
    public static Sequence Play(CutSceneStep step)
    {
        if (step == null) return null;

        if (step.StepType == CutSceneStepType.FadeIn) return PlayFadeIn(step);
        if (step.StepType == CutSceneStepType.FadeInWithMove) return PlayFadeInWithMove(step);
        if (step.StepType == CutSceneStepType.FadeInWithScale) return PlayFadeInWithScale(step);
        if (step.StepType == CutSceneStepType.PageFadeIn) return PlayPageFadeIn(step);

        return null;
    }

    public static void ApplyFinalState(CutSceneStep step)
    {
        if (step == null) return;

        CanvasGroup cg = ResolveCanvasGroup(step);
        if (cg != null) cg.alpha = 1f;

        if (step.StepType == CutSceneStepType.FadeInWithMove)
        {
            RectTransform moveTarget = ResolveMoveTarget(step);
            if (moveTarget != null) moveTarget.anchoredPosition = step.MoveTo;
        }
        else if (step.StepType == CutSceneStepType.FadeInWithScale)
        {
            RectTransform scaleTarget = ResolveScaleTarget(step);
            if (scaleTarget != null) scaleTarget.localScale = step.ScaleTo;
        }

        if (step.StepType == CutSceneStepType.PageFadeIn && step.ShowCanvasGroup != null)
        step.ShowCanvasGroup.alpha = 1f;
    }

    public static void HideForReset(CutSceneStep step)
    {
        if (step == null) return;

        GameObject target = ResolveTargetObject(step);
        if (target != null) target.SetActive(false);

        CanvasGroup cg = ResolveCanvasGroup(step);
        if (cg != null) cg.alpha = 0f;

        if (step.StepType == CutSceneStepType.FadeInWithMove)
        {
            RectTransform moveTarget = ResolveMoveTarget(step);
            if (moveTarget != null) moveTarget.anchoredPosition = step.MoveFrom;
        }
        else if (step.StepType == CutSceneStepType.FadeInWithScale)
        {
            RectTransform scaleTarget = ResolveScaleTarget(step);
            if (scaleTarget != null) scaleTarget.localScale = step.ScaleFrom;
        }
    }

    private static Sequence PlayFadeIn(CutSceneStep step)
    {
        GameObject target = ResolveTargetObject(step);
        if (target != null) target.SetActive(true);

        CanvasGroup cg = ResolveCanvasGroup(step);
        if (cg == null) return null;

        cg.alpha = 0f;

        Sequence seq = DOTween.Sequence();
        seq.Append(cg.DOFade(1f, step.FadeDuration).SetEase(step.FadeEase));
        return seq;
    }

    private static Sequence PlayFadeInWithMove(CutSceneStep step)
    {
        GameObject target = ResolveTargetObject(step);
        if (target != null) target.SetActive(true);

        CanvasGroup cg = ResolveCanvasGroup(step);
        RectTransform moveTarget = ResolveMoveTarget(step);
        if (cg == null || moveTarget == null) return null;

        cg.alpha = 0f;
        moveTarget.anchoredPosition = step.MoveFrom;

        Sequence seq = DOTween.Sequence();
        seq.Join(cg.DOFade(1f, step.FadeDuration).SetEase(step.FadeEase));
        seq.Join(moveTarget.DOAnchorPos(step.MoveTo, step.MoveDuration).SetEase(step.MoveEase));
        return seq;
    }

    private static Sequence PlayFadeInWithScale(CutSceneStep step)
    {
        GameObject target = ResolveTargetObject(step);
        if (target != null) target.SetActive(true);

        CanvasGroup cg = ResolveCanvasGroup(step);
        RectTransform scaleTarget = ResolveScaleTarget(step);
        if (cg == null || scaleTarget == null) return null;

        cg.alpha = 0f;
        scaleTarget.localScale = step.ScaleFrom;

        Sequence seq = DOTween.Sequence();
        seq.Join(cg.DOFade(1f, step.FadeDuration).SetEase(step.FadeEase));
        seq.Join(scaleTarget.DOScale(step.ScaleTo, step.ScaleDuration).SetEase(step.ScaleEase));
        return seq;
    }

    private static GameObject ResolveTargetObject(CutSceneStep step)
    {
        if (step.TargetObject != null) return step.TargetObject;
        if (step.CanvasGroup != null) return step.CanvasGroup.gameObject;
        if (step.MoveTarget != null) return step.MoveTarget.gameObject;
        if (step.ScaleTarget != null) return step.ScaleTarget.gameObject;
        return null;
    }

    private static CanvasGroup ResolveCanvasGroup(CutSceneStep step)
    {
        if (step.CanvasGroup != null) return step.CanvasGroup;

        GameObject target = ResolveTargetObject(step);
        if (target == null) return null;

        CanvasGroup cg = target.GetComponent<CanvasGroup>();
        if (cg == null) cg = target.AddComponent<CanvasGroup>();
        return cg;
    }

    private static RectTransform ResolveMoveTarget(CutSceneStep step)
    {
        if (step.MoveTarget != null) return step.MoveTarget;

        GameObject target = ResolveTargetObject(step);
        if (target == null) return null;
        return target.GetComponent<RectTransform>();
    }

    private static RectTransform ResolveScaleTarget(CutSceneStep step)
    {
        if (step.ScaleTarget != null) return step.ScaleTarget;

        GameObject target = ResolveTargetObject(step);
        if (target == null) return null;
        return target.GetComponent<RectTransform>();
    }

    private static Sequence PlayPageFadeIn(CutSceneStep step)
    {
        if (step.HideObjects != null)
        {
            for (int i = 0; i < step.HideObjects.Length; i++)
                if (step.HideObjects[i] != null) step.HideObjects[i].SetActive(false);
        }

        if (step.ShowObject != null) step.ShowObject.SetActive(true);

        CanvasGroup cg = step.ShowCanvasGroup;
        if (cg == null && step.ShowObject != null)
        {
            cg = step.ShowObject.GetComponent<CanvasGroup>();
            if (cg == null) cg = step.ShowObject.AddComponent<CanvasGroup>();
        }
        if (cg == null) return null;

        cg.alpha = 0f;

        Sequence seq = DOTween.Sequence();
        seq.Append(cg.DOFade(1f, step.FadeDuration).SetEase(step.FadeEase));
        return seq;
    }
}
