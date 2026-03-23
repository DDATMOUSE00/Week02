using UnityEngine;

[RequireComponent (typeof(PlayerGamePadVibration))]
[RequireComponent(typeof(PlayerCombo))]
public class PlayerSlamDamage : MonoBehaviour
{
    [Header("Player Controller")]
    [SerializeField] private PlayerControllerVersionTwo _controller;

    [Header("Slam Impact Range")]
    [SerializeField] private LayerMask _slamDamageLayer;
    [SerializeField] private Vector2 _slamBaseImpactBoxSize = new(2.4f, 2.4f);
    [SerializeField] private Vector2 _slamImpactBoxSizePerHeight = new(0.7f, 0.7f);
    [SerializeField] private Vector2 _slamImpactBoxSizePerChargeRatio = new(1.2f, 1.2f);
    [SerializeField] private Vector2 _slamMaxImpactBoxSize = new(7f, 7f);

    [Header("Combo Range Step")]
    [SerializeField] private Vector2 _comboLevel00ImpactBoxBonus = Vector2.zero;
    [SerializeField] private Vector2 _comboLevel01ImpactBoxBonus = new Vector2(0.75f, 0.75f);
    [SerializeField] private Vector2 _comboLevel02ImpactBoxBonus = new Vector2(1.5f, 1.5f);
    [SerializeField] private Vector2 _comboLevel03ImpactBoxBonus = new Vector2(2.25f, 2.25f);

    [Header("Combo System")]
    [SerializeField] private PlayerCombo _playerCombo;

    [Header("Slam Shake")]
    [SerializeField] private float _slamShakeFullHeight = 8f;

    [Header("Effect")]
    [SerializeField] private PlayerHitEffect _playerHitEffect;

    [Header("Game Pad Vibration")]
    [SerializeField] private PlayerGamePadVibration _gamePadVibration;

    private bool _hasLastImpact;
    private Vector2 _lastImpactPoint;
    private Vector2 _lastSlamImpactBoxSize;

    private HotdogStore _hotDogChaching;

    private void OnValidate()
    {
        _controller = GetComponent<PlayerControllerVersionTwo>();
        _playerCombo = GetComponent<PlayerCombo>();
        _gamePadVibration = GetComponent<PlayerGamePadVibration>();

        _slamBaseImpactBoxSize.x = Mathf.Max(0.01f, _slamBaseImpactBoxSize.x);
        _slamBaseImpactBoxSize.y = Mathf.Max(0.01f, _slamBaseImpactBoxSize.y);

        _slamImpactBoxSizePerHeight.x = Mathf.Max(0f, _slamImpactBoxSizePerHeight.x);
        _slamImpactBoxSizePerHeight.y = Mathf.Max(0f, _slamImpactBoxSizePerHeight.y);

        _slamImpactBoxSizePerChargeRatio.x = Mathf.Max(0f, _slamImpactBoxSizePerChargeRatio.x);
        _slamImpactBoxSizePerChargeRatio.y = Mathf.Max(0f, _slamImpactBoxSizePerChargeRatio.y);

        _comboLevel00ImpactBoxBonus.x = Mathf.Max(0f, _comboLevel00ImpactBoxBonus.x);
        _comboLevel00ImpactBoxBonus.y = Mathf.Max(0f, _comboLevel00ImpactBoxBonus.y);

        _comboLevel01ImpactBoxBonus.x = Mathf.Max(0f, _comboLevel01ImpactBoxBonus.x);
        _comboLevel01ImpactBoxBonus.y = Mathf.Max(0f, _comboLevel01ImpactBoxBonus.y);

        _comboLevel02ImpactBoxBonus.x = Mathf.Max(0f, _comboLevel02ImpactBoxBonus.x);
        _comboLevel02ImpactBoxBonus.y = Mathf.Max(0f, _comboLevel02ImpactBoxBonus.y);

        _comboLevel03ImpactBoxBonus.x = Mathf.Max(0f, _comboLevel03ImpactBoxBonus.x);
        _comboLevel03ImpactBoxBonus.y = Mathf.Max(0f, _comboLevel03ImpactBoxBonus.y);

        _slamMaxImpactBoxSize.x = Mathf.Max(_slamBaseImpactBoxSize.x, _slamMaxImpactBoxSize.x);
        _slamMaxImpactBoxSize.y = Mathf.Max(_slamBaseImpactBoxSize.y, _slamMaxImpactBoxSize.y);

        _slamShakeFullHeight = Mathf.Max(0f, _slamShakeFullHeight);
    }

    // 슬램 낙하 높이, 차지 비율, 콤보 레벨을 바탕으로 범위를 계산해 적에게 적용한다.
    public void ApplySlamDamage(Vector2 impactPoint, float slamStartY, float slamChargeRatio, bool showDebugLog)
    {
        bool isSuperCharge = _controller != null && slamChargeRatio >= _controller.SuperChargeThreshold;

        if (_playerHitEffect != null)
            _playerHitEffect.PlayAt(impactPoint, isSuperCharge);

        float fallDistance = GetCurrentSlamFallDistance(slamStartY, impactPoint.y);
        float slamShakeRatio = EvaluateSlamShakeRatio(impactPoint, slamStartY);
        int comboLevel = _playerCombo != null ? _playerCombo.CurrentComboLevel : 0;
        Vector2 impactBoxSize = GetSlamImpactBoxSize(fallDistance, slamChargeRatio, comboLevel);

        if (_gamePadVibration != null)
            _gamePadVibration.PlayChargedSlamImpactRumble(slamChargeRatio, slamShakeRatio, isSuperCharge);

        _hasLastImpact = true;
        _lastImpactPoint = impactPoint;
        _lastSlamImpactBoxSize = impactBoxSize;

        Collider2D[] hits = Physics2D.OverlapBoxAll(impactPoint, impactBoxSize, 0f, _slamDamageLayer);
        if (hits == null || hits.Length == 0)
        {
            if (showDebugLog)
            {
                Debug.Log(
                    $"Slam Impact | No Target | fallDistance: {fallDistance:F2}, " +
                    $"chargeRatio: {slamChargeRatio:F2}, comboLevel: {comboLevel}, boxSize: {impactBoxSize}");
            }

            return;
        }

        int killedCount = 0;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit == null)
                continue;

            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            var hotDogStore = hit.GetComponent<HotdogStore>();
            var building = hit.GetComponent<BreakBuilding>();

            if (building != null)
                building.SlamBuilding();
            if (hotDogStore != null)
                hotDogStore.DestoyStore();
            if (enemy == null)
                continue;

            enemy.Kill();
            killedCount++;

            if (_playerCombo != null)
                _playerCombo.AddKill();
        }

        if (killedCount > 0 && ComboPoolManager.Instance != null && _playerCombo != null)
            ComboPoolManager.Instance.Play(impactPoint, _playerCombo.CurrentCombo);
    }

    // 슬램 낙하 높이를 기준으로 카메라 흔들림용 0~1 비율을 계산한다.
    public float EvaluateSlamShakeRatio(Vector2 impactPoint, float slamStartY)
    {
        float fallDistance = GetCurrentSlamFallDistance(slamStartY, impactPoint.y);

        if (_slamShakeFullHeight <= 0f)
            return fallDistance > 0f ? 1f : 0f;

        return Mathf.Clamp01(fallDistance / _slamShakeFullHeight);
    }

    // 현재 슬램의 실제 낙하 높이를 계산한다.
    private float GetCurrentSlamFallDistance(float slamStartY, float impactY)
    {
        return Mathf.Max(0f, slamStartY - impactY);
    }

    // 낙하 높이, 차지 비율, 콤보 레벨에 비례한 충돌 박스 크기를 계산한다.
    private Vector2 GetSlamImpactBoxSize(float fallDistance, float slamChargeRatio, int comboLevel)
    {
        float clampedChargeRatio = Mathf.Clamp01(slamChargeRatio);

        Vector2 boxSize = _slamBaseImpactBoxSize;
        boxSize += _slamImpactBoxSizePerHeight * fallDistance;
        boxSize += _slamImpactBoxSizePerChargeRatio * clampedChargeRatio;
        boxSize += GetComboLevelImpactBoxBonus(comboLevel);

        boxSize.x = Mathf.Clamp(boxSize.x, _slamBaseImpactBoxSize.x, _slamMaxImpactBoxSize.x);
        boxSize.y = Mathf.Clamp(boxSize.y, _slamBaseImpactBoxSize.y, _slamMaxImpactBoxSize.y);

        return boxSize;
    }

    // 현재 콤보 레벨에 대응하는 계단형 범위 보너스를 반환한다.
    private Vector2 GetComboLevelImpactBoxBonus(int comboLevel)
    {
        switch (comboLevel)
        {
            case 3:
                return _comboLevel03ImpactBoxBonus;
            case 2:
                return _comboLevel02ImpactBoxBonus;
            case 1:
                return _comboLevel01ImpactBoxBonus;
            default:
                return _comboLevel00ImpactBoxBonus;
        }
    }

    // 현재 설정된 기본 박스와 마지막 실제 박스를 에디터에서 시각화한다.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.75f, 0f, 0.9f);
        Gizmos.DrawWireCube(transform.position, _slamBaseImpactBoxSize);

        if (!_hasLastImpact || _lastSlamImpactBoxSize.x <= 0f || _lastSlamImpactBoxSize.y <= 0f)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(_lastImpactPoint, _lastSlamImpactBoxSize);
    }
}