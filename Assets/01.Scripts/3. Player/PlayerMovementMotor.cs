using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovementMotor : MonoBehaviour
{
    #region Inspector

    [Header("Ground Check | Inspector 연결 필요")]
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private Vector2 _groundCheckSize = new(0.6f, 0.12f);

    [Header("Ground Resolve")]
    [SerializeField] private float _impactResolveCastStartOffsetY = 0.3f;
    [SerializeField] private float _impactResolveCastExtraDistance = 0.3f;
    [SerializeField] private float _impactSurfaceOffsetY = 0.02f;

    [Header("Grounded Guard")]
    [SerializeField] private float _jumpDetachIgnoreTime = 0.08f;

    [Header("Move")]
    [SerializeField] private float _groundMoveSpeed = 6f;
    [SerializeField] private float _inputDeadZone = 0.1f;

    [Header("Jump Launch Momentum")]
    [SerializeField] private bool _useJumpLaunchMomentum = true;
    [SerializeField] private float _launchMomentumDecay = 0f;

    [Header("Gravity")]
    [SerializeField] private float _riseGravityScale = 2.2f;
    [SerializeField] private float _fallGravityScale = 3.6f;

    #endregion

    #region Runtime Fields

    private Rigidbody2D _rb;

    private bool _isGrounded;
    private bool _wasGrounded;

    private bool _hasJumpLaunchMomentum;
    private float _jumpLaunchVelocityX;
    private float _jumpDetachIgnoreTimer;

    #endregion

    #region Properties

    public bool IsGrounded => _isGrounded;
    public bool WasGrounded => _wasGrounded;
    public float VerticalVelocity => _rb.linearVelocity.y;
    public float InputDeadZone => _inputDeadZone;

    #endregion

    #region Unity Lifecycle

    // 같은 오브젝트의 필수 컴포넌트를 가져오고, 외부 참조를 검증한다.
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        if (!ValidateRequiredReferences())
        {
            enabled = false;
            return;
        }
    }

    // 접지 무시 타이머를 감소시킨다.
    private void Update()
    {
        if (_jumpDetachIgnoreTimer > 0f)
            _jumpDetachIgnoreTimer -= Time.deltaTime;
    }

    // 인스펙터 값이 잘못되면 안전한 최소값으로 보정한다.
    private void OnValidate()
    {
        _groundCheckSize.x = Mathf.Max(0.01f, _groundCheckSize.x);
        _groundCheckSize.y = Mathf.Max(0.01f, _groundCheckSize.y);
        _impactResolveCastStartOffsetY = Mathf.Max(0f, _impactResolveCastStartOffsetY);
        _impactResolveCastExtraDistance = Mathf.Max(0f, _impactResolveCastExtraDistance);
        _jumpDetachIgnoreTime = Mathf.Max(0f, _jumpDetachIgnoreTime);
        _groundMoveSpeed = Mathf.Max(0f, _groundMoveSpeed);
        _inputDeadZone = Mathf.Clamp(_inputDeadZone, 0f, 1f);
        _riseGravityScale = Mathf.Max(0f, _riseGravityScale);
        _fallGravityScale = Mathf.Max(0f, _fallGravityScale);
        _launchMomentumDecay = Mathf.Max(0f, _launchMomentumDecay);
    }

    #endregion

    #region Setup Validation

    // 인스펙터에서 반드시 연결해야 하는 참조를 검증한다.
    private bool ValidateRequiredReferences()
    {
        bool isValid = true;

        if (_groundCheck == null)
        {
            Debug.LogError($"{nameof(PlayerMovementMotor)}: _groundCheck 가 비어 있습니다.", this);
            isValid = false;
        }

        return isValid;
    }

    #endregion

    #region Ground Check

    // 시작 시 바닥 상태를 초기화한다.
    public void InitializeGroundedState()
    {
        _isGrounded = CheckGrounded();
        _wasGrounded = _isGrounded;
    }

    // 현재 바닥 상태를 갱신한다.
    public void UpdateGroundedState()
    {
        _wasGrounded = _isGrounded;
        _isGrounded = CheckGrounded();
    }

    // 발밑 오버랩 박스로 바닥 접지를 판정한다.
    private bool CheckGrounded()
    {
        if (_groundCheck == null)
            return false;

        if (_jumpDetachIgnoreTimer > 0f && _rb.linearVelocity.y >= 0f)
            return false;

        return Physics2D.OverlapBox(_groundCheck.position, _groundCheckSize, 0f, _groundLayer) != null;
    }

    // 착지 직전인지 검사해 점프 버퍼 우선 여부를 판단한다.
    public bool IsNearGroundForBufferedLandingJump(float checkDistance)
    {
        if (_isGrounded || _groundCheck == null)
            return false;

        if (checkDistance <= 0f)
            return false;

        if (_rb.linearVelocity.y > 0f)
            return false;

        return Physics2D.BoxCast(
            _groundCheck.position,
            _groundCheckSize,
            0f,
            Vector2.down,
            checkDistance,
            _groundLayer).collider != null;
    }

    #endregion

    #region Movement

    // 현재 상태에 맞는 수평 이동 속도를 적용한다.
    public void ApplyHorizontalMovement(PlayerControllerVersionTwo controller)
    {
        Vector2 velocity = _rb.linearVelocity;

        if (_isGrounded)
        {
            velocity.x = controller.MoveInputX * _groundMoveSpeed;
            _rb.linearVelocity = velocity;
            return;
        }

        if (_hasJumpLaunchMomentum)
        {
            UpdateJumpLaunchMomentum();
            velocity.x = _jumpLaunchVelocityX;
            _rb.linearVelocity = velocity;
            return;
        }

        // 공중 조작은 막혀 있으므로 현재 x 속도를 유지한다.
    }

    // 차지 중에는 완전히 정지시킨다.
    public void ApplyChargeLock()
    {
        FreezeBody();
    }

    // 슬램 예고 중에는 완전히 정지시킨다.
    public void ApplySlamAnticipationLock()
    {
        FreezeBody();
    }

    // 슬램 중에는 수직으로 강하게 하강시킨다.
    public void ApplySlamMotion(float slamSpeed)
    {
        SetVelocity(new Vector2(0f, -slamSpeed));
    }

    // 몸을 완전히 정지시킨다.
    public void FreezeBody()
    {
        SetVelocity(Vector2.zero);
    }

    // Rigidbody 속도를 직접 설정한다.
    public void SetVelocity(Vector2 velocity)
    {
        _rb.linearVelocity = velocity;
    }

    #endregion

    #region Gravity

    // 현재 상태에 맞는 중력 스케일을 설정한다.
    public void ApplyGravityScale(PlayerControllerVersionTwo controller)
    {
        if (controller.IsCharging || controller.IsSlamAnticipating || controller.IsSlamming)
        {
            _rb.gravityScale = 0f;
            return;
        }

        if (_isGrounded)
        {
            _rb.gravityScale = 0f;
            return;
        }

        _rb.gravityScale = _rb.linearVelocity.y > 0.01f ? _riseGravityScale : _fallGravityScale;
    }

    // 지상에서 남은 하강 속도를 제거한다.
    public void ClampGroundedVerticalVelocity(PlayerControllerVersionTwo controller)
    {
        if (!_isGrounded || controller.IsMoveLocked || _rb.linearVelocity.y >= 0f)
            return;

        Vector2 velocity = _rb.linearVelocity;
        velocity.y = 0f;
        _rb.linearVelocity = velocity;
    }

    #endregion

    #region Jump Launch Momentum

    // 점프 직후 접지 무시 시간을 시작한다.
    public void StartJumpDetachIgnore()
    {
        _jumpDetachIgnoreTimer = _jumpDetachIgnoreTime;
    }

    // 점프 발사 운동량을 등록한다.
    public void RegisterJumpLaunch(float launchVelocityX)
    {
        if (_useJumpLaunchMomentum)
        {
            _jumpLaunchVelocityX = launchVelocityX;
            _hasJumpLaunchMomentum = true;
        }
        else
        {
            ClearJumpLaunchMomentum();
        }
    }

    // 점프 발사 운동량을 초기화한다.
    public void ClearJumpLaunchMomentum()
    {
        _jumpLaunchVelocityX = 0f;
        _hasJumpLaunchMomentum = false;
    }

    // 점프 발사 운동량을 감쇠 옵션에 따라 갱신한다.
    private void UpdateJumpLaunchMomentum()
    {
        if (_launchMomentumDecay <= 0f)
            return;

        _jumpLaunchVelocityX = Mathf.MoveTowards(
            _jumpLaunchVelocityX,
            0f,
            _launchMomentumDecay * Time.fixedDeltaTime);

        if (Mathf.Abs(_jumpLaunchVelocityX) <= 0.01f)
            ClearJumpLaunchMomentum();
    }

    #endregion

    #region Utility

    // 슬램 충돌 중심점을 지면 표면 기준의 안전한 좌표로 복원한다.
    public Vector2 GetImpactPoint(Vector3 fallbackPosition)
    {
        if (_groundCheck == null)
            return fallbackPosition;

        Vector2 castOrigin = (Vector2)_groundCheck.position + Vector2.up * _impactResolveCastStartOffsetY;
        float castDistance = _impactResolveCastStartOffsetY + _impactResolveCastExtraDistance;

        RaycastHit2D hit = Physics2D.BoxCast(
            castOrigin,
            _groundCheckSize,
            0f,
            Vector2.down,
            castDistance,
            _groundLayer);

        if (hit.collider != null)
        {
            float resolvedSurfaceY = hit.centroid.y - (_groundCheckSize.y * 0.5f) + _impactSurfaceOffsetY;
            return new Vector2(_groundCheck.position.x, resolvedSurfaceY);
        }

        return _groundCheck.position;
    }

    #endregion

    #region Gizmos

    // 에디터에서 바닥 체크 박스와 충돌 복원 캐스트를 시각화한다.
    private void OnDrawGizmosSelected()
    {
        if (_groundCheck == null)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_groundCheck.position, _groundCheckSize);

        Vector3 castOrigin = _groundCheck.position + Vector3.up * _impactResolveCastStartOffsetY;
        float castDistance = _impactResolveCastStartOffsetY + _impactResolveCastExtraDistance;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(castOrigin, _groundCheckSize);
        Gizmos.DrawLine(castOrigin, castOrigin + Vector3.down * castDistance);
    }

    #endregion
}