using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    #region Inspector

    [Tooltip("기본 카메라 size")]
    [SerializeField] private float _defaultSize = 5f;

    [Header("Target | Inspector 연결 필요")]
    [SerializeField] private Transform _target;
    [SerializeField] private Rigidbody2D _targetRb;

    [Header("Follow")]
    [SerializeField] private Vector3 _followOffset = Vector3.zero;
    [SerializeField] private bool _followX = true;
    [SerializeField] private bool _followY = true;

    [Header("Ground Reference Y")]
    [Tooltip("항상 보여주고 싶은 기준 바닥 Y값")]
    [SerializeField] private float _groundReferenceY = 0f;

    [Tooltip("카메라 Y를 groundReferenceY ~ playerY 사이 어디에 둘지 결정. 낮을수록 바닥을 더 많이 보여줌")]
    [Range(0f, 1f)]
    [SerializeField] private float _verticalFollowLerp = 0.35f;

    [Tooltip("카메라가 기준 바닥 Y로부터 최대 얼마나 올라갈 수 있는지 제한")]
    [SerializeField] private float _maxCameraRiseFromGround = 6f;

    [Header("Smooth")]
    [SerializeField] private float _xSmoothTime = 0.08f;
    [Space(10)]
    [SerializeField] private float _ySmoothTimeRising = 0.16f;
    [SerializeField] private float _ySmoothTimeFalling = 0.08f;
    [SerializeField] private float _ySmoothTimeGrounded = 0.10f;
    [Space(10)]
    [SerializeField] private float _xMaxSpeed = 100f;
    [SerializeField] private float _yMaxSpeed = 100f;

    [Header("Slam Land Shake")]
    [SerializeField] private bool _useSlamLandShake = true;

    [Tooltip("기본 최대 흔들림 지속 시간")]
    [SerializeField] private float _slamShakeDuration = 0.28f;

    [Tooltip("좌우 흔들림 최대 세기")]
    [SerializeField] private float _slamShakeStrengthX = 0.06f;

    [Tooltip("충격 순간 위로 붕 뜨는 최대 높이")]
    [SerializeField] private float _slamShakeLiftY = 0.22f;

    [Tooltip("잔진동 횟수. 낮을수록 무겁고 둔탁한 느낌")]
    [SerializeField] private float _slamShakeOscillationCount = 1.8f;

    [Tooltip("전체 흔들림 중 초기 충격 상승 구간 비율")]
    [Range(0.05f, 0.4f)]
    [SerializeField] private float _slamShakeKickPortion = 0.16f;

    [Tooltip("잔진동 감쇠 강도. 높을수록 빨리 멈춘다")]
    [SerializeField] private float _slamShakeDamping = 3.2f;

    [Header("Slam Land Roll")]
    [SerializeField] private bool _useSlamLandRoll = true;

    [Tooltip("최대 기울기 각도. 오른쪽 충격이면 음수, 왼쪽 충격이면 양수로 적용된다")]
    [SerializeField] private float _slamRollMaxAngle = 6f;

    [Tooltip("회전 감쇠 강도. 높을수록 빨리 원래 각도로 돌아온다")]
    [SerializeField] private float _slamRollDamping = 3.6f;

    [Header("Debug")]
    [SerializeField] private bool _drawGuide = true;

    #endregion

    #region Runtime Fields

    private Camera _camera;
    private float _fixedZ;
    private Vector3 _baseEulerAngles;

    private float _xVelocity;
    private float _yVelocity;

    private float _slamShakeTimer;
    private Vector3 _slamShakeOffset;
    private float _slamRollOffsetZ;

    private float _currentShakeDuration;
    private float _currentShakeStrengthX;
    private float _currentShakeLiftY;
    private float _currentShakeOscillationCount;

    private float _currentRollMaxAngle;
    private float _currentRollSign;
    private float _currentRollDamping;

    #endregion

    #region Unity Lifecycle

    // 같은 오브젝트의 Camera를 자동 참조한다.
    private void Reset()
    {
        _camera = GetComponent<Camera>();
    }

    // 카메라 참조와 기본 상태를 초기화한다.
    private void Awake()
    {
        _camera = GetComponent<Camera>();
        _fixedZ = transform.position.z;
        _baseEulerAngles = transform.eulerAngles;

        if (_camera != null)
            _camera.orthographicSize = _defaultSize;

        ResetShakeState();
        ApplyRotationOffset();
    }

    // 비활성화 시 흔들림과 회전 상태를 정리한다.
    private void OnDisable()
    {
        ResetShakeState();
        ApplyRotationOffset();
    }

    // 추적과 흔들림을 최종 위치와 회전에 반영한다.
    private void LateUpdate()
    {
        if (_target == null || _camera == null)
            return;

        UpdateSlamShake();
        UpdateFollow();
        ApplyRotationOffset();
    }

    #endregion

    #region Follow

    // 현재 타겟을 따라 카메라 위치를 갱신한다.
    private void UpdateFollow()
    {
        Vector3 basePosition = GetShakeRemovedCurrentPosition();

        float targetX = CalculateTargetX(basePosition.x);
        float targetY = CalculateTargetY(basePosition.y);

        float nextX = Mathf.SmoothDamp(
            basePosition.x,
            targetX,
            ref _xVelocity,
            _xSmoothTime,
            _xMaxSpeed);

        float nextY = Mathf.SmoothDamp(
            basePosition.y,
            targetY,
            ref _yVelocity,
            GetCurrentYSmoothTime(),
            _yMaxSpeed);

        transform.position = new Vector3(nextX, nextY, _fixedZ) + _slamShakeOffset;
    }

    // 흔들림 오프셋을 제외한 현재 기준 좌표를 반환한다.
    private Vector3 GetShakeRemovedCurrentPosition()
    {
        return transform.position - _slamShakeOffset;
    }

    // X축 목표 좌표를 계산한다.
    private float CalculateTargetX(float fallbackX)
    {
        if (!_followX)
            return fallbackX;

        return _target.position.x + _followOffset.x;
    }

    // Y축 목표 좌표를 계산한다.
    private float CalculateTargetY(float fallbackY)
    {
        if (!_followY)
            return fallbackY;

        float playerY = _target.position.y + _followOffset.y;
        float blendedY = Mathf.Lerp(_groundReferenceY, playerY, _verticalFollowLerp);
        return Mathf.Min(blendedY, _groundReferenceY + _maxCameraRiseFromGround);
    }

    // 플레이어의 수직 속도에 따라 Y축 추적 스무딩을 결정한다.
    private float GetCurrentYSmoothTime()
    {
        if (_targetRb == null)
            return _ySmoothTimeGrounded;

        float vy = _targetRb.linearVelocity.y;

        if (vy > 0.05f)
            return _ySmoothTimeRising;

        if (vy < -0.05f)
            return _ySmoothTimeFalling;

        return _ySmoothTimeGrounded;
    }

    #endregion

    #region Slam Shake / Roll

    // 슬램 흔들림과 회전 상태를 매 프레임 갱신한다.
    private void UpdateSlamShake()
    {
        if (!_useSlamLandShake || _currentShakeDuration <= 0f)
        {
            ResetShakeState();
            return;
        }

        if (_slamShakeTimer <= 0f)
        {
            _slamShakeOffset = Vector3.zero;
            _slamRollOffsetZ = 0f;
            return;
        }

        _slamShakeTimer = Mathf.Max(0f, _slamShakeTimer - Time.deltaTime);

        float elapsedNormalized = 1f - (_slamShakeTimer / _currentShakeDuration);

        _slamShakeOffset = EvaluateHeavyImpactOffset(elapsedNormalized);
        _slamRollOffsetZ = EvaluateHeavyImpactRoll(elapsedNormalized);

        if (_slamShakeTimer <= 0f)
        {
            _slamShakeOffset = Vector3.zero;
            _slamRollOffsetZ = 0f;
        }
    }

    // 무거운 충돌 느낌의 위치 오프셋을 계산한다.
    private Vector3 EvaluateHeavyImpactOffset(float elapsedNormalized)
    {
        float kickPortion = Mathf.Clamp01(_slamShakeKickPortion);

        if (elapsedNormalized <= kickPortion)
        {
            float kickT = kickPortion <= 0f ? 1f : elapsedNormalized / kickPortion;
            float lift = Mathf.Lerp(0f, _currentShakeLiftY, EaseOutCubic(kickT));
            return new Vector3(0f, lift, 0f);
        }

        float settleT = (elapsedNormalized - kickPortion) / Mathf.Max(0.0001f, 1f - kickPortion);
        float damping = Mathf.Exp(-_slamShakeDamping * settleT);
        float wave = settleT * Mathf.PI * 2f * _currentShakeOscillationCount;

        float offsetY = _currentShakeLiftY * damping * Mathf.Cos(wave);
        float offsetX = _currentShakeStrengthX * damping * Mathf.Sin(wave);

        return new Vector3(offsetX, offsetY, 0f);
    }

    // 무거운 충돌 느낌의 Z 회전값을 계산한다.
    private float EvaluateHeavyImpactRoll(float elapsedNormalized)
    {
        if (!_useSlamLandRoll || Mathf.Approximately(_currentRollSign, 0f))
            return 0f;

        float kickPortion = Mathf.Clamp01(_slamShakeKickPortion);

        if (elapsedNormalized <= kickPortion)
        {
            float kickT = kickPortion <= 0f ? 1f : elapsedNormalized / kickPortion;
            return Mathf.Lerp(0f, _currentRollMaxAngle * _currentRollSign, EaseOutCubic(kickT));
        }

        float settleT = (elapsedNormalized - kickPortion) / Mathf.Max(0.0001f, 1f - kickPortion);
        float damping = Mathf.Exp(-_currentRollDamping * settleT);
        float wave = settleT * Mathf.PI * 2f * _currentShakeOscillationCount;

        return _currentRollMaxAngle * _currentRollSign * damping * Mathf.Cos(wave);
    }

    // 현재 회전 오프셋을 카메라에 적용한다.
    private void ApplyRotationOffset()
    {
        transform.rotation = Quaternion.Euler(
            _baseEulerAngles.x,
            _baseEulerAngles.y,
            _baseEulerAngles.z + _slamRollOffsetZ);
    }

    // cubic ease-out 값을 반환한다.
    private float EaseOutCubic(float t)
    {
        t = Mathf.Clamp01(t);
        float inv = 1f - t;
        return 1f - (inv * inv * inv);
    }

    // 슬램 착지 흔들림을 최대 세기로 재생한다.
    public void PlaySlamLandShake()
    {
        PlaySlamLandShake(1f, transform.position.x);
    }

    // 슬램 비율만 반영해서 흔들림을 재생한다.
    public void PlaySlamLandShake(float slamRatio)
    {
        PlaySlamLandShake(slamRatio, transform.position.x);
    }

    // 슬램 비율과 충격 위치를 반영해 흔들림과 회전을 재생한다.
    public void PlaySlamLandShake(float slamRatio, float impactWorldX)
    {
        if (!_useSlamLandShake || _slamShakeDuration <= 0f)
            return;

        float clampedRatio = Mathf.Clamp01(slamRatio);
        if (clampedRatio <= 0f)
            return;

        float sideDelta = impactWorldX - transform.position.x;
        float rollSign = 0f;

        if (sideDelta > 0.001f)
            rollSign = -1f;
        else if (sideDelta < -0.001f)
            rollSign = 1f;

        _currentShakeDuration = _slamShakeDuration * clampedRatio;
        _currentShakeStrengthX = _slamShakeStrengthX * clampedRatio;
        _currentShakeLiftY = _slamShakeLiftY * clampedRatio;
        _currentShakeOscillationCount = _slamShakeOscillationCount;

        _currentRollMaxAngle = _slamRollMaxAngle * clampedRatio;
        _currentRollSign = rollSign;
        _currentRollDamping = _slamRollDamping;

        _slamShakeTimer = _currentShakeDuration;
        _slamShakeOffset = Vector3.zero;
        _slamRollOffsetZ = 0f;
    }

    // 흔들림 관련 런타임 상태를 초기화한다.
    private void ResetShakeState()
    {
        _slamShakeTimer = 0f;
        _slamShakeOffset = Vector3.zero;
        _slamRollOffsetZ = 0f;

        _currentShakeDuration = 0f;
        _currentShakeStrengthX = 0f;
        _currentShakeLiftY = 0f;
        _currentShakeOscillationCount = 0f;

        _currentRollMaxAngle = 0f;
        _currentRollSign = 0f;
        _currentRollDamping = 0f;
    }

    #endregion

    #region Gizmos

    // 에디터에서 추적 기준선을 시각화한다.
    private void OnDrawGizmosSelected()
    {
        if (!_drawGuide)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(
            new Vector3(-100f, _groundReferenceY, 0f),
            new Vector3(100f, _groundReferenceY, 0f));

        if (_target == null)
            return;

        float playerY = _target.position.y + _followOffset.y;
        float targetY = Mathf.Lerp(_groundReferenceY, playerY, _verticalFollowLerp);
        targetY = Mathf.Min(targetY, _groundReferenceY + _maxCameraRiseFromGround);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(
            new Vector3(_target.position.x, _groundReferenceY, 0f),
            new Vector3(_target.position.x, playerY, 0f));

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(new Vector3(_target.position.x, targetY, 0f), 0.12f);
    }

    #endregion
}