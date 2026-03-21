using DG.Tweening;
using TMPro;
using UnityEngine;

public class ComboUITextBehaviour : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private TMP_Text _tmp;
    [SerializeField] private RectTransform _idleRotationTarget;
    [SerializeField] private RectTransform _impactRotationTarget;
    [SerializeField] private RectTransform _scaleTarget;

    [Header("Display")]
    [SerializeField] private string _suffix = " COMBO";
    [SerializeField] private bool _hideWhenZero = false;
    [SerializeField] private CanvasGroup _cg;

    [Header("Idle Wobble")]
    [SerializeField] private float _idleRotateAngle = 6f;
    [SerializeField] private float _idleMaxDuration = 0.65f;
    [SerializeField] private float _idleMinDuration = 0.18f;
    [SerializeField] private int _maxComboForSpeed = 30;

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
    private Tween _scaleTween;

    private int _currentCombo;
    private float _lastComboRatio;
    private float _punchDirection = 1f;

    private void Awake()
    {
        RectTransform selfRect = transform as RectTransform;

        if (_idleRotationTarget == null)
            _idleRotationTarget = selfRect;

        if (_impactRotationTarget == null)
            _impactRotationTarget = selfRect;

        if (_scaleTarget == null)
            _scaleTarget = selfRect;
    }

    private void OnEnable()
    {
        RefreshText();
        RefreshVisible();
        RestartIdleMotion(_lastComboRatio, true);
    }

    private void OnDisable()
    {
        KillAllTweens();
        ResetVisual();
    }

    // 외부에서 현재 콤보와 비율을 받아 표시를 갱신하고 반응 모션을 재생한다.
    public void AddKill(int currentCombo, float comboRatio)
    {
        _currentCombo = Mathf.Max(0, currentCombo);
        _lastComboRatio = Mathf.Clamp01(comboRatio);

        RefreshText();
        RefreshVisible();
        RestartIdleMotion(_lastComboRatio, false);
        PlayKillReaction(_lastComboRatio);
    }

    // 최대 콤보를 모를 때는 내부 설정값 기준으로 비율을 계산해 사용한다.
    public void AddKill(int currentCombo)
    {
        _currentCombo = Mathf.Max(0, currentCombo);
        _lastComboRatio = GetComboRatioFromCount(_currentCombo);

        RefreshText();
        RefreshVisible();
        RestartIdleMotion(_lastComboRatio, false);
        PlayKillReaction(_lastComboRatio);
    }

    // 콤보를 초기화하고 기본 상태로 되돌린다.
    public void ResetCombo()
    {
        _currentCombo = 0;
        _lastComboRatio = 0f;

        RefreshText();
        RefreshVisible();
        RestartIdleMotion(0f, false);
    }

    // 텍스트를 현재 콤보 값으로 갱신한다.
    private void RefreshText()
    {
        if (_tmp == null)
            return;

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

    // 콤보에 따라 더 빨라지는 기본 갸우뚱 모션을 재시작한다.
    private void RestartIdleMotion(float comboRatio, bool immediateReset)
    {
        KillTween(ref _idleTween);

        if (_idleRotationTarget == null)
            return;

        if (immediateReset)
            _idleRotationTarget.localRotation = Quaternion.identity;

        float duration = Mathf.Lerp(_idleMaxDuration, _idleMinDuration, comboRatio);
        duration = Mathf.Max(0.05f, duration);

        Sequence idleSequence = DOTween.Sequence();
        idleSequence.Append(_idleRotationTarget.DOLocalRotate(
            new Vector3(0f, 0f, -_idleRotateAngle),
            duration * 0.5f).SetEase(Ease.InOutSine));
        idleSequence.Append(_idleRotationTarget.DOLocalRotate(
            new Vector3(0f, 0f, _idleRotateAngle),
            duration).SetEase(Ease.InOutSine));
        idleSequence.Append(_idleRotationTarget.DOLocalRotate(
            Vector3.zero,
            duration * 0.5f).SetEase(Ease.InOutSine));
        idleSequence.SetLoops(-1, LoopType.Restart);

        _idleTween = idleSequence;
    }

    // 킬 발생 시 z축 강한 흔들림과 스케일 펀치를 재생한다.
    private void PlayKillReaction(float comboRatio)
    {
        float rotateAmount = _basePunchRotate + (_maxExtraPunchRotate * comboRatio);
        float scaleAmount = _basePunchScale + (_maxExtraPunchScale * comboRatio);

        _punchDirection *= -1f;

        KillTween(ref _impactRotationTween);
        KillTween(ref _scaleTween);

        if (_impactRotationTarget != null)
        {
            _impactRotationTween = _impactRotationTarget.DOPunchRotation(
                new Vector3(0f, 0f, rotateAmount * _punchDirection),
                _punchRotateDuration,
                _punchRotateVibrato,
                0.85f);
        }

        if (_scaleTarget != null)
        {
            _scaleTween = _scaleTarget.DOPunchScale(
                Vector3.one * scaleAmount,
                _punchScaleDuration,
                _punchScaleVibrato,
                0.9f);
        }
    }

    // 콤보 수만으로 0~1 비율을 계산한다.
    private float GetComboRatioFromCount(int comboCount)
    {
        if (_maxComboForSpeed <= 0)
            return 0f;

        return Mathf.Clamp01((float)comboCount / _maxComboForSpeed);
    }

    // 모든 트윈을 안전하게 종료한다.
    private void KillAllTweens()
    {
        KillTween(ref _idleTween);
        KillTween(ref _impactRotationTween);
        KillTween(ref _scaleTween);
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
        if (_idleRotationTarget != null)
            _idleRotationTarget.localRotation = Quaternion.identity;

        if (_impactRotationTarget != null)
            _impactRotationTarget.localRotation = Quaternion.identity;

        if (_scaleTarget != null)
            _scaleTarget.localScale = Vector3.one;
    }
}