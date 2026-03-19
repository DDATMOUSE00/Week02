using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class DebugPlayerControllerVersion2 : MonoBehaviour
{
    #region Inspector
    [Header("Camera")]
    [SerializeField] private DebugCameraController _cameraController;

    [Header("Input Action References")]
    [SerializeField] private InputActionReference _moveActionReference;
    [SerializeField] private InputActionReference _jumpActionReference;

    [Header("Reference")]
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private Transform _groundCheck;

    [Header("Ground Check")]
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private Vector2 _groundCheckSize = new(0.6f, 0.12f);

    [Header("Grounded Guard")]
    [SerializeField] private float _jumpDetachIgnoreTime = 0.08f;

    [Header("Move")]
    [SerializeField] private float _groundMoveSpeed = 6f;
    [SerializeField] private float _airMoveSpeed = 6f;
    [SerializeField] private float _inputDeadZone = 0.1f;

    [Header("Jump")]
    [SerializeField] private float _normalJumpForceY = 10f;
    [SerializeField] private float _normalJumpForceX = 4f;

    [Header("Charge Jump")]
    [SerializeField] private float _chargeThreshold = 0.12f;
    [SerializeField] private float _maxChargeTime = 0.6f;
    [SerializeField] private float _maxChargeJumpForceY = 16f;
    [SerializeField] private float _maxChargeJumpForceX = 8f;

    [Header("Jump Launch Momentum")]
    [SerializeField] private bool _useJumpLaunchMomentum = true;
    [SerializeField][Range(0f, 1f)] private float _airControlPercent = 0.1f;
    [SerializeField] private float _launchMomentumDecay = 0f;

    [Header("Slam")]
    [SerializeField] private float _slamSpeed = 22f;

    [Header("Slam Anticipation")]
    [SerializeField] private float _slamAnticipationDuration = 0.08f;
    [SerializeField] private Transform _visualRoot;
    [SerializeField] private float _slamAnticipationScaleX = 1.12f;
    [SerializeField] private float _slamAnticipationScaleY = 0.88f;
    [SerializeField] private float _slamAnticipationRotateZ = 10f;
    [SerializeField] private float _slamDiveStretchScaleX = 0.92f;
    [SerializeField] private float _slamDiveStretchScaleY = 1.16f;
    [SerializeField] private float _slamDiveStretchDuration = 0.06f;

    [Header("Slam Impact Damage")]
    [SerializeField] private LayerMask _slamDamageLayer;
    [SerializeField] private float _slamBaseImpactRadius = 1.2f;
    [SerializeField] private float _slamImpactRadiusPerHeight = 0.35f;
    [SerializeField] private float _slamMaxImpactRadius = 3.5f;
    [SerializeField] private float _slamBaseDamage = 10f;
    [SerializeField] private float _slamDamagePerHeight = 4f;
    [SerializeField] private float _slamMaxDamage = 999f;

    [Header("Gravity")]
    [SerializeField] private float _riseGravityScale = 2.2f;
    [SerializeField] private float _fallGravityScale = 3.6f;
    [SerializeField] private float _slamGravityScale = 6f;

    [Header("Buffer")]
    [SerializeField] private float _jumpBufferTime = 0.12f;

    [Header("Landing Jump Assist")]
    [SerializeField] private float _preLandBufferCheckDistance = 0.28f;

    [Header("Debug")]
    [SerializeField] private bool _showDebugLog = false;

    [Header("Sprite")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Sprite _defaultSprite;
    [SerializeField] private Sprite _jumpSprite;
    [SerializeField] private Sprite _slamSprite;

    [Header("Walk Visual")]
    [SerializeField] private DebugGorillaWalkBounce _gorillaWalkBounce;

    [Header("Walk Dust FX")]
    [SerializeField] private ParticleSystem _walkDustParticle;
    [SerializeField] private float _walkDustInterval = 0.12f;

    [Header("Slam Land Impact")]
    [SerializeField] private bool _slamLandEffectOnlyAfterChargedJump = true;
    [SerializeField] private ParticleSystem _slamLandImpactParticle;
    [SerializeField] private bool _useSlamLandHitStop = true;
    [SerializeField] private float _slamLandHitStopDuration = 0.035f;

    #endregion

    #region Runtime Fields

    private bool _isGrounded;
    private bool _wasGrounded;

    private bool _isCharging;
    private bool _isSlamAnticipating;
    private bool _isSlamming;
    private bool _canSlam = true;

    private bool _wasCurrentJumpCharged;
    private bool _slamStartedFromChargedJump;

    private bool _hasJumpLaunchMomentum;

    private float _moveInputX;
    private float _jumpLaunchVelocityX;

    private int _facingDirection = 1;

    private float _chargeTimer;
    private float _jumpBufferTimer;
    private float _slamAnticipationTimer;
    private float _jumpDetachIgnoreTimer;
    private float _walkDustTimer;

    private float _cachedTimeScale = 1f;
    private float _cachedFixedDeltaTime = 0.02f;

    private float _slamStartY;
    private float _lastSlamFallDistance;
    private float _lastSlamImpactRadius;

    private Vector3 _visualBaseLocalScale = Vector3.one;
    private Vector3 _visualBaseLocalEulerAngles = Vector3.zero;

    private Tween _slamVisualTween;
    private Coroutine _slamLandHitStopCoroutine;

    #endregion

    #region Properties

    private InputAction MoveAction => _moveActionReference != null ? _moveActionReference.action : null;
    private InputAction JumpAction => _jumpActionReference != null ? _jumpActionReference.action : null;
    private bool JumpHeld => JumpAction != null && JumpAction.IsPressed();

    private bool IsMoveLocked => _isCharging || _isSlamAnticipating || _isSlamming;

    private bool IsWalking =>
        _isGrounded &&
        !IsMoveLocked &&
        Mathf.Abs(_moveInputX) > _inputDeadZone;

    #endregion

    #region Unity Lifecycle

    // Rigidbody 자동 참조를 세팅한다.
    private void Reset() => _rb = GetComponent<Rigidbody2D>();

    // 필요한 참조와 기본 시각 상태를 초기화한다.
    private void Awake()
    {
        _rb ??= GetComponent<Rigidbody2D>();
        _spriteRenderer ??= GetComponentInChildren<SpriteRenderer>();
        _gorillaWalkBounce ??= GetComponentInChildren<DebugGorillaWalkBounce>();

        if (_visualRoot == null && _spriteRenderer != null)
            _visualRoot = _spriteRenderer.transform;

        if (_visualRoot != null)
        {
            _visualBaseLocalScale = _visualRoot.localScale;
            _visualBaseLocalEulerAngles = _visualRoot.localEulerAngles;
        }

        _cachedTimeScale = Time.timeScale;
        _cachedFixedDeltaTime = Time.fixedDeltaTime;
    }

    // 시작 시 바닥 상태와 비주얼을 동기화한다.
    private void Start()
    {
        _isGrounded = CheckGrounded();
        _wasGrounded = _isGrounded;
        RefreshVisualState();
    }

    // 입력과 런타임 상태를 활성화한다.
    private void OnEnable()
    {
        MoveAction?.Enable();
        JumpAction?.Enable();

        _canSlam = true;
        _walkDustTimer = 0f;
        _wasCurrentJumpCharged = false;
        _slamStartedFromChargedJump = false;
        _slamStartY = transform.position.y;
        _lastSlamFallDistance = 0f;
        _lastSlamImpactRadius = 0f;

        ResetJumpLaunchMomentum();
        _gorillaWalkBounce?.SetWalking(false);
    }

    // 입력과 임시 연출 상태를 정리한다.
    private void OnDisable()
    {
        MoveAction?.Disable();
        JumpAction?.Disable();

        _gorillaWalkBounce?.SetWalking(false);

        KillSlamVisualTween();
        ResetVisualPoseImmediate();
        StopWalkDust();
        CancelSlamLandHitStop();
    }

    // 입력, 타이머, 비주얼을 프레임 단위로 갱신한다.
    private void Update()
    {
        ReadMoveInput();
        UpdateFacingDirection();
        UpdateTimers();
        HandleJumpInput();

        RefreshVisualState();
        UpdateWalkDust();
    }

    // 물리 상태와 이동 로직을 고정 프레임 단위로 갱신한다.
    private void FixedUpdate()
    {
        UpdateGroundedState();
        HandleGroundedStateTransitions();

        if (_isCharging)
        {
            ApplyChargeLock();
            ApplyGravityScale();
            RefreshVisualState();
            return;
        }

        if (_isSlamAnticipating)
        {
            ApplySlamAnticipationLock();
            ApplyGravityScale();
            RefreshVisualState();
            return;
        }

        if (_isSlamming)
        {
            ApplySlamMotion();
            ApplyGravityScale();
            RefreshVisualState();
            return;
        }

        TryConsumeBufferedJumpOnGround();
        ApplyHorizontalMovement();
        ApplyGravityScale();
        ClampGroundedVerticalVelocity();

        RefreshVisualState();
    }

    #endregion

    #region Input / Timers

    // 좌우 이동 입력값을 읽는다.
    private void ReadMoveInput()
    {
        if (MoveAction == null)
        {
            _moveInputX = 0f;
            return;
        }

        float x = MoveAction.ReadValue<Vector2>().x;
        _moveInputX = Mathf.Abs(x) >= _inputDeadZone ? x : 0f;
    }

    // 현재 입력을 기준으로 바라보는 방향을 갱신한다.
    private void UpdateFacingDirection()
    {
        if (!_isGrounded)
            return;

        if (_isCharging || _isSlamAnticipating)
            return;

        if (_moveInputX > _inputDeadZone) _facingDirection = 1;
        else if (_moveInputX < -_inputDeadZone) _facingDirection = -1;
    }

    // 각종 입력 버퍼와 상태 타이머를 감소시킨다.
    private void UpdateTimers()
    {
        if (_jumpBufferTimer > 0f) _jumpBufferTimer -= Time.deltaTime;
        if (_jumpDetachIgnoreTimer > 0f) _jumpDetachIgnoreTimer -= Time.deltaTime;

        if (_isCharging && JumpHeld)
            _chargeTimer = Mathf.Min(_chargeTimer + Time.deltaTime, _maxChargeTime);

        if (_isSlamAnticipating)
        {
            _slamAnticipationTimer -= Time.deltaTime;
            if (_slamAnticipationTimer <= 0f)
                BeginSlamDive();
        }
    }

    // 점프 입력에 따라 차지 점프, 슬램, 버퍼 점프를 처리한다.
    private void HandleJumpInput()
    {
        if (JumpAction == null)
            return;

        bool pressed = JumpAction.WasPressedThisFrame();
        bool released = JumpAction.WasReleasedThisFrame();

        if (pressed)
        {
            if (_isGrounded && !IsMoveLocked)
            {
                BeginCharge();
                return;
            }

            if (!_isGrounded)
            {
                if (IsNearGroundForBufferedLandingJump())
                {
                    _jumpBufferTimer = _jumpBufferTime;

                    if (_showDebugLog)
                        Debug.Log("Pre-land Buffered Jump");

                    return;
                }

                if (!_isCharging && !_isSlamAnticipating && !_isSlamming && _canSlam)
                {
                    StartSlamAnticipation();
                    return;
                }

                _jumpBufferTimer = _jumpBufferTime;
                return;
            }
        }

        if (_isCharging && released)
            ReleaseChargeJump();
    }

    // 착지 직전인지 검사해서 점프 버퍼 우선 여부를 판단한다.
    private bool IsNearGroundForBufferedLandingJump()
    {
        if (_isGrounded || _groundCheck == null || _rb == null)
            return false;

        if (_preLandBufferCheckDistance <= 0f)
            return false;

        if (_rb.linearVelocity.y > 0f)
            return false;

        return Physics2D.BoxCast(
            _groundCheck.position,
            _groundCheckSize,
            0f,
            Vector2.down,
            _preLandBufferCheckDistance,
            _groundLayer).collider != null;
    }

    #endregion

    #region Ground Check / Landing

    // 현재 바닥 상태를 갱신한다.
    private void UpdateGroundedState()
    {
        _wasGrounded = _isGrounded;
        _isGrounded = CheckGrounded();
    }

    // 발밑 오버랩 박스로 바닥 접지를 판정한다.
    private bool CheckGrounded()
    {
        if (_groundCheck == null)
            return false;

        if (_jumpDetachIgnoreTimer > 0f && _rb != null && _rb.linearVelocity.y >= 0f)
            return false;

        return Physics2D.OverlapBox(_groundCheck.position, _groundCheckSize, 0f, _groundLayer) != null;
    }

    // 이륙과 착지 전환 시점의 상태 변화를 처리한다.
    private void HandleGroundedStateTransitions()
    {
        bool justLanded = !_wasGrounded && _isGrounded && _rb.linearVelocity.y <= 0.01f;
        bool justLeftGround = _wasGrounded && !_isGrounded;
        bool landedFromSlam = justLanded && _isSlamming;

        bool shouldPlaySlamLandImpact =
            landedFromSlam &&
            (!_slamLandEffectOnlyAfterChargedJump || _slamStartedFromChargedJump);

        if (_showDebugLog)
        {
            Debug.Log(
                $"Landing Check | justLanded: {justLanded}, " +
                $"isSlamming: {_isSlamming}, " +
                $"landedFromSlam: {landedFromSlam}, " +
                $"wasCurrentJumpCharged: {_wasCurrentJumpCharged}, " +
                $"slamStartedFromChargedJump: {_slamStartedFromChargedJump}, " +
                $"shouldPlaySlamLandImpact: {shouldPlaySlamLandImpact}, " +
                $"velocityY: {_rb.linearVelocity.y}");
        }

        if (landedFromSlam)
            TryApplySlamDamageToTargets();

        if (shouldPlaySlamLandImpact)
        {
            PlaySlamLandImpact();
            StartSlamLandHitStop();
        }

        if (justLanded)
            OnLanded();

        if (justLeftGround && _showDebugLog)
            Debug.Log("Left Ground");

        if (_isCharging && !_isGrounded)
        {
            _isCharging = false;
            if (_showDebugLog)
                Debug.Log("Charge canceled because grounded state was lost.");
        }
    }

    // 착지 시 공중 행동 상태를 초기화한다.
    private void OnLanded()
    {
        _isSlamAnticipating = false;
        _isSlamming = false;
        _slamAnticipationTimer = 0f;
        _canSlam = true;
        _wasCurrentJumpCharged = false;
        _slamStartedFromChargedJump = false;

        ResetJumpLaunchMomentum();
        ResetVisualPoseImmediate();

        if (_showDebugLog)
            Debug.Log("Landed");
    }

    #endregion

    #region Jump / Slam Actions

    // 지상에서 차지 점프 준비를 시작한다.
    private void BeginCharge()
    {
        _isCharging = true;
        _isSlamAnticipating = false;
        _isSlamming = false;

        _chargeTimer = 0f;
        _jumpBufferTimer = 0f;

        if (_moveInputX > _inputDeadZone) _facingDirection = 1;
        else if (_moveInputX < -_inputDeadZone) _facingDirection = -1;

        SetVelocity(Vector2.zero);

        if (_showDebugLog)
            Debug.Log("Begin Charge");
    }

    // 차지 시간을 바탕으로 점프를 실제 발사한다.
    private void ReleaseChargeJump()
    {
        float chargeTime = _chargeTimer;

        bool isChargedJump = chargeTime >= _chargeThreshold;
        float ratio = isChargedJump
            ? Mathf.InverseLerp(_chargeThreshold, _maxChargeTime, chargeTime)
            : 0f;

        float jumpForceY = isChargedJump
            ? Mathf.Lerp(_normalJumpForceY, _maxChargeJumpForceY, ratio)
            : _normalJumpForceY;

        float jumpForceX = isChargedJump
            ? Mathf.Lerp(_normalJumpForceX, _maxChargeJumpForceX, ratio)
            : _normalJumpForceX;

        LaunchJump(jumpForceX, jumpForceY, isChargedJump);

        if (_showDebugLog)
        {
            Debug.Log(isChargedJump
                ? $"Charged Jump | ChargeTime: {chargeTime:F2}, Ratio: {ratio:F2}"
                : $"Normal Jump | TapTime: {chargeTime:F2}");
        }
    }

    // 공중에서 슬램 예고 상태에 진입한다.
    private void StartSlamAnticipation()
    {
        _isCharging = false;
        _isSlamAnticipating = true;
        _isSlamming = false;
        _canSlam = false;

        _jumpBufferTimer = 0f;
        _slamAnticipationTimer = _slamAnticipationDuration;
        _slamStartedFromChargedJump = _wasCurrentJumpCharged;
        _slamStartY = transform.position.y;

        ResetJumpLaunchMomentum();
        SetVelocity(Vector2.zero);
        PlaySlamAnticipationVisual();

        if (_showDebugLog)
            Debug.Log($"Start Slam Anticipation | fromChargedJump: {_slamStartedFromChargedJump}, slamStartY: {_slamStartY:F2}");
    }

    // 슬램 예고가 끝나면 실제 급강하를 시작한다.
    private void BeginSlamDive()
    {
        if (!_isSlamAnticipating)
            return;

        _isSlamAnticipating = false;
        _isSlamming = true;
        _slamAnticipationTimer = 0f;

        PlaySlamDiveVisual();
        SetVelocity(new Vector2(0f, -_slamSpeed));

        if (_showDebugLog)
            Debug.Log("Start Slam Dive");
    }

    // 점프 발사 시 x/y 운동량을 함께 적용한다.
    private void LaunchJump(float jumpForceX, float jumpForceY, bool isChargedJump)
    {
        _isCharging = false;
        _canSlam = true;
        _jumpBufferTimer = 0f;
        _wasCurrentJumpCharged = isChargedJump;
        _jumpDetachIgnoreTimer = _jumpDetachIgnoreTime;

        float launchVelocityX = _facingDirection * jumpForceX;

        if (_useJumpLaunchMomentum)
        {
            _jumpLaunchVelocityX = launchVelocityX;
            _hasJumpLaunchMomentum = true;
        }
        else
        {
            _jumpLaunchVelocityX = 0f;
            _hasJumpLaunchMomentum = false;
        }

        _chargeTimer = 0f;
        SetVelocity(new Vector2(launchVelocityX, jumpForceY));
    }

    // 점프 발사 운동량을 초기화한다.
    private void ResetJumpLaunchMomentum()
    {
        _hasJumpLaunchMomentum = false;
        _jumpLaunchVelocityX = 0f;
    }

    // 버퍼된 점프 입력을 지상에서 소모한다.
    private void TryConsumeBufferedJumpOnGround()
    {
        if (!_isGrounded || IsMoveLocked || _jumpBufferTimer <= 0f)
            return;

        _jumpBufferTimer = 0f;

        if (JumpHeld)
        {
            BeginCharge();
            return;
        }

        LaunchJump(_normalJumpForceX, _normalJumpForceY, false);

        if (_showDebugLog)
            Debug.Log("Buffered Normal Jump");
    }

    #endregion

    #region Slam Impact Damage

    // 슬램 낙하 높이를 바탕으로 범위와 데미지를 계산해 적에게 적용한다.
    private void TryApplySlamDamageToTargets()
    {
        Vector2 impactPoint = GetSlamImpactPoint();
        float fallDistance = GetCurrentSlamFallDistance(impactPoint.y);
        float impactRadius = GetSlamImpactRadius(fallDistance);
        float damage = GetSlamDamage(fallDistance);

        _lastSlamFallDistance = fallDistance;
        _lastSlamImpactRadius = impactRadius;

        Collider2D[] hits = Physics2D.OverlapCircleAll(impactPoint, impactRadius, _slamDamageLayer);
        if (hits == null || hits.Length == 0)
        {
            if (_showDebugLog)
                Debug.Log($"Slam Impact | No Target | fallDistance: {fallDistance:F2}, radius: {impactRadius:F2}, damage: {damage:F2}");
            return;
        }

        
    }

    // 슬램 충돌 중심점을 반환한다.
    private Vector2 GetSlamImpactPoint()
    {
        if (_groundCheck != null)
            return _groundCheck.position;

        return transform.position;
    }

    // 현재 슬램의 실제 낙하 높이를 계산한다.
    private float GetCurrentSlamFallDistance(float impactY)
    {
        return Mathf.Max(0f, _slamStartY - impactY);
    }

    // 낙하 높이에 비례한 충돌 반경을 계산한다.
    private float GetSlamImpactRadius(float fallDistance)
    {
        float radius = _slamBaseImpactRadius + (fallDistance * _slamImpactRadiusPerHeight);
        return Mathf.Clamp(radius, _slamBaseImpactRadius, _slamMaxImpactRadius);
    }

    // 낙하 높이에 비례한 슬램 데미지를 계산한다.
    private float GetSlamDamage(float fallDistance)
    {
        float damage = _slamBaseDamage + (fallDistance * _slamDamagePerHeight);
        return Mathf.Clamp(damage, _slamBaseDamage, _slamMaxDamage);
    }

    #endregion

    #region Physics Movement

    // 현재 상태에 맞는 수평 이동 속도를 적용한다.
    private void ApplyHorizontalMovement()
    {
        Vector2 velocity = _rb.linearVelocity;

        if (_isGrounded)
        {
            velocity.x = _moveInputX * _groundMoveSpeed;
            _rb.linearVelocity = velocity;
            return;
        }

        if (_hasJumpLaunchMomentum)
        {
            if (_launchMomentumDecay > 0f)
            {
                _jumpLaunchVelocityX = Mathf.MoveTowards(
                    _jumpLaunchVelocityX,
                    0f,
                    _launchMomentumDecay * Time.fixedDeltaTime);

                if (Mathf.Abs(_jumpLaunchVelocityX) <= 0.01f)
                {
                    _jumpLaunchVelocityX = 0f;
                    _hasJumpLaunchMomentum = false;
                }
            }

            float airControlVelocityX = _moveInputX * _airMoveSpeed * _airControlPercent;
            velocity.x = _jumpLaunchVelocityX + airControlVelocityX;
        }
        else
        {
            velocity.x = _moveInputX * _airMoveSpeed;
        }

        _rb.linearVelocity = velocity;
    }

    // 차지 중에는 완전히 정지시킨다.
    private void ApplyChargeLock() => SetVelocity(Vector2.zero);

    // 슬램 예고 중에는 완전히 정지시킨다.
    private void ApplySlamAnticipationLock() => SetVelocity(Vector2.zero);

    // 슬램 중에는 수직으로 강하게 하강시킨다.
    private void ApplySlamMotion() => SetVelocity(new Vector2(0f, -_slamSpeed));

    // 현재 상태에 맞는 중력 스케일을 설정한다.
    private void ApplyGravityScale()
    {
        if (_isCharging || _isSlamAnticipating)
        {
            _rb.gravityScale = 0f;
            return;
        }

        if (_isSlamming)
        {
            _rb.gravityScale = _slamGravityScale;
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
    private void ClampGroundedVerticalVelocity()
    {
        if (!_isGrounded || IsMoveLocked || _rb.linearVelocity.y >= 0f)
            return;

        Vector2 velocity = _rb.linearVelocity;
        velocity.y = 0f;
        _rb.linearVelocity = velocity;
    }

    // Rigidbody 속도를 직접 설정한다.
    private void SetVelocity(Vector2 velocity) => _rb.linearVelocity = velocity;

    #endregion

    #region Particles / Hit Stop

    // 걷기 상태일 때 주기적으로 먼지 파티클을 재생한다.
    private void UpdateWalkDust()
    {
        if (_walkDustParticle == null)
            return;

        if (!IsWalking)
        {
            _walkDustTimer = 0f;
            return;
        }

        _walkDustTimer -= Time.deltaTime;
        if (_walkDustTimer > 0f)
            return;

        _walkDustTimer = _walkDustInterval;
        _walkDustParticle.Play();
    }

    // 걷기 먼지 파티클을 즉시 정지하고 비운다.
    private void StopWalkDust()
    {
        if (_walkDustParticle == null)
            return;

        _walkDustParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    // 슬램 착지 이펙트를 바닥 위치에서 재생한다.
    private void PlaySlamLandImpact()
    {
        if (_slamLandImpactParticle == null)
            return;

        Transform fxTransform = _slamLandImpactParticle.transform;
        fxTransform.position = _groundCheck != null ? _groundCheck.position : transform.position;

        _slamLandImpactParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        _slamLandImpactParticle.Play();

        _cameraController?.PlaySlamLandShake();
    }

    // 슬램 착지 시 짧은 히트스탑을 시작한다.
    private void StartSlamLandHitStop()
    {
        if (!_useSlamLandHitStop || _slamLandHitStopDuration <= 0f)
            return;

        CancelSlamLandHitStop();

        _cachedTimeScale = Time.timeScale;
        _cachedFixedDeltaTime = Time.fixedDeltaTime;
        _slamLandHitStopCoroutine = StartCoroutine(CoSlamLandHitStop());
    }

    // 히트스탑 동안 시간을 멈췄다가 복구한다.
    private IEnumerator CoSlamLandHitStop()
    {
        Time.timeScale = 0f;
        Time.fixedDeltaTime = 0f;

        yield return new WaitForSecondsRealtime(_slamLandHitStopDuration);

        Time.timeScale = _cachedTimeScale;
        Time.fixedDeltaTime = _cachedFixedDeltaTime;
        _slamLandHitStopCoroutine = null;
    }

    // 진행 중인 히트스탑을 취소하고 시간을 복구한다.
    private void CancelSlamLandHitStop()
    {
        if (_slamLandHitStopCoroutine != null)
        {
            StopCoroutine(_slamLandHitStopCoroutine);
            _slamLandHitStopCoroutine = null;
        }

        Time.timeScale = _cachedTimeScale;
        Time.fixedDeltaTime = _cachedFixedDeltaTime;
    }

    #endregion

    #region Visuals

    // 현재 상태에 맞게 스프라이트와 보행 비주얼을 갱신한다.
    private void RefreshVisualState()
    {
        UpdateSpriteByState();
        UpdateWalkVisual();
    }

    // 현재 상태에 따라 표시할 스프라이트를 선택한다.
    private void UpdateSpriteByState()
    {
        if (_spriteRenderer == null)
            return;

        Sprite targetSprite = _defaultSprite;

        if (_isSlamming)
            targetSprite = _slamSprite ?? _jumpSprite ?? _defaultSprite;
        else if (!_isGrounded)
            targetSprite = _jumpSprite ?? _defaultSprite;
        else
            targetSprite = _defaultSprite;

        if (_spriteRenderer.sprite != targetSprite)
            _spriteRenderer.sprite = targetSprite;
    }

    // 걷기 바운스와 좌우 반전을 갱신한다.
    private void UpdateWalkVisual()
    {
        if (_gorillaWalkBounce != null)
        {
            _gorillaWalkBounce.SetFacing(_facingDirection);
            _gorillaWalkBounce.SetWalking(IsWalking);
            return;
        }

        if (_spriteRenderer != null)
            _spriteRenderer.flipX = _facingDirection < 0;
    }

    // 슬램 예고 포즈의 스케일/회전 트윈을 재생한다.
    private void PlaySlamAnticipationVisual()
    {
        if (_visualRoot == null)
            return;

        KillSlamVisualTween();
        ResetVisualPose();

        float signedRotateZ = _facingDirection >= 0
            ? -Mathf.Abs(_slamAnticipationRotateZ)
            : Mathf.Abs(_slamAnticipationRotateZ);

        Vector3 targetScale = new(
            _visualBaseLocalScale.x * _slamAnticipationScaleX,
            _visualBaseLocalScale.y * _slamAnticipationScaleY,
            _visualBaseLocalScale.z);

        Vector3 targetEuler = new(
            _visualBaseLocalEulerAngles.x,
            _visualBaseLocalEulerAngles.y,
            _visualBaseLocalEulerAngles.z + signedRotateZ);

        _slamVisualTween = DOTween.Sequence()
            .Append(_visualRoot.DOScale(targetScale, _slamAnticipationDuration).SetEase(Ease.OutQuad))
            .Join(_visualRoot.DOLocalRotate(targetEuler, _slamAnticipationDuration).SetEase(Ease.OutQuad));
    }

    // 슬램 돌입 순간의 늘어나는 연출을 재생한다.
    private void PlaySlamDiveVisual()
    {
        if (_visualRoot == null)
            return;

        KillSlamVisualTween();

        Vector3 stretchScale = new(
            _visualBaseLocalScale.x * _slamDiveStretchScaleX,
            _visualBaseLocalScale.y * _slamDiveStretchScaleY,
            _visualBaseLocalScale.z);

        float stretchIn = _slamDiveStretchDuration * 0.4f;
        float stretchOut = _slamDiveStretchDuration * 0.6f;

        _slamVisualTween = DOTween.Sequence()
            .Append(_visualRoot.DOScale(stretchScale, stretchIn).SetEase(Ease.OutQuad))
            .Join(_visualRoot.DOLocalRotate(_visualBaseLocalEulerAngles, stretchIn).SetEase(Ease.OutQuad))
            .Append(_visualRoot.DOScale(_visualBaseLocalScale, stretchOut).SetEase(Ease.InQuad))
            .Join(_visualRoot.DOLocalRotate(_visualBaseLocalEulerAngles, stretchOut).SetEase(Ease.InQuad));
    }

    // 비주얼 루트를 기본 포즈로 즉시 복구한다.
    private void ResetVisualPoseImmediate()
    {
        if (_visualRoot == null)
            return;

        KillSlamVisualTween();
        ResetVisualPose();
    }

    // 비주얼 루트를 저장된 기본 포즈로 되돌린다.
    private void ResetVisualPose()
    {
        _visualRoot.localScale = _visualBaseLocalScale;
        _visualRoot.localEulerAngles = _visualBaseLocalEulerAngles;
    }

    // 실행 중인 슬램 비주얼 트윈을 종료한다.
    private void KillSlamVisualTween()
    {
        if (_slamVisualTween != null && _slamVisualTween.IsActive())
            _slamVisualTween.Kill();

        _slamVisualTween = null;
    }

    #endregion

    #region Gizmos

    // 에디터에서 바닥 체크 박스를 시각화한다.
    private void OnDrawGizmosSelected()
    {
        if (_groundCheck == null)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_groundCheck.position, _groundCheckSize);

        if (_lastSlamImpactRadius > 0f)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_groundCheck.position, _lastSlamImpactRadius);
        }
    }

    #endregion
}
