using UnityEngine;

[RequireComponent(typeof(PlayerCombo))]
public class PlayerAuraEffect : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private PlayerCombo _playerCombo;

    [Header("Aura")]
    [SerializeField] private ParticleSystem _lv1Aura;
    [SerializeField] private ParticleSystem _lv2Aura;
    [SerializeField] private ParticleSystem _lv3Aura;

    private int _currentAuraLevel = -1;

    private void OnValidate()
    {
        _playerCombo = GetComponent<PlayerCombo>();
    }
    private void OnEnable()
    {
        _currentAuraLevel = -1;
        RefreshAura(force: true);
    }

    private void LateUpdate()
    {
        RefreshAura();
    }

    private void OnDisable()
    {
        StopAllAura();
        _currentAuraLevel = -1;
    }

    private void RefreshAura(bool force = false)
    {
        if (_playerCombo == null)
            return;

        int targetLevel = Mathf.Clamp(_playerCombo.CurrentComboLevel, 0, 3);

        if (!force && _currentAuraLevel == targetLevel)
            return;

        _currentAuraLevel = targetLevel;
        ApplyAuraLevel(_currentAuraLevel);
    }

    private void ApplyAuraLevel(int comboLevel)
    {
        StopAllAura();

        switch (comboLevel)
        {
            case 1:
                PlayAura(_lv1Aura);
                break;

            case 2:
                PlayAura(_lv2Aura);
                break;

            case 3:
                PlayAura(_lv3Aura);
                break;
        }
    }

    private void PlayAura(ParticleSystem aura)
    {
        if (aura == null)
            return;

        aura.gameObject.SetActive(true);
        aura.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        aura.Play(true);
    }

    private void StopAllAura()
    {
        StopAura(_lv1Aura);
        StopAura(_lv2Aura);
        StopAura(_lv3Aura);
    }

    private void StopAura(ParticleSystem aura)
    {
        if (aura == null)
            return;

        aura.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}