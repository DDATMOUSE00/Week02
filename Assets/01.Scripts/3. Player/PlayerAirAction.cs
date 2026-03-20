using UnityEngine;

[RequireComponent(typeof(PlayerSlamDamage))]
public class PlayerAirAction : MonoBehaviour
{
    #region Inspector

    [Header("Reference | Inspector 연결 필요")]
    [SerializeField] private CameraController _cameraController;

    [Header("Slam")]
    [SerializeField] private float _slamSpeed = 22f;
    [SerializeField] private float _slamAnticipationDuration = 0.08f;

    [Header("Charged Slam")]
    [SerializeField] private float _chargedSlamSpeed = 30f;
    [SerializeField] private float _chargedSlamAnticipationDuration = 0.14f;

    [Header("Effect")]
    [SerializeField] private PlayerSlamBeforeEffect _effect;

    #endregion

    #region Runtime Fields

    private PlayerSlamDamage _slamDamage;

    private float _slamAnticipationTimer;
    private float _slamStartY;
    private float _currentSlamSpeed;

    #endregion

    #region Properties

    public float SlamSpeed => _currentSlamSpeed > 0f ? _currentSlamSpeed : _slamSpeed;

    #endregion

    #region Unity Lifecycle

    // 같은 오브젝트의 필수 컴포넌트를 가져온다.
    private void Awake()
    {
        _slamDamage = GetComponent<PlayerSlamDamage>();

        if (_cameraController == null)
            Debug.LogWarning($"{nameof(PlayerAirAction)}: _cameraController 가 비어 있어서 슬램 카메라 흔들림은 재생되지 않습니다.", this);
    }

    #endregion

    #region Slam

    // 현재 공중 슬램 진입이 가능한지 반환한다.
    public bool CanStartAirSlam(PlayerControllerVersionTwo controller)
    {
        return !controller.IsCharging &&
               !controller.IsSlamAnticipating &&
               !controller.IsSlamming &&
               controller.CanSlam;
    }

    // 슬램 예고 상태에 진입한다.
    public void StartSlamAnticipation(PlayerControllerVersionTwo controller, PlayerMovementMotor movement)
    {
        controller.EnterSlamAnticipationState();

        bool useChargedSlam = controller.LastJumpWasCharged;

        _slamAnticipationTimer = useChargedSlam
            ? _chargedSlamAnticipationDuration
            : _slamAnticipationDuration;

        _currentSlamSpeed = useChargedSlam
            ? _chargedSlamSpeed
            : _slamSpeed;

        _slamStartY = controller.transform.position.y;

        movement.ClearJumpLaunchMomentum();
        movement.FreezeBody();

        if (_effect != null && controller.LastJumpWasCharged)
            _effect.Play();

        if (controller.ShowDebugLog)
        {
            Debug.Log(
                $"Start Slam Anticipation | charged: {useChargedSlam}, " +
                $"slamStartY: {_slamStartY:F2}, anticipation: {_slamAnticipationTimer:F2}, " +
                $"slamSpeed: {_currentSlamSpeed:F2}");
        }
    }

    // 슬램 예고 타이머를 갱신하고, 시간이 끝나면 슬램 돌입을 시작한다.
    public void TickAnticipationTimer(PlayerControllerVersionTwo controller, PlayerMovementMotor movement, float deltaTime)
    {
        if (!controller.IsSlamAnticipating)
            return;

        _slamAnticipationTimer -= deltaTime;

        if (_slamAnticipationTimer <= 0f)
            BeginSlamDive(controller, movement);
    }

    // 슬램 돌입 상태로 전환한다.
    private void BeginSlamDive(PlayerControllerVersionTwo controller, PlayerMovementMotor movement)
    {
        if (!controller.IsSlamAnticipating)
            return;

        controller.EnterSlamDiveState();
        _slamAnticipationTimer = 0f;

        movement.SetVelocity(new Vector2(0f, -SlamSpeed));

        if (controller.ShowDebugLog)
            Debug.Log($"Start Slam Dive | slamSpeed: {SlamSpeed:F2}");
    }

    // 슬램 낙하 높이를 바탕으로 데미지를 적용하고, 비율에 따라 카메라 흔들림을 재생한다.
    public void ApplySlamDamage(PlayerMovementMotor movement, Vector3 fallbackPosition, bool showDebugLog)
    {
        Vector2 impactPoint = movement.GetImpactPoint(fallbackPosition);

        float slamRatio = _slamDamage.EvaluateSlamShakeRatio(impactPoint, _slamStartY);

        _slamDamage.ApplySlamDamage(impactPoint, _slamStartY, showDebugLog);
        _cameraController?.PlaySlamLandShake(slamRatio, impactPoint.x);
    }

    // 착지 후 슬램 런타임 상태를 초기화한다.
    public void ResetRuntimeState()
    {
        _slamAnticipationTimer = 0f;
        _currentSlamSpeed = 0f;
    }

    #endregion
}