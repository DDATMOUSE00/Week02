using UnityEngine;

public class PlayerDistanceChecker : MonoBehaviour
{
    [SerializeField] private Transform _player;

    public Transform Player => _player;

    public void Init(Transform player)
    {
        _player = player;
    }

    public bool HasPlayer()
    {
        return _player != null;
    }
    private float GetSqrDistance(Vector3 targetPosition)
    {
        if (_player == null)
            return float.MaxValue;

        Vector2 playerPos = _player.position;
        Vector2 targetPos = targetPosition;

        return (playerPos - targetPos).sqrMagnitude;
    }

    public float GetDistance(Vector3 targetPosition)
    {
        return Mathf.Sqrt(GetSqrDistance(targetPosition));
    }

    //가까운지 체크
    public bool IsInRange(Vector3 targetPosition, float distance)
    {
        return GetSqrDistance(targetPosition) <= distance * distance;
    }

    //멀리있는지 체크
    public bool IsOutOfRange(Vector3 targetPosition, float distance)
    {
        return GetSqrDistance(targetPosition) >= distance * distance;
    }
}
