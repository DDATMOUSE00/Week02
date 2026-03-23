using UnityEngine;

[RequireComponent (typeof(PlayerCombo))]
public class PlayerHitEffect : MonoBehaviour
{
    [Header("Player Combo")]
    [SerializeField] private PlayerCombo _playerCombo;

    [Header("Normal Slam Particle")]
    [SerializeField] private ParticleSystem _normalHitParticle;

    [Header("Charging Slam Particle - Star")]
    [SerializeField] private ParticleSystem _starLevel_1_Particle;
    [SerializeField] private ParticleSystem _starLevel_2_Particle;
    [SerializeField] private ParticleSystem _starLevel_3_Particle;

    [Header("Charging Slam Particle - Hit")]
    [SerializeField] private ParticleSystem _hitLevel_0_Particle;
    [SerializeField] private ParticleSystem _hitLevel_1_Particle;
    [SerializeField] private ParticleSystem _hitLevel_2_Particle;
    [SerializeField] private ParticleSystem _hitLevel_3_Particle;

    [Header("World Offset")]
    [SerializeField] private Vector3 _starWorldOffset;
    [SerializeField] private Vector3 _hitWorldOffset;

    [Header("Force Y")]
    [SerializeField] private bool _useForcedY;
    [SerializeField] private float _forcedY;

    private bool _hasPendingPlay;
    private bool _pendingIsCharging = false;
    private Vector3 _pendingWorldPosition;

    private void OnValidate()
    {
        _playerCombo = GetComponent<PlayerCombo>();
    }

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
            PlayParticlePerLevel(playPosition + _starWorldOffset);
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

    private void PlayParticlePerLevel(Vector3 worldPosition)
    {
        //PlayParticle(_hitParticle, playPosition + _hitWorldOffset);
        switch (_playerCombo.CurrentComboLevel)
        {
            case 0:
                PlayParticle(_hitLevel_0_Particle, worldPosition);
                break;
            case 1:
                PlayParticle(_starLevel_1_Particle, worldPosition);
                PlayParticle(_hitLevel_1_Particle, worldPosition);
                break;       
            case 2:
                PlayParticle(_starLevel_2_Particle, worldPosition);
                PlayParticle(_hitLevel_2_Particle, worldPosition);
                break;
            case 3:
                PlayParticle(_starLevel_3_Particle, worldPosition);
                PlayParticle(_hitLevel_3_Particle, worldPosition);
                break;
            default:
                break;
        }
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