using UnityEngine;

public class PlayerHitEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem _starParticle;
    [SerializeField] private ParticleSystem _hitParticle;

    public void Play()
    {
        _starParticle.Play();
        _hitParticle.Play();
    }
}