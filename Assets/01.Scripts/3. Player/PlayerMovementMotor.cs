using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
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

    [Header("World X Bound")]
    [SerializeField] private bool _useWorldXBound = true;
    [SerializeField] private float _minWorldX = -10f;
    [SerializeField] private float _maxWorldX = 10f;

    [Header("Simple Wall Resolve")]
    [SerializeField] private bool _useSimpleWallResolve = true;
    [SerializeField] private Collider2D _tutorialObstacleCollider;
    [SerializeField] private float _wallResolveSkin = 0.02f;
    [SerializeField] private float _wallContactCheckDistance = 0.06f;

    #endregion

    #region Runtime Fields

    private Rigidbody2D _rb;
    private Collider2D _bodyCollider;

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

    // 같은 오브젝트의 Rigidbody2D와 Collider2D를 캐싱하고 필수 참조를 검증한다.
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _bodyCollider = GetComponent<Collider2D>();

        if (!ValidateRequiredReferences())
        {
            enabled = false;
            return;
        }
    }

    // 점프 직후 접지 무시 타이머를 매 프레임 감소시킨다.
    private void Update()
    {
        if (_jumpDetachIgnoreTimer > 0f)
            _jumpDetachIgnoreTimer -= Time.deltaTime;
    }

    // 인스펙터 값이 비정상일 때 최소 안전 범위로 자동 보정한다.
    private void OnValidate()
    {
        _groundCheckSize.x = Mathf.Max(0.01f, _groundCheckSize.x);
        _groundCheckSize.y = Mathf.Max(0.01f, _groundCheckSize.y);
        _impactResolveCastStartOffsetY = Mathf.Max(0f, _impactResolveCastStartOffsetY);
        _impactResolveCastExtraDistance = Mathf.Max(0f, _impactResolveCastExtraDistance);
        _impactSurfaceOffsetY = Mathf.Max(0f, _impactSurfaceOffsetY);
        _jumpDetachIgnoreTime = Mathf.Max(0f, _jumpDetachIgnoreTime);
        _groundMoveSpeed = Mathf.Max(0f, _groundMoveSpeed);
        _inputDeadZone = Mathf.Clamp(_inputDeadZone, 0f, 1f);
        _riseGravityScale = Mathf.Max(0f, _riseGravityScale);
        _fallGravityScale = Mathf.Max(0f, _fallGravityScale);
        _launchMomentumDecay = Mathf.Max(0f, _launchMomentumDecay);
        _wallResolveSkin = Mathf.Max(0f, _wallResolveSkin);
        _wallContactCheckDistance = Mathf.Max(0f, _wallContactCheckDistance);
    }

    #endregion

    #region Setup Validation

    // 실행에 필요한 참조가 빠졌는지 검사하고 에디터에서 바로 알 수 있게 에러를 출력한다.
    private bool ValidateRequiredReferences()
    {
        bool isValid = true;

        if (_groundCheck == null)
        {
            Debug.LogError($"{nameof(PlayerMovementMotor)}: _groundCheck 가 비어 있습니다.", this);
            isValid = false;
        }

        if (_bodyCollider == null)
        {
            Debug.LogError($"{nameof(PlayerMovementMotor)}: 플레이어 본체 Collider2D 를 찾지 못했습니다.", this);
            isValid = false;
        }

        return isValid;
    }

    #endregion

    #region Ground Check

    // 시작 시 현재 접지 상태를 읽어서 이전 상태와 함께 초기화한다.
    public void InitializeGroundedState()
    {
        _isGrounded = CheckGrounded();
        _wasGrounded = _isGrounded;
    }

    // 매 물리 프레임마다 이전 접지 상태와 현재 접지 상태를 갱신한다.
    public void UpdateGroundedState()
    {
        _wasGrounded = _isGrounded;
        _isGrounded = CheckGrounded();
    }

    // 발밑 오버랩 박스로 현재 바닥 접지 여부를 판정한다.
    private bool CheckGrounded()
    {
        if (_groundCheck == null)
            return false;

        if (_jumpDetachIgnoreTimer > 0f && _rb.linearVelocity.y >= 0f)
            return false;

        return Physics2D.OverlapBox(_groundCheck.position, _groundCheckSize, 0f, _groundLayer) != null;
    }

    // 착지 직전의 짧은 구간인지 검사해서 버퍼 점프 우선 처리 여부를 판단한다.
    public bool IsNearGroundForBufferedLandingJump(float checkDistance)
    {
        if (_isGrounded || _groundCheck == null || checkDistance <= 0f || _rb.linearVelocity.y > 0f)
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

    // 현재 상태에 맞춰 지상 이동 또는 점프 발사 운동량 기반의 수평 속도를 적용한다.
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
        }
    }

    // 차지 중에는 모든 이동을 멈춰 입력 연출에 집중하도록 만든다.
    public void ApplyChargeLock()
    {
        FreezeBody();
    }

    // 슬램 예고 중에는 모든 이동을 멈춰 정지 연출을 보장한다.
    public void ApplySlamAnticipationLock()
    {
        FreezeBody();
    }

    // 슬램 중에는 수직 속도를 강제로 내려찍기 값으로 고정한다.
    public void ApplySlamMotion(float slamSpeed)
    {
        SetVelocity(new Vector2(0f, -slamSpeed));
    }

    // Rigidbody2D의 현재 속도를 완전히 0으로 만들어 즉시 정지시킨다.
    public void FreezeBody()
    {
        SetVelocity(Vector2.zero);
    }

    // Rigidbody2D의 선형 속도를 외부 상태에 맞게 직접 덮어쓴다.
    public void SetVelocity(Vector2 velocity)
    {
        _rb.linearVelocity = velocity;
    }

    // 이동이 끝난 뒤 월드 좌우 제한과 튜토리얼 벽 접촉 보정을 순서대로 처리한다.
    public void ResolveEnvironmentConstraints()
    {
        ClampWorldX();
        ResolveTutorialWallContact();
    }

    #endregion

    #region Gravity

    // 현재 플레이 상태에 따라 중력 스케일을 0, 상승 중력, 하강 중력 중 하나로 설정한다.
    public void ApplyGravityScale(PlayerControllerVersionTwo controller)
    {
        if (controller.IsCharging || controller.IsSlamAnticipating || controller.IsSlamming || _isGrounded)
        {
            _rb.gravityScale = 0f;
            return;
        }

        _rb.gravityScale = _rb.linearVelocity.y > 0.01f ? _riseGravityScale : _fallGravityScale;
    }

    // 지상에서 남아 있는 하강 속도를 제거해 착지 직후 바닥 파고듦을 방지한다.
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

    // 점프 직후 짧은 시간 동안 바닥 재접지를 무시하도록 타이머를 시작한다.
    public void StartJumpDetachIgnore()
    {
        _jumpDetachIgnoreTimer = _jumpDetachIgnoreTime;
    }

    // 점프 시 계산된 발사형 수평 운동량을 등록해 공중 초기 추진력을 유지한다.
    public void RegisterJumpLaunch(float launchVelocityX)
    {
        if (!_useJumpLaunchMomentum)
        {
            ClearJumpLaunchMomentum();
            return;
        }

        _jumpLaunchVelocityX = launchVelocityX;
        _hasJumpLaunchMomentum = true;
    }

    // 점프 발사 운동량 상태를 초기값으로 되돌려 공중 추진력을 제거한다.
    public void ClearJumpLaunchMomentum()
    {
        _jumpLaunchVelocityX = 0f;
        _hasJumpLaunchMomentum = false;
    }

    // 점프 발사 운동량이 켜져 있을 때 설정된 감쇠값만큼 서서히 0으로 줄인다.
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

    #region Environment Constraint

    // 플레이어가 지정된 월드 좌우 범위를 벗어나지 않도록 X 좌표와 진행 방향 속도를 정리한다.
    private void ClampWorldX()
    {
        if (!_useWorldXBound)
            return;

        float minX = Mathf.Min(_minWorldX, _maxWorldX);
        float maxX = Mathf.Max(_minWorldX, _maxWorldX);

        Vector2 position = _rb.position;
        Vector2 velocity = _rb.linearVelocity;
        float clampedX = Mathf.Clamp(position.x, minX, maxX);

        bool wasClamped = !Mathf.Approximately(position.x, clampedX);
        bool hitLeftBound = clampedX <= minX + 0.0001f && velocity.x < 0f;
        bool hitRightBound = clampedX >= maxX - 0.0001f && velocity.x > 0f;

        if (wasClamped)
        {
            position.x = clampedX;
            _rb.position = position;
        }

        if (!hitLeftBound && !hitRightBound)
            return;

        velocity.x = 0f;
        _rb.linearVelocity = velocity;
        ClearJumpLaunchMomentum();
    }

    // 공중에서 튜토리얼 장애물의 옆면에 닿거나 살짝 겹치면 X축으로 떼어내며 벽 달라붙음을 끊는다.
    private void ResolveTutorialWallContact()
    {
        if (!_useSimpleWallResolve || _tutorialObstacleCollider == null || _bodyCollider == null || _isGrounded)
            return;

        ColliderDistance2D distance = _bodyCollider.Distance(_tutorialObstacleCollider);

        if (distance.isOverlapped)
        {
            ResolveWallOverlap(distance);
            return;
        }

        if (!IsSideWallContact(distance))
            return;

        Vector2 velocity = _rb.linearVelocity;
        bool pushingIntoLeftWall = distance.normal.x > 0.01f && velocity.x < 0f;
        bool pushingIntoRightWall = distance.normal.x < -0.01f && velocity.x > 0f;

        if (!pushingIntoLeftWall && !pushingIntoRightWall)
            return;

        velocity.x = 0f;
        _rb.linearVelocity = velocity;
        ClearJumpLaunchMomentum();
    }

    // 실제로 벽과 겹쳤을 때 normal 방향과 여유 거리만큼 X축으로만 안전하게 밀어낸다.
    private void ResolveWallOverlap(ColliderDistance2D distance)
    {
        Vector2 separation = distance.normal * (Mathf.Abs(distance.distance) + _wallResolveSkin);

        if (Mathf.Abs(separation.x) <= 0.0001f)
            return;

        Vector2 position = _rb.position;
        Vector2 velocity = _rb.linearVelocity;

        position.x += separation.x;
        velocity.x = 0f;

        _rb.position = position;
        _rb.linearVelocity = velocity;
        ClearJumpLaunchMomentum();
    }

    // 현재 충돌 방향이 위아래가 아니라 좌우 벽 접촉에 가까운 상황인지 판별한다.
    private bool IsSideWallContact(ColliderDistance2D distance)
    {
        if (distance.isOverlapped)
            return true;

        if (distance.distance < 0f || distance.distance > _wallContactCheckDistance)
            return false;

        return Mathf.Abs(distance.normal.x) > 0.6f;
    }

    #endregion

    #region Utility

    // 슬램 충돌 지점을 발밑 기준으로 다시 계산해 이펙트가 지면 속이 아니라 표면에서 나오게 만든다.
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

        if (hit.collider == null)
            return _groundCheck.position;

        float resolvedSurfaceY = hit.centroid.y - (_groundCheckSize.y * 0.5f) + _impactSurfaceOffsetY;
        return new Vector2(_groundCheck.position.x, resolvedSurfaceY);
    }

    #endregion

    #region Gizmos

    // 선택 시 바닥 체크 영역과 충돌 복원 범위, 월드 바운드, 튜토리얼 장애물을 에디터에 시각화한다.
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

        if (_useWorldXBound)
        {
            float minX = Mathf.Min(_minWorldX, _maxWorldX);
            float maxX = Mathf.Max(_minWorldX, _maxWorldX);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(new Vector3(minX, -100f, 0f), new Vector3(minX, 100f, 0f));
            Gizmos.DrawLine(new Vector3(maxX, -100f, 0f), new Vector3(maxX, 100f, 0f));
        }

        if (_tutorialObstacleCollider != null)
        {
            Gizmos.color = Color.red;
            Bounds obstacleBounds = _tutorialObstacleCollider.bounds;
            Gizmos.DrawWireCube(obstacleBounds.center, obstacleBounds.size);
        }
    }

    #endregion
}