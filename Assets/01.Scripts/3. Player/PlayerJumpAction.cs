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

    [Header("Buffer")]
    [SerializeField] private float _jumpBufferTime = 0.12f;

    #endregion

    #region Runtime Fields

    private float _chargeTimer;
    private float _jumpBufferTimer;

    #endregion

    #region Properties

    public bool HasBufferedJump => _jumpBufferTimer > 0f;

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
        float ratio = isChargedJump
            ? Mathf.InverseLerp(_chargeThreshold, _maxChargeTime, chargeTime)
            : 0f;

        float jumpForceY = isChargedJump
            ? Mathf.Lerp(_normalJumpForceY, _maxChargeJumpForceY, ratio)
            : _normalJumpForceY;

        float jumpForceX = isChargedJump
            ? Mathf.Lerp(_normalJumpForceX, _maxChargeJumpForceX, ratio)
            : _normalJumpForceX;

        LaunchJump(controller, movement, jumpForceX, jumpForceY);

        if (controller.ShowDebugLog)
        {
            Debug.Log(isChargedJump
                ? $"Charged Jump | ChargeTime: {chargeTime:F2}, Ratio: {ratio:F2}"
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
}