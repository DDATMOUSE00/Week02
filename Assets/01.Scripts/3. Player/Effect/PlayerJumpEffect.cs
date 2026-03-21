using UnityEngine;

public class PlayerJumpEffect : MonoBehaviour
{
    [Header("Particle")]
    [SerializeField] private ParticleSystem _jumpEffect;

    [Header("World Offset")]
    [SerializeField] private Vector3 _starWorldOffset;
    [SerializeField] private Vector3 _hitWorldOffset;

    [Header("Force Y")]
    [SerializeField] private bool _useForcedY;
    [SerializeField] private float _forcedY;

    private bool _hasPendingPlay;
    private Vector3 _pendingWorldPosition;

    public void PlayAt(Vector2 worldPosition)
    {
        PlayAt((Vector3)worldPosition);
    }

    public void PlayAt(Vector3 worldPosition)
    {
        _pendingWorldPosition = worldPosition;
        _hasPendingPlay = true;
    }

    private void LateUpdate()
    {
        if (!_hasPendingPlay)
            return;

        _hasPendingPlay = false;

        Vector3 playPosition = GetResolvedPlayPosition(_pendingWorldPosition);

        PlayParticle(_jumpEffect, playPosition + _starWorldOffset);
    }

    private Vector3 GetResolvedPlayPosition(Vector3 worldPosition)
    {
        if (_useForcedY)
            worldPosition.y = _forcedY;

        return worldPosition;
    }

    private void PlayParticle(ParticleSystem particle, Vector3 worldPosition)
    {
        if (particle == null)
            return;

        Transform particleTransform = particle.transform;
        particleTransform.position = worldPosition;
        particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        particle.Play();
    }
}