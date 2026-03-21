using UnityEngine;

public class PlayerSlamBeforeEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particle;
    [SerializeField] private Vector3 _worldOffset;

    public void Play()
    {
        if (_particle == null)
            return;

        _particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        _particle.Play();
    }

    public void PlayAt(Vector3 worldPosition)
    {
        if (_particle == null)
            return;

        Transform particleTransform = _particle.transform;
        particleTransform.position = new Vector3(
            worldPosition.x + _worldOffset.x,
            worldPosition.y + _worldOffset.y,
            particleTransform.position.z);

        _particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        _particle.Play();
    }
}
