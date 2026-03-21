using DG.Tweening;
using TMPro;
using UnityEngine;

public class ComboUITextBehaviour : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private TMP_Text _tmp;
    [SerializeField] private RectTransform _remainScaleTarget;
    [SerializeField] private RectTransform _idleRotationTarget;
    [SerializeField] private RectTransform _impactRotationTarget;
    [SerializeField] private RectTransform _punchScaleTarget;
    [SerializeField] private ComboTextRainbowGradient _rainbowGradient;
    [SerializeField] private CanvasGroup _cg;

    [Header("Display")]
    [SerializeField] private string _suffix = " COMBO";
    [SerializeField] private bool _hideWhenZero = false;

    [Header("Idle Wobble")]
    [SerializeField] private float _idleRotateAngle = 6f;
    [SerializeField] private float _idleMaxDuration = 0.65f;
    [SerializeField] private float _idleMinDuration = 0.18f;

    [Header("Remain Scale")]
    [SerializeField] private float _fullRemainScale = 1f;
    [SerializeField] private float _minRemainScale = 0.72f;
    [SerializeField] private AnimationCurve _remainScaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Kill Reaction")]
    [SerializeField] private float _basePunchRotate = 12f;
    [SerializeField] private float _maxExtraPunchRotate = 16f;
    [SerializeField] private float _punchRotateDuration = 0.28f;
    [SerializeField] private int _punchRotateVibrato = 10;
    [SerializeField] private float _basePunchScale = 0.18f;
    [SerializeField] private float _maxExtraPunchScale = 0.22f;
    [SerializeField] private float _punchScaleDuration = 0.30f;
    [SerializeField] private int _punchScaleVibrato = 8;

    private Tween _idleTween;
    private Tween _impactRotationTween;
    private Tween _punchScaleTween;

    private int _currentCombo;
    private float _lastComboRatio;
    private float _lastRemainRatio;
    private float _idlePhase;
    private float _punchDirection = 1f;

    private void Awake()
    {
        RectTransform selfRect = transform as RectTransform;

        if (_remainScaleTarget == null)
            _remainScaleTarget = selfRect;

        if (_idleRotationTarget == null)
            _idleRotationTarget = selfRect;

        if (_impactRotationTarget == null)
            _impactRotationTarget = selfRect;

        if (_punchScaleTarget == null)
            _punchScaleTarget = selfRect;
    }

    private void OnEnable()
    {
        RefreshText();
        RefreshVisible();
        ApplyRemainScale();
        RestartIdleMotion(true);
        RainbowEnable();
    }

    private void OnDisable()
    {
        KillAllTweens();
        ResetVisual();
    }

    // 외부에서 킬 누적 시 콤보 UI를 갱신하고 반응 모션을 재생한다.
    public void AddKill(int currentCombo, float comboRatio, float remainRatio = 1f)
    {
        _currentCombo = Mathf.Max(0, currentCombo);
        _lastComboRatio = Mathf.Clamp01(comboRatio);
        _lastRemainRatio = Mathf.Clamp01(remainRatio);

        RefreshText();
        RefreshVisible();
        ApplyRemainScale();
        RestartIdleMotion(false);
        PlayKillReaction();
        RainbowEnable();
    }

    // 외부에서 남은 유지 시간 비율을 계속 넘겨주면 기본 스케일을 갱신한다.
    public void SetRemainRatio(float remainRatio)
    {
        _lastRemainRatio = Mathf.Clamp01(remainRatio);
        ApplyRemainScale();
    }

    // 콤보를 초기화하고 UI를 기본 상태로 되돌린다.
    public void ResetCombo()
    {
        _currentCombo = 0;
        _lastComboRatio = 0f;
        _lastRemainRatio = 0f;

        RefreshText();
        RefreshVisible();
        ApplyRemainScale();
        RestartIdleMotion(false);
        RainbowEnable();
    }

    // 무지개 효과 활성 여부를 갱신한다.
    private void RainbowEnable()
    {
        if (_rainbowGradient == null)
            return;

        _rainbowGradient.enabled = _lastComboRatio >= 0.999f;
    }

    // 텍스트를 현재 콤보 값으로 갱신한다.
    private void RefreshText()
    {
        if (_tmp == null)
            return;

        if (_lastComboRatio >= 1f)
            _tmp.text = $"MAX!{_suffix}";
        else
            _tmp.text = $"{_currentCombo}{_suffix}";
    }

    // 0콤보일 때 숨김 옵션을 반영한다.
    private void RefreshVisible()
    {
        if (_cg == null)
            return;

        if (_hideWhenZero && _currentCombo <= 0)
            _cg.alpha = 0f;
        else
            _cg.alpha = 1f;
    }

    // 남은 시간 비율에 따라 기본 스케일을 갱신한다.
    private void ApplyRemainScale()
    {
        if (_remainScaleTarget == null)
            return;

        float evaluatedRatio = EvaluateRemainScaleRatio(_lastRemainRatio);
        float scale = Mathf.Lerp(_minRemainScale, _fullRemainScale, evaluatedRatio);
        _remainScaleTarget.localScale = Vector3.one * scale;
    }

    // 콤보가 높을수록 빨라지는 기본 갸우뚱 모션을 재시작한다.
    private void RestartIdleMotion(bool immediateReset)
    {
        KillTween(ref _idleTween);

        if (_idleRotationTarget == null)
            return;

        if (immediateReset)
        {
            _idlePhase = 0f;
            _idleRotationTarget.localRotation = Quaternion.identity;
        }

        float duration = Mathf.Lerp(_idleMaxDuration, _idleMinDuration, _lastComboRatio);
        duration = Mathf.Max(0.05f, duration);

        float startPhase = _idlePhase;
        float endPhase = startPhase + (Mathf.PI * 2f);

        _idleTween = DOVirtual.Float(startPhase, endPhase, duration, value =>
        {
            _idlePhase = Mathf.Repeat(value, Mathf.PI * 2f);
            float angle = Mathf.Sin(_idlePhase) * _idleRotateAngle;
            _idleRotationTarget.localRotation = Quaternion.Euler(0f, 0f, angle);
        })
        .SetEase(Ease.Linear)
        .SetLoops(-1, LoopType.Incremental);
    }

    // 킬 발생 시 z축 강한 흔들림과 스케일 펀치를 재생한다.
    private void PlayKillReaction()
    {
        float rotateAmount = _basePunchRotate + (_maxExtraPunchRotate * _lastComboRatio);
        float scaleAmount = _basePunchScale + (_maxExtraPunchScale * _lastComboRatio);

        _punchDirection *= -1f;

        KillTween(ref _impactRotationTween);
        KillTween(ref _punchScaleTween);

        if (_impactRotationTarget != null)
        {
            _impactRotationTween = _impactRotationTarget.DOPunchRotation(
                new Vector3(0f, 0f, rotateAmount * _punchDirection),
                _punchRotateDuration,
                _punchRotateVibrato,
                0.85f);
        }

        if (_punchScaleTarget != null)
        {
            _punchScaleTween = _punchScaleTarget.DOPunchScale(
                Vector3.one * scaleAmount,
                _punchScaleDuration,
                _punchScaleVibrato,
                0.9f);
        }
    }

    // 남은 시간 비율에 curve를 적용한다.
    private float EvaluateRemainScaleRatio(float remainRatio)
    {
        float clampedRatio = Mathf.Clamp01(remainRatio);

        if (_remainScaleCurve == null || _remainScaleCurve.length == 0)
            return clampedRatio;

        return Mathf.Clamp01(_remainScaleCurve.Evaluate(clampedRatio));
    }

    // 모든 트윈을 안전하게 종료한다.
    private void KillAllTweens()
    {
        KillTween(ref _idleTween);
        KillTween(ref _impactRotationTween);
        KillTween(ref _punchScaleTween);
    }

    // 단일 트윈을 안전하게 종료한다.
    private void KillTween(ref Tween tween)
    {
        if (tween == null)
            return;

        if (tween.IsActive())
            tween.Kill();

        tween = null;
    }

    // 비활성화 시 남는 회전/스케일 값을 기본값으로 되돌린다.
    private void ResetVisual()
    {
        if (_remainScaleTarget != null)
            _remainScaleTarget.localScale = Vector3.one * _fullRemainScale;

        if (_idleRotationTarget != null)
            _idleRotationTarget.localRotation = Quaternion.identity;

        if (_impactRotationTarget != null)
            _impactRotationTarget.localRotation = Quaternion.identity;

        if (_punchScaleTarget != null)
            _punchScaleTarget.localScale = Vector3.one;

        if (_rainbowGradient != null)
            _rainbowGradient.enabled = false;
    }
}