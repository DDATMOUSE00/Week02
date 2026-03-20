using System.Collections.Generic;
using UnityEngine;

public class PlayerSlamDamage : MonoBehaviour
{
    [Header("Slam Impact Damage")]
    [SerializeField] private LayerMask _slamDamageLayer;
    [SerializeField] private float _slamBaseImpactRadius = 1.2f;
    [SerializeField] private float _slamImpactRadiusPerHeight = 0.35f;
    [SerializeField] private float _slamMaxImpactRadius = 3.5f;
    [SerializeField] private float _slamBaseDamage = 10f;
    [SerializeField] private float _slamDamagePerHeight = 4f;
    [SerializeField] private float _slamMaxDamage = 999f;

    private bool _hasLastImpact;
    private Vector2 _lastImpactPoint;
    private float _lastSlamImpactRadius;

    // 슬램 낙하 높이를 바탕으로 범위와 데미지를 계산해 적에게 적용한다.
    public void ApplySlamDamage(Vector2 impactPoint, float slamStartY, bool showDebugLog)
    {
        float fallDistance = GetCurrentSlamFallDistance(slamStartY, impactPoint.y);
        float impactRadius = GetSlamImpactRadius(fallDistance);
        float damage = GetSlamDamage(fallDistance);

        _hasLastImpact = true;
        _lastImpactPoint = impactPoint;
        _lastSlamImpactRadius = impactRadius;

        Collider2D[] hits = Physics2D.OverlapCircleAll(impactPoint, impactRadius, _slamDamageLayer);
        if (hits == null || hits.Length == 0)
        {
            if (showDebugLog)
            {
                Debug.Log(
                    $"Slam Impact | No Target | fallDistance: {fallDistance:F2}, " +
                    $"radius: {impactRadius:F2}, damage: {damage:F2}");
            }

            return;
        }

        //HashSet<ISlamDamageable> damagedTargets = new();

        //for (int i = 0; i < hits.Length; i++)
        //{
        //    Collider2D hit = hits[i];
        //    if (hit == null)
        //        continue;

        //    ISlamDamageable damageable = hit.GetComponent<ISlamDamageable>();
        //    damageable ??= hit.GetComponentInParent<ISlamDamageable>();

        //    if (damageable == null)
        //        continue;

        //    if (!damagedTargets.Add(damageable))
        //        continue;

        //    damageable.TakeSlamDamage(damage, impactPoint);
        //}

        //if (showDebugLog)
        //{
        //    Debug.Log(
        //        $"Slam Impact | fallDistance: {fallDistance:F2}, " +
        //        $"radius: {impactRadius:F2}, damage: {damage:F2}, " +
        //        $"rawHits: {hits.Length}, uniqueTargets: {damagedTargets.Count}");
        //}
    }

    // 현재 슬램의 실제 낙하 높이를 계산한다.
    private float GetCurrentSlamFallDistance(float slamStartY, float impactY)
    {
        return Mathf.Max(0f, slamStartY - impactY);
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

    // 마지막 슬램 범위를 에디터에서 시각화한다.
    private void OnDrawGizmosSelected()
    {
        if (!_hasLastImpact || _lastSlamImpactRadius <= 0f)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_lastImpactPoint, _lastSlamImpactRadius);
    }
}