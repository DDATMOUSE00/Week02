using UnityEngine;

public class PlayerSlamBeforeEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particle;

    public void Play()
    {
        _particle.Play();
    }
}