using DG.Tweening;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerControllerVersionTwo))]
[RequireComponent(typeof(PlayerJumpAction))]
public class PlayerChargeEffect : MonoBehaviour
{
    #region Inspector

    [Header("Reference | Inspector 연결 필요")]
    [SerializeField] private Transform _visualRoot;
    [SerializeField] private Slider _chargeSlider;
    [SerializeField] private CanvasGroup _chargeSliderCanvasGroup;

    [Header("Charge Squash")]
    [SerializeField] private float _yOffest = 0;

    [Tooltip("차지 UI가 보여지기 시작하는 Ratio 값")][SerializeField] private float _showOffest = 0.1f;
    [SerializeField] private float _chargeExitDuration = 0.1f;
    [SerializeField] private float _maxChargeScaleX = 1.12f;
    [SerializeField] private float _maxChargeScaleY = 0.88f;

    [Header("Charge UI")]
    [SerializeField] private float _uiFadeDuration = 0.12f;

    #endregion

    #region Runtime Fields

    private PlayerControllerVersionTwo _controller;
    private PlayerJumpAction _jumpAction;

    private bool _wasCharging;

    private Vector3 _baseScale = Vector3.one;

    private Tween _scaleTween;
    private Tween _uiFadeTween;
    private bool _isShow;

    #endregion

    #region Unity Lifecycle

    // 같은 오브젝트의 필수 컴포넌트를 가져오고, 인스펙터 참조를 검증한다.
    private void Awake()
    {
        _controller = GetComponent<PlayerControllerVersionTwo>();
        _jumpAction = GetComponent<PlayerJumpAction>();

        if (!ValidateRequiredReferences())
        {
            enabled = false;
            return;
        }

        _baseScale = _visualRoot.localScale;
    }

    // 활성화 시 기본 상태를 맞춘다.
    private void OnEnable()
    {
        _wasCharging = false;
        ResetVisualImmediate();
        ResetChargeUIImmediate();
    }

    // 비활성화 시 트윈과 UI를 정리한다.
    private void OnDisable()
    {
        KillTweens();
        ResetVisualImmediate();
        ResetChargeUIImmediate();
    }

    // 차지 상태 변화와 차지 UI, 비주얼 스케일을 갱신한다.
    private void Update()
    {
        bool isCharging = _controller.IsCharging;

        //if (!_wasCharging && isCharging && _jumpAction.ChargeEffectiveRatio == 0)
        //PlayChargeStartEffect();
        
        if (_jumpAction.ChargeEffectiveRatio >= 0.1f)
            PlayChargeStartEffect();

        if (isCharging)
        {
            UpdateChargeScaleByEffectiveRatio();
            UpdateChargeUI();
        }

        if (_wasCharging && !isCharging)
            PlayChargeEndEffect();

        _wasCharging = isCharging;
    }

    #endregion

    #region Setup Validation

    // 인스펙터에서 반드시 연결해야 하는 참조를 검증한다.
    private bool ValidateRequiredReferences()
    {
        bool isValid = true;

        if (_visualRoot == null)
        {
            Debug.LogError($"{nameof(PlayerChargeEffect)}: _visualRoot 가 비어 있습니다.", this);
            isValid = false;
        }

        if (_chargeSlider == null)
        {
            Debug.LogError($"{nameof(PlayerChargeEffect)}: _chargeSlider 가 비어 있습니다.", this);
            isValid = false;
        }

        if (_chargeSliderCanvasGroup == null)
        {
            Debug.LogError($"{nameof(PlayerChargeEffect)}: _chargeSliderCanvasGroup 이 비어 있습니다.", this);
            isValid = false;
        }

        return isValid;
    }

    #endregion

    #region Charge Effect

    // 차지 시작 시 UI를 표시한다.
    private void PlayChargeStartEffect()
    {
        if (_isShow == true)
            return;
        _isShow = true;

        _chargeSliderCanvasGroup.transform.localPosition = new Vector3(
            _chargeSliderCanvasGroup.transform.localPosition.x,
            _chargeSliderCanvasGroup.transform.localPosition.y + _yOffest,
            _chargeSliderCanvasGroup.transform.localPosition.z
            );

        KillScaleTween();
        KillUiTween();

        _uiFadeTween = _chargeSliderCanvasGroup
            .DOFade(1f, _uiFadeDuration)
            .SetEase(Ease.OutQuad);
    }

    // 차지 종료 시 원래 스케일과 UI 상태로 복귀한다.
    private void PlayChargeEndEffect()
    {
        KillScaleTween();
        KillUiTween();

        _scaleTween = _visualRoot
            .DOScale(_baseScale, _chargeExitDuration)
            .SetEase(Ease.OutQuad);

        _uiFadeTween = _chargeSliderCanvasGroup
            .DOFade(0f, _uiFadeDuration)
            .SetEase(Ease.OutQuad);

        _chargeSlider.value = 0f;
        _isShow = false;
    }

    // 실제 힘 비율에 맞춰 x/y 스케일을 실시간으로 찌그러뜨린다.
    private void UpdateChargeScaleByEffectiveRatio()
    {
        KillScaleTween();

        float ratio = _jumpAction.ChargeEffectiveRatio;

        Vector3 targetScale = new Vector3(
            Mathf.Lerp(_baseScale.x, _baseScale.x * _maxChargeScaleX, ratio),
            Mathf.Lerp(_baseScale.y, _baseScale.y * _maxChargeScaleY, ratio),
            _baseScale.z);

        _visualRoot.localScale = targetScale;
    }

    // 현재 차지량에 맞게 슬라이더 값을 갱신한다.
    private void UpdateChargeUI()
    {
        _chargeSlider.value = _jumpAction.ChargeEffectiveRatio;
    }

    #endregion

    #region Utility

    // 비주얼을 즉시 기본 상태로 되돌린다.
    private void ResetVisualImmediate()
    {
        if (_visualRoot != null)
            _visualRoot.localScale = _baseScale;
    }

    // UI를 즉시 숨기고 값을 초기화한다.
    private void ResetChargeUIImmediate()
    {
        if (_chargeSlider != null)
        {
            _chargeSlider.minValue = 0f;
            _chargeSlider.maxValue = 1f;
            _chargeSlider.value = 0f;
        }

        if (_chargeSliderCanvasGroup != null)
            _chargeSliderCanvasGroup.alpha = 0f;
    }

    // 모든 트윈을 정리한다.
    private void KillTweens()
    {
        KillScaleTween();
        KillUiTween();
    }

    // 스케일 트윈을 종료한다.
    private void KillScaleTween()
    {
        if (_scaleTween != null && _scaleTween.IsActive())
            _scaleTween.Kill();

        _scaleTween = null;
    }

    // UI 트윈을 종료한다.
    private void KillUiTween()
    {
        if (_uiFadeTween != null && _uiFadeTween.IsActive())
            _uiFadeTween.Kill();

        _uiFadeTween = null;
    }

    #endregion
}