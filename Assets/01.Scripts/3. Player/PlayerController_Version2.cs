using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovementMotor))]
[RequireComponent(typeof(PlayerJumpAction))]
[RequireComponent(typeof(PlayerAirAction))]
public class PlayerControllerVersionTwo : MonoBehaviour
{
    #region Inspector

    [Header("Input Action References | Inspector 연결 필요")]
    [SerializeField] private InputActionReference _moveActionReference;
    [SerializeField] private InputActionReference _jumpActionReference;

    [Header("Landing Jump Assist")]
    [SerializeField] private float _preLandBufferCheckDistance = 0.28f;

    [Header("Debug")]
    [SerializeField] private bool _showDebugLog = false;

    #endregion

    #region Runtime Fields

    private PlayerMovementMotor _movement;
    private PlayerJumpAction _jumpAction;
    private PlayerAirAction _airAction;

    private bool _isCharging;
    private bool _isSlamAnticipating;
    private bool _isSlamming;
    private bool _canSlam = true;

    private float _moveInputX;

    private int _facingDirection = 1;
    private int _lockedActionFacingDirection = 1;

    //이전 점프가 차지였는지?
    private bool _lastJumpWasCharged;
    private float _lastJumpChargeRatio;

    #endregion

    #region Properties

    private InputAction MoveAction => _moveActionReference != null ? _moveActionReference.action : null;
    private InputAction JumpAction => _jumpActionReference != null ? _jumpActionReference.action : null;
    private bool JumpHeld => JumpAction != null && JumpAction.IsPressed();

    public bool IsGrounded => _movement != null && _movement.IsGrounded;
    public bool IsCharging => _isCharging;
    public bool IsSlamAnticipating => _isSlamAnticipating;
    public bool IsSlamming => _isSlamming;
    public bool CanSlam => _canSlam;
    public bool IsMoveLocked => _isCharging || _isSlamAnticipating || _isSlamming;
    public float MoveInputX => _moveInputX;
    public int FacingDirection => _facingDirection;
    public int LockedActionFacingDirection => _lockedActionFacingDirection;
    public bool ShowDebugLog => _showDebugLog;
    public bool LastJumpWasCharged => _lastJumpWasCharged;
    public float LastJumpChargeRatio => _lastJumpChargeRatio;

    public bool IsWalking =>
        IsGrounded &&
        !IsMoveLocked &&
        Mathf.Abs(_moveInputX) > _movement.InputDeadZone;

    public int VisualFacingDirection =>
        IsMoveLocked ? _lockedActionFacingDirection : _facingDirection;

    #endregion

    #region Unity Lifecycle

    // 같은 오브젝트의 필수 컴포넌트를 가져오고, 외부 참조를 검증한다.
    private void Awake()
    {
        _movement = GetComponent<PlayerMovementMotor>();
        _jumpAction = GetComponent<PlayerJumpAction>();
        _airAction = GetComponent<PlayerAirAction>();

        if (!ValidateRequiredReferences())
        {
            enabled = false;
            return;
        }
    }

    // 시작 시 바닥 상태를 초기화한다.
    private void Start()
    {
        _movement.InitializeGroundedState();
    }

    // 입력과 런타임 상태를 활성화한다.
    private void OnEnable()
    {
        MoveAction?.Enable();
        JumpAction?.Enable();

        _canSlam = true;
        _lastJumpWasCharged = false;
        _lastJumpChargeRatio = 0f;
        _lockedActionFacingDirection = _facingDirection;
    }

    // 입력을 비활성화한다.
    private void OnDisable()
    {
        MoveAction?.Disable();
        JumpAction?.Disable();
    }

    // 입력과 타이머를 갱신한다.
    private void Update()
    {
        ReadMoveInput();
        UpdateFacingDirection();

        _jumpAction.TickTimers(_isCharging, JumpHeld, Time.deltaTime);
        HandlePreLandHeldJumpBuffer();
        _airAction.TickAnticipationTimer(this, _movement, Time.deltaTime);

        HandleJumpInput();
    }

    // 물리 상태와 이동을 갱신한다.
    private void FixedUpdate()
    {
        _movement.UpdateGroundedState();
        HandleGroundedStateTransitions();

        if (_isCharging)
        {
            _movement.ApplyChargeLock();
            _movement.ApplyGravityScale(this);
            return;
        }

        if (_isSlamAnticipating)
        {
            _movement.ApplySlamAnticipationLock();
            _movement.ApplyGravityScale(this);
            return;
        }

        if (_isSlamming)
        {
            _movement.ApplySlamMotion(_airAction.SlamSpeed);
            _movement.ApplyGravityScale(this);
            return;
        }

        _jumpAction.TryConsumeBufferedJumpOnGround(this, _movement, JumpHeld);
        _movement.ApplyHorizontalMovement(this);
        _movement.ApplyGravityScale(this);
        _movement.ClampGroundedVerticalVelocity(this);
    }

    #endregion

    #region Setup Validation
    // 공중에서 점프 버튼을 누른 채 바닥에 가까워지면 착지용 점프 버퍼를 예약한다.
    private void HandlePreLandHeldJumpBuffer()
    {
        if (IsGrounded)
            return;

        if (!JumpHeld)
            return;

        if (_jumpAction.HasBufferedJump)
            return;

        if (_movement.IsNearGroundForBufferedLandingJump(_preLandBufferCheckDistance))
        {
            _jumpAction.SetJumpBuffer();

            if (_showDebugLog)
                Debug.Log("Pre-land Held Jump Buffered");
        }
    }

    // 인스펙터에서 반드시 연결해야 하는 참조를 검증한다.
    private bool ValidateRequiredReferences()
    {
        bool isValid = true;

        if (_moveActionReference == null)
        {
            Debug.LogError($"{nameof(PlayerControllerVersionTwo)}: _moveActionReference 가 비어 있습니다.", this);
            isValid = false;
        }

        if (_jumpActionReference == null)
        {
            Debug.LogError($"{nameof(PlayerControllerVersionTwo)}: _jumpActionReference 가 비어 있습니다.", this);
            isValid = false;
        }

        if (_movement == null)
        {
            Debug.LogError($"{nameof(PlayerControllerVersionTwo)}: PlayerMovementMotor 를 찾지 못했습니다.", this);
            isValid = false;
        }

        if (_jumpAction == null)
        {
            Debug.LogError($"{nameof(PlayerControllerVersionTwo)}: PlayerJumpAction 을 찾지 못했습니다.", this);
            isValid = false;
        }

        if (_airAction == null)
        {
            Debug.LogError($"{nameof(PlayerControllerVersionTwo)}: PlayerAirAction 을 찾지 못했습니다.", this);
            isValid = false;
        }

        return isValid;
    }

    #endregion

    #region Input

    // 좌우 이동 입력값을 읽는다.
    private void ReadMoveInput()
    {
        if (MoveAction == null)
        {
            _moveInputX = 0f;
            return;
        }

        float x = MoveAction.ReadValue<Vector2>().x;
        _moveInputX = Mathf.Abs(x) >= _movement.InputDeadZone ? x : 0f;
    }

    // 지상에서만 바라보는 방향을 갱신한다.
    private void UpdateFacingDirection()
    {
        if (!IsGrounded)
            return;

        if (_isCharging || _isSlamAnticipating)
            return;

        int inputFacing = GetInputFacingDirection();
        if (inputFacing != 0)
            _facingDirection = inputFacing;
    }

    // 점프 입력에 따라 차지, 슬램, 점프 버퍼를 처리한다.
    private void HandleJumpInput()
    {
        if (JumpAction == null)
            return;

        bool pressed = JumpAction.WasPressedThisFrame();
        bool released = JumpAction.WasReleasedThisFrame();

        if (pressed)
        {
            if (IsGrounded && !IsMoveLocked)
            {
                _jumpAction.BeginCharge(this, _movement);
                return;
            }

            if (!IsGrounded)
            {
                if (_movement.IsNearGroundForBufferedLandingJump(_preLandBufferCheckDistance))
                {
                    _jumpAction.SetJumpBuffer();

                    if (_showDebugLog)
                        Debug.Log("Pre-land Buffered Jump");

                    return;
                }

                if (_airAction.CanStartAirSlam(this))
                {
                    _airAction.StartSlamAnticipation(this, _movement);
                    return;
                }

                _jumpAction.SetJumpBuffer();
                return;
            }
        }

        if (_isCharging && released)
            _jumpAction.ReleaseChargeJump(this, _movement);
    }

    // 현재 입력에서 좌우 방향값을 추출한다.
    public int GetInputFacingDirection()
    {
        if (_moveInputX > _movement.InputDeadZone)
            return 1;

        if (_moveInputX < -_movement.InputDeadZone)
            return -1;

        return 0;
    }

    // 행동 시작 시 사용할 방향을 결정하고 고정한다.
    public void LockFacingForAction()
    {
        int inputFacing = GetInputFacingDirection();
        _lockedActionFacingDirection = inputFacing != 0 ? inputFacing : _facingDirection;
        _facingDirection = _lockedActionFacingDirection;
    }

    #endregion

    #region State Control

    // 차지 상태에 진입한다.
    public void EnterChargeState()
    {
        _isCharging = true;
        _isSlamAnticipating = false;
        _isSlamming = false;

        LockFacingForAction();
    }

    // 점프 발사 직후 상태를 갱신한다.
    public void OnJumpLaunched(bool wasChargedJump, float jumpChargeRatio)
    {
        _isCharging = false;
        _canSlam = true;
        _lastJumpWasCharged = wasChargedJump;
        _lastJumpChargeRatio = wasChargedJump ? Mathf.Clamp01(jumpChargeRatio) : 0f;
    }

    // 슬램 예고 상태에 진입한다.
    public void EnterSlamAnticipationState()
    {
        _isCharging = false;
        _isSlamAnticipating = true;
        _isSlamming = false;
        _canSlam = false;

        _lockedActionFacingDirection = _facingDirection;
    }

    // 슬램 돌입 상태에 진입한다.
    public void EnterSlamDiveState()
    {
        _isSlamAnticipating = false;
        _isSlamming = true;
    }

    // 착지 후 공중 행동 상태를 초기화한다.
    public void ResetAirActionStates()
    {
        _isSlamAnticipating = false;
        _isSlamming = false;
        _canSlam = true;
        _lastJumpWasCharged = false;
        _lastJumpChargeRatio = 0f;
    }

    // 접지 이탈로 차지를 취소한다.
    public void CancelChargeBecauseGroundLost()
    {
        _isCharging = false;
    }

    #endregion

    #region Ground / Landing

    // 이륙과 착지 시점의 상태 변화를 처리한다.
    private void HandleGroundedStateTransitions()
    {
        bool justLanded = !_movement.WasGrounded && _movement.IsGrounded && _movement.VerticalVelocity <= 0.01f;
        bool justLeftGround = _movement.WasGrounded && !_movement.IsGrounded;
        bool landedFromSlam = justLanded && _isSlamming;

        if (_showDebugLog)
        {
            Debug.Log(
                $"Landing Check | justLanded: {justLanded}, " +
                $"isSlamming: {_isSlamming}, " +
                $"landedFromSlam: {landedFromSlam}, " +
                $"velocityY: {_movement.VerticalVelocity}");
        }

        if (landedFromSlam)
            _airAction.ApplySlamDamage(this, _movement, transform.position, _showDebugLog);

        if (justLanded)
            OnLanded();

        if (justLeftGround && _showDebugLog)
            Debug.Log("Left Ground");

        if (_isCharging && !_movement.IsGrounded)
        {
            CancelChargeBecauseGroundLost();

            if (_showDebugLog)
                Debug.Log("Charge canceled because grounded state was lost.");
        }
    }

    // 착지 시 공중 행동 상태를 초기화한다.
    private void OnLanded()
    {
        ResetAirActionStates();
        _airAction.ResetRuntimeState();
        _movement.ClearJumpLaunchMomentum();

        if (_showDebugLog)
            Debug.Log("Landed");
    }

    #endregion
}