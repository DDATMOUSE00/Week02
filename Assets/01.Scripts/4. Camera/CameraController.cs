using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform _target;
    [SerializeField] private Rigidbody2D _targetRb;

    [Header("Follow")]
    [SerializeField] private Vector3 _followOffset = new Vector3(0f, 0f, 0f);
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
    [SerializeField] private float _ySmoothTimeRising = 0.16f;
    [SerializeField] private float _ySmoothTimeFalling = 0.08f;
    [SerializeField] private float _ySmoothTimeGrounded = 0.10f;
    [SerializeField] private float _xMaxSpeed = 100f;
    [SerializeField] private float _yMaxSpeed = 100f;

    [Header("Zoom Out (Optional)")]
    [SerializeField] private bool _useDynamicZoom = true;

    [Tooltip("기본 카메라 size")]
    [SerializeField] private float _defaultSize = 5f;

    [Tooltip("최대 줌아웃 size")]
    [SerializeField] private float _maxZoomOutSize = 6.2f;

    [Tooltip("플레이어와 기준 바닥 Y 차이가 이 값 이상부터 줌아웃 시작")]
    [SerializeField] private float _zoomStartHeight = 3f;

    [Tooltip("플레이어와 기준 바닥 Y 차이가 이 값에 도달하면 최대 줌아웃")]
    [SerializeField] private float _zoomFullHeight = 8f;

    [SerializeField] private float _zoomSmoothTime = 0.12f;

    [Header("Slam Land Shake")]
    [SerializeField] private bool _useSlamLandShake = true;
    [SerializeField] private float _slamShakeDuration = 0.18f;
    [SerializeField] private float _slamShakeStrengthX = 0.18f;
    [SerializeField] private float _slamShakeLiftY = 0.08f;
    [SerializeField] private float _slamShakeOscillationCount = 4f;

    [Header("Debug")]
    [SerializeField] private bool _drawGuide = true;

    private Camera _camera;
    private float _fixedZ;

    private float _xVelocity;
    private float _yVelocity;
    private float _zoomVelocity;

    private float _slamShakeTimer;
    private Vector3 _slamShakeOffset;

    private void Reset()
    {
        _camera = GetComponent<Camera>();
    }

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        _fixedZ = transform.position.z;

        // 2D 게임 기준으로 Orthographic 카메라를 전제로 사용
        if (_camera != null)
            _camera.orthographicSize = _defaultSize;
    }

    private void OnDisable()
    {
        _slamShakeTimer = 0f;
        _slamShakeOffset = Vector3.zero;
    }

    private void LateUpdate()
    {
        if (_target == null || _camera == null)
            return;

        UpdateSlamShake();
        UpdateFollow();
        UpdateZoom();
    }

    private void UpdateFollow()
    {
        // 흔들림 오프셋이 포함된 현재 위치를 그대로 쓰면
        // SmoothDamp 추적값이 오염되므로, 기존 shake를 뺀 기준 좌표로 계산한다.
        Vector3 currentPosition = transform.position - _slamShakeOffset;

        float targetX = currentPosition.x;
        if (_followX)
            targetX = _target.position.x + _followOffset.x;

        float targetY = currentPosition.y;
        if (_followY)
            targetY = CalculateTargetY();

        // 상승 중에는 Y를 조금 더 느리게 따라가고,
        // 하강/슬램 중에는 더 빠르게 따라가서 바닥 정보를 빨리 보여준다.
        float currentYSmoothTime = GetCurrentYSmoothTime();

        float nextX = Mathf.SmoothDamp(
            currentPosition.x,
            targetX,
            ref _xVelocity,
            _xSmoothTime,
            _xMaxSpeed);

        float nextY = Mathf.SmoothDamp(
            currentPosition.y,
            targetY,
            ref _yVelocity,
            currentYSmoothTime,
            _yMaxSpeed);

        transform.position = new Vector3(nextX, nextY, _fixedZ) + _slamShakeOffset;
    }

    private float CalculateTargetY()
    {
        // 플레이어 높이에 오프셋 반영
        float playerY = _target.position.y + _followOffset.y;

        // 카메라 Y를 "기준 바닥 Y"와 "플레이어 Y" 사이에서 보간한다.
        // verticalFollowLerp가 낮을수록 플레이어를 끝까지 쫓지 않고
        // 아래 바닥 시야를 더 많이 남겨둔다.
        float blendedY = Mathf.Lerp(_groundReferenceY, playerY, _verticalFollowLerp);

        // 너무 높이 올라가서 화면이 위로 끌려가지 않도록 상승 한계를 둔다.
        float clampedY = Mathf.Min(blendedY, _groundReferenceY + _maxCameraRiseFromGround);

        return clampedY;
    }

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

    private void UpdateZoom()
    {
        float targetSize = _defaultSize;

        if (_useDynamicZoom)
        {
            // 플레이어와 기준 바닥의 Y 차이를 기준으로 약하게만 줌아웃
            float heightFromGround = Mathf.Max(0f, _target.position.y - _groundReferenceY);

            // InverseLerp로 시작 높이~최대 높이를 0~1 비율로 환산
            float zoomT = Mathf.InverseLerp(_zoomStartHeight, _zoomFullHeight, heightFromGround);

            targetSize = Mathf.Lerp(_defaultSize, _maxZoomOutSize, zoomT);
        }

        _camera.orthographicSize = Mathf.SmoothDamp(
            _camera.orthographicSize,
            targetSize,
            ref _zoomVelocity,
            _zoomSmoothTime);
    }

    private void UpdateSlamShake()
    {
        if (!_useSlamLandShake || _slamShakeDuration <= 0f)
        {
            _slamShakeTimer = 0f;
            _slamShakeOffset = Vector3.zero;
            return;
        }

        if (_slamShakeTimer <= 0f)
        {
            _slamShakeOffset = Vector3.zero;
            return;
        }

        _slamShakeTimer = Mathf.Max(0f, _slamShakeTimer - Time.deltaTime);

        float t = 1f - (_slamShakeTimer / _slamShakeDuration);
        float fade = 1f - t;

        float offsetX =
            Mathf.Sin(t * Mathf.PI * 2f * _slamShakeOscillationCount) *
            _slamShakeStrengthX *
            fade;

        float offsetY =
            Mathf.Sin(t * Mathf.PI) *
            _slamShakeLiftY;

        _slamShakeOffset = new Vector3(offsetX, offsetY, 0f);

        if (_slamShakeTimer <= 0f)
            _slamShakeOffset = Vector3.zero;
    }

    // 슬램 착지 순간 호출해서 덜컹하는 카메라 흔들림을 재생한다.
    public void PlaySlamLandShake()
    {
        if (!_useSlamLandShake || _slamShakeDuration <= 0f)
            return;

        _slamShakeTimer = _slamShakeDuration;
        _slamShakeOffset = Vector3.zero;
    }

    private void OnDrawGizmosSelected()
    {
        if (!_drawGuide)
            return;

        // 기준 바닥 Y 시각화
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(
            new Vector3(-100f, _groundReferenceY, 0f),
            new Vector3(100f, _groundReferenceY, 0f));

        if (_target == null)
            return;

        float playerY = _target.position.y + _followOffset.y;
        float targetY = Mathf.Lerp(_groundReferenceY, playerY, _verticalFollowLerp);
        targetY = Mathf.Min(targetY, _groundReferenceY + _maxCameraRiseFromGround);

        // 플레이어 위치와 카메라 목표 Y 보조선
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(
            new Vector3(_target.position.x, _groundReferenceY, 0f),
            new Vector3(_target.position.x, playerY, 0f));

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(new Vector3(_target.position.x, targetY, 0f), 0.12f);
    }
}