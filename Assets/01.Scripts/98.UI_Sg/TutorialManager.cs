using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// PlayerControllerVersionTwo를 중심으로 튜토리얼 상태를 관리한다.
public class TutorialManager : Singleton<TutorialManager>
{
    public enum TutorialStep
    {
        None,
        MoveAD,
        JumpCharge,
        InAir,
        SlamWait,
        Finished
    }

    #region Inspector

    [Header("현재 튜토리얼 단계")]
    [SerializeField] private TutorialStep _currentStep = TutorialStep.None;

    [Header("Reference")]
    [SerializeField] private PlayerControllerVersionTwo _playerController;
    [SerializeField] private Manual_Ui_sg _uiManualUI;

    [Header("Tutorial")]
    [SerializeField, Range(0f, 1f)] private float _requiredChargeNormalized = 0.5f;
    [Space(10)]
    [SerializeField] private float _slowMotionStartScale = 0.1f;
    [SerializeField] private float _slowMotionLerpDuration = 0.15f;
    [Space(10)]
    [SerializeField] private float _peakCheckVelocityY = 0.1f;

    [Header("Hint UI")]
    [SerializeField] private TMP_Text _pressTmp;
    [SerializeField] private TMP_Text _pressHoldTmp;

    [Header("Hint UI Follow")]
    [SerializeField] private Transform _hintFollowTarget;
    [SerializeField] private Vector3 _hintFollowOffset = new Vector3(0f, 2f, 0f);

    [Header("Debug")]
    [SerializeField] private bool _showDebugLog = false;

    public float GameStartPosition;

    private Coroutine _slowMotionRoutine;
    private float _defaultFixedDeltaTime;

    #endregion

    #region Runtime Fields

    private Rigidbody2D _playerRb;
    private PlayerJumpAction _playerJumpAction;
    private Camera _mainCamera;

    private bool _isPeakDetected;
    private bool _canJumpNow;

    #endregion

    #region Properties

    public TutorialStep CurrentStep => _currentStep;

    private bool HasValidPlayerReferences =>
        _playerController != null &&
        _playerRb != null &&
        _playerJumpAction != null;

    #endregion

    #region Initialize / Lifecycle

    // 싱글톤 초기화 시 플레이어 관련 참조와 카메라를 캐싱하고 힌트 텍스트를 갱신한다.
    public override void Init()
    {
        _defaultFixedDeltaTime = Time.fixedDeltaTime;
        _mainCamera = Camera.main;

        if (_playerController != null)
            _playerController.PlayerInputLock(true);

        CacheReferences();
        RestoreTimeScale();
        ResetRuntimeFlags();
        RefreshHintTexts();
        HideAllHintObjects();
    }

    // Awake 시 초기화 루틴을 실행한다.
    protected override void Awake()
    {
        base.Awake();
        Init();
    }

    // 활성화될 때 입력 타입 변경 이벤트를 구독하고 텍스트를 현재 장치 기준으로 갱신한다.
    private void OnEnable()
    {
        SubscribeInputTypeEvent();
        RefreshHintTexts();
    }

    // 비활성화될 때 입력 타입 변경 이벤트를 해제한다.
    private void OnDisable()
    {
        UnsubscribeInputTypeEvent();
    }

    // 파괴될 때 이벤트와 슬로모션이 남지 않도록 복구한다.
    private void OnDestroy()
    {
        UnsubscribeInputTypeEvent();
        RestoreTimeScale();
    }

    // 현재 튜토리얼 단계에 따라 진행 조건을 검사한다.
    private void Update()
    {
        if (!HasValidPlayerReferences)
            return;

        switch (_currentStep)
        {
            case TutorialStep.JumpCharge:
                break;

            case TutorialStep.InAir:
                UpdateInAirStep();
                break;

            case TutorialStep.SlamWait:
                UpdateSlamWaitStep();
                break;
        }
    }

    // 모든 힌트 오브젝트를 플레이어 머리 위 위치로 갱신한다.
    private void LateUpdate()
    {
        UpdateHintFollow();
    }

    #endregion

    #region Public API

    // 튜토리얼을 시작 상태로 초기화하고 첫 단계를 연다.
    public void StartTutorial()
    {
        if (_uiManualUI != null)
            _uiManualUI.gameObject.SetActive(true);

        ResetRuntimeFlags();
        RefreshHintTexts();
        SetStep(TutorialStep.MoveAD);
    }

    // 첫 번째 트리거 진입 시 이동 튜토리얼에서 점프 차지 단계로 전환한다.
    public void OnJumpTrigger()
    {
        if (_currentStep != TutorialStep.MoveAD)
            return;

        SetStep(TutorialStep.JumpCharge);
    }

    // 공중 트리거 진입 시 점프 차지 단계에서 공중 단계로 전환한다.
    public void OnInAirTrigger()
    {
        if (_currentStep != TutorialStep.JumpCharge)
            return;

        SetStep(TutorialStep.InAir);
    }

    // 슬램 입력이 완료된 것으로 판단되면 튜토리얼을 종료한다.
    public void OnSlamInput()
    {
        if (_currentStep != TutorialStep.SlamWait)
            return;

        SetStep(TutorialStep.Finished);
    }

    // 외부에서 단계를 직접 변경할 때 사용하는 진입점이다.
    public void SetStep(TutorialStep nextStep)
    {
        if (_currentStep == nextStep)
            return;

        _currentStep = nextStep;

        ApplyStepRuntimeState(nextStep);
        ApplyStepUI(nextStep);

        if (_showDebugLog)
            Debug.Log($"Tutorial Step Changed : {_currentStep}", this);
    }

    #endregion

    #region Input Type

    // InputTypeManager의 입력 타입 변경 이벤트를 구독한다.
    private void SubscribeInputTypeEvent()
    {
        if (InputTypeManager.Instance == null)
            return;

        InputTypeManager.Instance.OnInputTypeChanged -= OnInputTypeChanged;
        InputTypeManager.Instance.OnInputTypeChanged += OnInputTypeChanged;
    }

    // InputTypeManager의 입력 타입 변경 이벤트를 해제한다.
    private void UnsubscribeInputTypeEvent()
    {
        if (InputTypeManager.Instance == null)
            return;

        InputTypeManager.Instance.OnInputTypeChanged -= OnInputTypeChanged;
    }

    // 입력 장치가 바뀌면 힌트 문구를 새 장치 기준으로 갱신한다.
    private void OnInputTypeChanged(bool isGamePad)
    {
        RefreshHintTexts();
    }

    // 현재 입력 타입에 맞는 힌트 문구를 TMP에 반영한다.
    private void RefreshHintTexts()
    {
        bool isGamePad = ResolveCurrentIsGamePad();

        if (_pressTmp != null)
            _pressTmp.text = isGamePad ? "Press X For Slam!" : "Press SpaceBar For Slam!";

        if (_pressHoldTmp != null)
            _pressHoldTmp.text = isGamePad ? "Press X" : "Press SpaceBar";
    }

    // 현재 입력 장치가 게임패드인지 판별한다.
    private bool ResolveCurrentIsGamePad()
    {
        if (InputTypeManager.Instance != null)
            return InputTypeManager.Instance.IsGamePad;

        bool hasGamePad = Gamepad.current != null;
        bool hasKeyboardOrMouse = Keyboard.current != null || Mouse.current != null;
        return hasGamePad && !hasKeyboardOrMouse;
    }

    #endregion

    #region Step Update

    // 점프 정점에 도달했는지 검사한다.
    private void UpdateInAirStep()
    {
        CheckPeakHeight();
    }

    // 플레이어가 실제로 슬램 상태에 진입했는지 검사한다.
    private void UpdateSlamWaitStep()
    {
        if (_playerController.IsSlamAnticipating || _playerController.IsSlamming)
        {
            SetActiveSafe(_pressTmp, false);
            OnSlamInput();
        }
    }

    #endregion

    #region Step Logic

    // 단계 변경 시 시간 배율과 런타임 플래그를 적용한다.
    private void ApplyStepRuntimeState(TutorialStep step)
    {
        switch (step)
        {
            case TutorialStep.MoveAD:
                _playerController.PlayerInputLock(false);
                RestoreTimeScale();
                ResetRuntimeFlags();
                break;

            case TutorialStep.JumpCharge:
                RestoreTimeScale();
                _canJumpNow = false;
                _isPeakDetected = false;
                break;

            case TutorialStep.InAir:
                RestoreTimeScale();
                break;

            case TutorialStep.SlamWait:
                ApplySlowMotion();
                break;

            case TutorialStep.Finished:
                RestoreTimeScale();
                GameManager.Instance.GameStart();
                break;
        }
    }

    // 단계 변경 시 안내 UI와 힌트 문구를 갱신한다.
    private void ApplyStepUI(TutorialStep step)
    {
        RefreshHintTexts();

        switch (step)
        {
            case TutorialStep.None:
                HideAllHintObjects();
                _uiManualUI?.HideAll();
                break;

            case TutorialStep.MoveAD:
                HideAllHintObjects();
                _uiManualUI?.ShowOnlyAD();
                break;

            case TutorialStep.JumpCharge:
                SetActiveSafe(_pressTmp, false);
                SetActiveSafe(_pressHoldTmp, true);
                _uiManualUI?.ShowOnlySpace();
                break;

            case TutorialStep.InAir:
                SetActiveSafe(_pressTmp, false);
                SetActiveSafe(_pressHoldTmp, false);
                _uiManualUI?.ShowOnlySpace();
                break;

            case TutorialStep.SlamWait:
                SetActiveSafe(_pressHoldTmp, false);
                SetActiveSafe(_pressTmp, true);
                _uiManualUI?.ShowOnlySpace();
                break;

            case TutorialStep.Finished:
                HideAllHintObjects();
                _uiManualUI?.HideAll();
                break;
        }
    }

    // 상승이 끝나고 하강이 시작되는 순간을 잡아 슬램 대기 단계로 넘긴다.
    private void CheckPeakHeight()
    {
        if (_isPeakDetected)
            return;

        if (_playerController.IsGrounded)
            return;

        if (_playerRb.linearVelocity.y > _peakCheckVelocityY)
            return;

        _isPeakDetected = true;
        SetStep(TutorialStep.SlamWait);
    }

    #endregion

    #region Hint UI Follow

    // 따라갈 기준 대상을 반환하며, 비어 있으면 플레이어 본체를 사용한다.
    private Transform GetHintFollowTarget()
    {
        if (_hintFollowTarget != null)
            return _hintFollowTarget;

        if (_playerController != null)
            return _playerController.transform;

        return null;
    }

    // 현재 활성/비활성 여부와 무관하게 힌트 UI 위치를 갱신한다.
    private void UpdateHintFollow()
    {
        Transform followTarget = GetHintFollowTarget();
        if (followTarget == null)
            return;

        Vector3 targetWorldPosition = followTarget.position + _hintFollowOffset;

        UpdateHintObjectPosition(_pressTmp, targetWorldPosition);
        UpdateHintObjectPosition(_pressHoldTmp, targetWorldPosition);
    }

    // 힌트 오브젝트가 UI인지 월드 오브젝트인지에 따라 위치를 안전하게 갱신한다.
    private void UpdateHintObjectPosition(Component hintObject, Vector3 targetWorldPosition)
    {
        if (hintObject == null)
            return;

        RectTransform rectTransform = hintObject.transform as RectTransform;
        if (rectTransform == null)
        {
            hintObject.transform.position = targetWorldPosition;
            return;
        }

        Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            rectTransform.position = targetWorldPosition;
            return;
        }

        if (canvas.renderMode == RenderMode.WorldSpace)
        {
            rectTransform.position = targetWorldPosition;
            return;
        }

        RectTransform parentRect = rectTransform.parent as RectTransform;
        if (parentRect == null)
            return;

        if (_mainCamera == null)
            _mainCamera = Camera.main;

        if (_mainCamera == null)
            return;

        Camera uiCamera = null;
        if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = canvas.worldCamera != null ? canvas.worldCamera : _mainCamera;

        Vector2 screenPosition = _mainCamera.WorldToScreenPoint(targetWorldPosition);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPosition, uiCamera, out Vector2 localPoint))
            rectTransform.localPosition = localPoint;
    }

    #endregion

    #region Helpers

    // 플레이어 컨트롤러를 기준으로 필요한 컴포넌트를 한 번에 캐싱한다.
    private void CacheReferences()
    {
        if (_playerController == null)
        {
            Debug.LogError($"{nameof(TutorialManager)} : _playerController 가 비어 있습니다.", this);
            return;
        }

        _playerRb = _playerController.GetComponent<Rigidbody2D>();
        _playerJumpAction = _playerController.GetComponent<PlayerJumpAction>();

        if (_playerRb == null)
            Debug.LogError($"{nameof(TutorialManager)} : Rigidbody2D 를 찾지 못했습니다.", this);

        if (_playerJumpAction == null)
            Debug.LogError($"{nameof(TutorialManager)} : PlayerJumpAction 을 찾지 못했습니다.", this);
    }

    // 튜토리얼용 런타임 플래그를 초기화한다.
    private void ResetRuntimeFlags()
    {
        _isPeakDetected = false;
        _canJumpNow = false;
    }

    // 모든 힌트 오브젝트를 끈다.
    private void HideAllHintObjects()
    {
        SetActiveSafe(_pressTmp, false);
        SetActiveSafe(_pressHoldTmp, false);
    }

    // 느린 연출을 위해 시간 배율을 낮춘다.
    private void ApplySlowMotion()
    {
        if (_slowMotionRoutine != null)
            StopCoroutine(_slowMotionRoutine);

        _slowMotionRoutine = StartCoroutine(CoApplySlowMotion());
    }

    // 시간 배율을 기본값으로 복구한다.
    private void RestoreTimeScale()
    {
        if (_slowMotionRoutine != null)
        {
            StopCoroutine(_slowMotionRoutine);
            _slowMotionRoutine = null;
        }

        Time.timeScale = 1f;
        Time.fixedDeltaTime = _defaultFixedDeltaTime;
    }

    // 0.1 -> 0으로 unscaled time 기준으로 천천히 감속시킨다.
    private IEnumerator CoApplySlowMotion()
    {
        float elapsed = 0f;
        float startScale = _slowMotionStartScale;

        Time.timeScale = startScale;
        Time.fixedDeltaTime = _defaultFixedDeltaTime * Time.timeScale;

        while (elapsed < _slowMotionLerpDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / _slowMotionLerpDuration);
            Time.timeScale = Mathf.Lerp(startScale, 0f, t);
            Time.fixedDeltaTime = _defaultFixedDeltaTime * Time.timeScale;

            yield return null;
        }

        Time.timeScale = 0f;
        Time.fixedDeltaTime = 0f;
        _slowMotionRoutine = null;
    }

    // null 안전하게 컴포넌트의 게임오브젝트 활성 상태를 바꾼다.
    private void SetActiveSafe(Component target, bool isActive)
    {
        if (target != null)
            target.gameObject.SetActive(isActive);
    }

    #endregion
}