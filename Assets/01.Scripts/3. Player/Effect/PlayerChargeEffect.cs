using DG.Tweening;
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

    [Header("Charge UI Follow")]
    [SerializeField] private Transform _followTarget;

    [Header("Charge Squash")]
    [SerializeField] private float _yOffest = 0f;

    [Tooltip("차지 종료 시 원래 스케일로 복귀하는 시간")]
    [SerializeField] private float _chargeExitDuration = 0.1f;
    [SerializeField] private float _maxChargeScaleX = 1.12f;
    [SerializeField] private float _maxChargeScaleY = 0.88f;

    [Header("Charge UI")]
    [SerializeField] private float _uiFadeDuration = 0.12f;

    #endregion

    #region Runtime Fields

    private PlayerControllerVersionTwo _controller;
    private PlayerJumpAction _jumpAction;

    private RectTransform _chargeSliderRect;
    private Canvas _parentCanvas;
    private RectTransform _parentCanvasRect;
    private Camera _uiCamera;

    private bool _wasCharging;
    private bool _isShow;

    private Vector3 _baseScale = Vector3.one;

    private Tween _scaleTween;
    private Tween _uiFadeTween;

    #endregion

    #region Unity Lifecycle

    // 같은 오브젝트의 필수 컴포넌트와 UI 추적에 필요한 참조를 캐싱한다.
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

        _chargeSliderRect = _chargeSliderCanvasGroup.transform as RectTransform;
        _parentCanvas = _chargeSliderCanvasGroup.GetComponentInParent<Canvas>();
        _parentCanvasRect = _parentCanvas != null ? _parentCanvas.GetComponent<RectTransform>() : null;
        _uiCamera = ResolveUiCamera();

        if (_followTarget == null)
            _followTarget = transform;
    }

    // 활성화 시 기본 상태와 UI 상태를 초기화한다.
    private void OnEnable()
    {
        _wasCharging = false;
        ResetVisualImmediate();
        ResetChargeUIImmediate();
    }

    // 비활성화 시 트윈과 UI 상태를 정리한다.
    private void OnDisable()
    {
        KillTweens();
        ResetVisualImmediate();
        ResetChargeUIImmediate();
    }

    // 차지 상태 변화에 맞춰 스케일, 슬라이더 값, 페이드 연출을 갱신한다.
    private void Update()
    {
        bool isCharging = _controller.IsCharging;

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

    // 화면 갱신 직전에 슬라이더를 플레이어 머리 위 좌표로 따라가게 만든다.
    private void LateUpdate()
    {
        if (_chargeSliderCanvasGroup == null)
            return;

        if (_chargeSliderCanvasGroup.alpha <= 0f)
            return;

        UpdateChargeUIFollowPosition();
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

        if (_chargeSliderCanvasGroup != null && _chargeSliderCanvasGroup.transform as RectTransform == null)
        {
            Debug.LogError($"{nameof(PlayerChargeEffect)}: _chargeSliderCanvasGroup 은 UI RectTransform 이어야 합니다.", this);
            isValid = false;
        }

        if (_chargeSliderCanvasGroup != null && _chargeSliderCanvasGroup.GetComponentInParent<Canvas>() == null)
        {
            Debug.LogError($"{nameof(PlayerChargeEffect)}: _chargeSliderCanvasGroup 상위에서 Canvas 를 찾지 못했습니다.", this);
            isValid = false;
        }

        return isValid;
    }

    // 현재 Canvas 설정에 맞는 UI 카메라를 반환한다.
    private Camera ResolveUiCamera()
    {
        if (_parentCanvas == null)
            return Camera.main;

        if (_parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return null;

        if (_parentCanvas.worldCamera != null)
            return _parentCanvas.worldCamera;

        return Camera.main;
    }

    #endregion

    #region Charge Effect

    // 차지 UI를 최초 한 번 표시하고 현재 위치를 즉시 맞춘다.
    private void PlayChargeStartEffect()
    {
        if (_isShow)
            return;

        _isShow = true;

        UpdateChargeUIFollowPosition();

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

    // 슬라이더를 플레이어 머리 위 월드 좌표에 대응되는 UI 위치로 갱신한다.
    private void UpdateChargeUIFollowPosition()
    {
        if (_followTarget == null || _chargeSliderRect == null)
            return;

        Vector3 targetWorldPosition = _followTarget.position;
        targetWorldPosition.y += _yOffest;

        if (_parentCanvas != null && _parentCanvas.renderMode == RenderMode.WorldSpace)
        {
            _chargeSliderRect.position = targetWorldPosition;
            return;
        }

        if (_parentCanvasRect == null)
            return;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(_uiCamera, targetWorldPosition);
        Camera eventCamera = _parentCanvas != null && _parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : _uiCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentCanvasRect, screenPoint, eventCamera, out Vector2 localPoint))
            _chargeSliderRect.anchoredPosition = localPoint;
    }

    #endregion

    #region Utility

    // 비주얼을 즉시 기본 스케일로 되돌린다.
    private void ResetVisualImmediate()
    {
        if (_visualRoot != null)
            _visualRoot.localScale = _baseScale;
    }

    // UI를 즉시 숨기고 슬라이더 값을 초기화한다.
    private void ResetChargeUIImmediate()
    {
        _isShow = false;

        if (_chargeSlider != null)
        {
            _chargeSlider.minValue = 0f;
            _chargeSlider.maxValue = 1f;
            _chargeSlider.value = 0f;
        }

        if (_chargeSliderCanvasGroup != null)
            _chargeSliderCanvasGroup.alpha = 0f;
    }

    // 모든 트윈을 안전하게 종료한다.
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

    // UI 페이드 트윈을 종료한다.
    private void KillUiTween()
    {
        if (_uiFadeTween != null && _uiFadeTween.IsActive())
            _uiFadeTween.Kill();

        _uiFadeTween = null;
    }

    #endregion
}