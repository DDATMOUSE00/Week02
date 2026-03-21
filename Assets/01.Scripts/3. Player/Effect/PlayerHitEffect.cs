using UnityEngine;

public class PlayerHitEffect : MonoBehaviour
{
    [Header("Normal Slam Particle")]
    [SerializeField] private ParticleSystem _normalHitParticle;

    [Header("Charging Slam Particle")]
    [SerializeField] private ParticleSystem _starParticle;
    [SerializeField] private ParticleSystem _hitParticle;

    [Header("World Offset")]
    [SerializeField] private Vector3 _starWorldOffset;
    [SerializeField] private Vector3 _hitWorldOffset;

    [Header("Force Y")]
    [SerializeField] private bool _useForcedY;
    [SerializeField] private float _forcedY;

    private bool _hasPendingPlay;
    private bool _pendingIsCharging = false;
    private Vector3 _pendingWorldPosition;

    public void PlayAt(Vector3 worldPosition, bool isCharging)
    {
        _pendingWorldPosition = worldPosition;
        _hasPendingPlay = true;
        _pendingIsCharging = isCharging;
    }

    private void LateUpdate()
    {
        if (!_hasPendingPlay)
            return;

        Vector3 playPosition = GetResolvedPlayPosition(_pendingWorldPosition);

        if(_pendingIsCharging)
        {
            PlayParticle(_starParticle, playPosition + _starWorldOffset);
            PlayParticle(_hitParticle, playPosition + _hitWorldOffset);
        }
        else
            PlayParticle(_normalHitParticle, playPosition + _hitWorldOffset);

        _hasPendingPlay = false;
        _pendingIsCharging = false;
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