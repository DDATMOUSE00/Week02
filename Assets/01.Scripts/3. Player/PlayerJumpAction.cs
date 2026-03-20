using UnityEngine;

public class PlayerJumpAction : MonoBehaviour
{
    #region Inspector

    [Header("Jump")]
    [SerializeField] private float _normalJumpForceY = 10f;
    [SerializeField] private float _normalJumpForceX = 4f;

    [Header("Charge Jump")]
    [SerializeField] private float _chargeThreshold = 0.12f;
    [SerializeField] private float _maxChargeTime = 0.6f;
    [SerializeField] private float _maxChargeJumpForceY = 16f;
    [SerializeField] private float _maxChargeJumpForceX = 8f;

    [Header("Charge Curve")]
    [SerializeField] private AnimationCurve _chargeForceCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Buffer")]
    [SerializeField] private float _jumpBufferTime = 0.12f;

    #endregion

    #region Runtime Fields

    private float _chargeTimer;
    private float _jumpBufferTimer;

    #endregion

    #region Properties

    public bool HasBufferedJump => _jumpBufferTimer > 0f;

    // 홀드 시간 기준의 0~1 차지량이다. 슬라이더 표시용으로 사용한다.
    public float ChargeHoldNormalized => GetChargeHoldNormalized(_chargeTimer);

    // 실제 점프 힘 계산에 쓰이는 0~1 비율이다. threshold 이후 curve가 적용된다.
    public float ChargeEffectiveRatio => EvaluateChargeForceRatio(_chargeTimer);

    #endregion

    #region Timers

    // 차지 타이머와 점프 버퍼 타이머를 갱신한다.
    public void TickTimers(bool isCharging, bool jumpHeld, float deltaTime)
    {
        if (_jumpBufferTimer > 0f)
            _jumpBufferTimer -= deltaTime;

        if (isCharging && jumpHeld)
            _chargeTimer = Mathf.Min(_chargeTimer + deltaTime, _maxChargeTime);
    }

    // 점프 버퍼를 설정한다.
    public void SetJumpBuffer()
    {
        _jumpBufferTimer = _jumpBufferTime;
    }

    // 점프 버퍼를 비운다.
    public void ClearJumpBuffer()
    {
        _jumpBufferTimer = 0f;
    }

    #endregion

    #region Charge Jump

    // 지상에서 차지 준비를 시작한다.
    public void BeginCharge(PlayerControllerVersionTwo controller, PlayerMovementMotor movement)
    {
        controller.EnterChargeState();

        _chargeTimer = 0f;
        _jumpBufferTimer = 0f;

        movement.FreezeBody();

        if (controller.ShowDebugLog)
            Debug.Log("Begin Charge");
    }

    // 차지 시간을 바탕으로 점프를 발사한다.
    public void ReleaseChargeJump(PlayerControllerVersionTwo controller, PlayerMovementMotor movement)
    {
        float chargeTime = _chargeTimer;
        bool isChargedJump = chargeTime >= _chargeThreshold;

        float effectiveRatio = EvaluateChargeForceRatio(chargeTime);

        float jumpForceY = isChargedJump
            ? Mathf.Lerp(_normalJumpForceY, _maxChargeJumpForceY, effectiveRatio)
            : _normalJumpForceY;

        float jumpForceX = isChargedJump
            ? Mathf.Lerp(_normalJumpForceX, _maxChargeJumpForceX, effectiveRatio)
            : _normalJumpForceX;

        LaunchJump(controller, movement, jumpForceX, jumpForceY);

        if (controller.ShowDebugLog)
        {
            Debug.Log(isChargedJump
                ? $"Charged Jump | ChargeTime: {chargeTime:F2}, EvaluatedRatio: {effectiveRatio:F2}"
                : $"Normal Jump | TapTime: {chargeTime:F2}");
        }
    }

    // 지상에서 버퍼된 점프 입력을 소모한다.
    public void TryConsumeBufferedJumpOnGround(PlayerControllerVersionTwo controller, PlayerMovementMotor movement, bool jumpHeld)
    {
        if (!controller.IsGrounded || controller.IsMoveLocked || _jumpBufferTimer <= 0f)
            return;

        _jumpBufferTimer = 0f;

        if (jumpHeld)
        {
            BeginCharge(controller, movement);
            return;
        }

        controller.LockFacingForAction();
        LaunchJump(controller, movement, _normalJumpForceX, _normalJumpForceY);

        if (controller.ShowDebugLog)
            Debug.Log("Buffered Normal Jump");
    }

    // 점프를 실제 발사한다.
    private void LaunchJump(PlayerControllerVersionTwo controller, PlayerMovementMotor movement, float jumpForceX, float jumpForceY)
    {
        controller.OnJumpLaunched();

        movement.StartJumpDetachIgnore();

        float launchVelocityX = controller.LockedActionFacingDirection * jumpForceX;
        movement.RegisterJumpLaunch(launchVelocityX);
        movement.SetVelocity(new Vector2(launchVelocityX, jumpForceY));

        _chargeTimer = 0f;
        _jumpBufferTimer = 0f;
    }

    #endregion

    #region Charge Evaluate

    // 홀드 시간을 0~1 범위의 차지량으로 변환한다.
    private float GetChargeHoldNormalized(float chargeTime)
    {
        if (_maxChargeTime <= 0f)
            return 0f;

        return Mathf.Clamp01(chargeTime / _maxChargeTime);
    }

    // threshold 이후의 차지 비율에 curve를 적용해 실제 힘 계산용 비율을 반환한다.
    private float EvaluateChargeForceRatio(float chargeTime)
    {
        if (chargeTime < _chargeThreshold)
            return 0f;

        if (_maxChargeTime <= _chargeThreshold)
            return 1f;

        float rawRatio = Mathf.InverseLerp(_chargeThreshold, _maxChargeTime, chargeTime);

        if (_chargeForceCurve == null || _chargeForceCurve.length == 0)
            return rawRatio;

        return Mathf.Clamp01(_chargeForceCurve.Evaluate(rawRatio));
    }

    #endregion
}