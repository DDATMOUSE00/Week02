using UnityEngine;

public class PlayerSlamDamage : MonoBehaviour
{
    [Header("Slam Impact Damage")]
    [SerializeField] private LayerMask _slamDamageLayer;
    [SerializeField] private Vector2 _slamBaseImpactBoxSize = new(2.4f, 2.4f);
    [SerializeField] private Vector2 _slamImpactBoxSizePerHeight = new(0.7f, 0.7f);
    [SerializeField] private Vector2 _slamImpactBoxSizePerChargeRatio = new(1.2f, 1.2f);
    [SerializeField] private Vector2 _slamMaxImpactBoxSize = new(7f, 7f);
    [SerializeField] private float _slamBaseDamage = 10f;
    [SerializeField] private float _slamDamagePerHeight = 4f;
    [SerializeField] private float _slamMaxDamage = 999f;

    [Header("Combo System")]
    [SerializeField] private PlayerCombo _playerCombo;

    [Header("Slam Shake")]
    [SerializeField] private float _slamShakeFullHeight = 8f;

    [Header("Effect")]
    [SerializeField] private PlayerHitEffect _playerHitEffect;

    private bool _hasLastImpact;
    private Vector2 _lastImpactPoint;
    private Vector2 _lastSlamImpactBoxSize;

    private void OnValidate()
    {
        _slamBaseImpactBoxSize.x = Mathf.Max(0.01f, _slamBaseImpactBoxSize.x);
        _slamBaseImpactBoxSize.y = Mathf.Max(0.01f, _slamBaseImpactBoxSize.y);

        _slamImpactBoxSizePerHeight.x = Mathf.Max(0f, _slamImpactBoxSizePerHeight.x);
        _slamImpactBoxSizePerHeight.y = Mathf.Max(0f, _slamImpactBoxSizePerHeight.y);

        _slamImpactBoxSizePerChargeRatio.x = Mathf.Max(0f, _slamImpactBoxSizePerChargeRatio.x);
        _slamImpactBoxSizePerChargeRatio.y = Mathf.Max(0f, _slamImpactBoxSizePerChargeRatio.y);

        _slamMaxImpactBoxSize.x = Mathf.Max(_slamBaseImpactBoxSize.x, _slamMaxImpactBoxSize.x);
        _slamMaxImpactBoxSize.y = Mathf.Max(_slamBaseImpactBoxSize.y, _slamMaxImpactBoxSize.y);

        _slamBaseDamage = Mathf.Max(0f, _slamBaseDamage);
        _slamDamagePerHeight = Mathf.Max(0f, _slamDamagePerHeight);
        _slamMaxDamage = Mathf.Max(_slamBaseDamage, _slamMaxDamage);
        _slamShakeFullHeight = Mathf.Max(0f, _slamShakeFullHeight);
    }

    // 슬램 낙하 높이와 차지 비율을 바탕으로 범위와 데미지를 계산해 적에게 적용한다.
    public void ApplySlamDamage(Vector2 impactPoint, float slamStartY, float slamChargeRatio, bool showDebugLog)
    {
        if (_playerHitEffect != null)
            _playerHitEffect.PlayAt(impactPoint);

        float fallDistance = GetCurrentSlamFallDistance(slamStartY, impactPoint.y);
        Vector2 impactBoxSize = GetSlamImpactBoxSize(fallDistance, slamChargeRatio);
        float damage = GetSlamDamage(fallDistance);

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
                    $"chargeRatio: {slamChargeRatio:F2}, boxSize: {impactBoxSize}, damage: {damage:F2}");
            }

            return;
        }

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit == null)
                continue;

            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy == null)
                continue;

            enemy.Kill();

            if (_playerCombo != null)
                _playerCombo.AddKill();
        }
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

    // 낙하 높이와 차지 비율에 비례한 충돌 박스 크기를 계산한다.
    private Vector2 GetSlamImpactBoxSize(float fallDistance, float slamChargeRatio)
    {
        float clampedChargeRatio = Mathf.Clamp01(slamChargeRatio);

        Vector2 boxSize = _slamBaseImpactBoxSize;
        boxSize += _slamImpactBoxSizePerHeight * fallDistance;
        boxSize += _slamImpactBoxSizePerChargeRatio * clampedChargeRatio;

        boxSize.x = Mathf.Clamp(boxSize.x, _slamBaseImpactBoxSize.x, _slamMaxImpactBoxSize.x);
        boxSize.y = Mathf.Clamp(boxSize.y, _slamBaseImpactBoxSize.y, _slamMaxImpactBoxSize.y);

        return boxSize;
    }

    // 낙하 높이에 비례한 슬램 데미지를 계산한다.
    private float GetSlamDamage(float fallDistance)
    {
        float damage = _slamBaseDamage + (fallDistance * _slamDamagePerHeight);
        return Mathf.Clamp(damage, _slamBaseDamage, _slamMaxDamage);
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