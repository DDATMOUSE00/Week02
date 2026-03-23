using System.Collections;
using UnityEngine;

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
    [SerializeField] private GameObject _pressTmp;
    [SerializeField] private GameObject _pressHoldTmp;

    [Header("Debug")]
    [SerializeField] private bool _showDebugLog = false;

    public float GameStartPosition;

    //슬로우 연출용
    private Coroutine _slowMotionRoutine;
    private float _defaultFixedDeltaTime;

    #endregion

    #region Runtime Fields

    private Rigidbody2D _playerRb;
    private PlayerJumpAction _playerJumpAction;

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

    // 싱글톤 초기화 시 플레이어 관련 참조를 캐싱한다.
    public override void Init()
    {
        _defaultFixedDeltaTime = Time.fixedDeltaTime;
        _playerController.PlayerInputLock(true);

        CacheReferences();
        RestoreTimeScale();
        ResetRuntimeFlags();
        HideAllHintObjects();
    }

    protected override void Awake()
    {
        base.Awake();

        Init();
    }
    // 비활성화될 때 슬로모션이 남지 않도록 복구한다.
    private void OnDestroy()
    {
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

    #endregion

    #region Public API

    // 튜토리얼을 시작 상태로 초기화하고 첫 단계를 연다.
    public void StartTutorial()
    {
        if (_uiManualUI != null)
            _uiManualUI.gameObject.SetActive(true);

        ResetRuntimeFlags();
        SetStep(TutorialStep.MoveAD);
    }

    // 첫 번째 트리거 진입 시 이동 튜토리얼에서 점프 차지 단계로 전환한다.
    public void OnJumpTrigger()
    {
        if (_currentStep != TutorialStep.MoveAD)
            return;

        SetStep(TutorialStep.JumpCharge); //스페이스바 누르라고 뜸
    }
    public void OnInAirTrigger()
    {
        if (_currentStep != TutorialStep.JumpCharge)
            return;

        SetStep(TutorialStep.InAir); //스페이스바 누르라고 뜸
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

    #region Step Update

    // 차지량이 충분한지와 실제 점프 이륙 여부를 검사한다.
    // TODO: 수정해야함, 이걸 트리거로 바꿔야함

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

    // 단계 변경 시 안내 UI를 갱신한다.
    private void ApplyStepUI(TutorialStep step)
    {
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

    // null 안전하게 오브젝트 활성 상태를 바꾼다.
    private void SetActiveSafe(GameObject target, bool isActive)
    {
        if (target != null)
            target.SetActive(isActive);
    }

    #endregion
}