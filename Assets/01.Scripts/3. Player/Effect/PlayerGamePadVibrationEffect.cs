using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGamePadVibration : MonoBehaviour
{
    [Header("Charged Slam Vibration")]
    [SerializeField, Range(0f, 1f)] private float _minChargeRatioToVibrate = 0.05f;

    [SerializeField, Range(0f, 1f)] private float _minLowFrequency = 0.25f;
    [SerializeField, Range(0f, 1f)] private float _maxLowFrequency = 0.9f;

    [SerializeField, Range(0f, 1f)] private float _minHighFrequency = 0.15f;
    [SerializeField, Range(0f, 1f)] private float _maxHighFrequency = 0.75f;

    [SerializeField] private float _minDuration = 0.08f;
    [SerializeField] private float _maxDuration = 0.18f;

    [SerializeField] private float _superChargeMultiplier = 1.15f;

    private Coroutine _rumbleCoroutine;
    private Gamepad _activeGamepad;

    // 오브젝트가 꺼질 때 남아 있는 진동을 정리한다.
    private void OnDisable()
    {
        StopRumble();
    }

    // 오브젝트가 파괴될 때 남아 있는 진동을 정리한다.
    private void OnDestroy()
    {
        StopRumble();
    }

    // 차지 슬램 착지 시 차지량과 낙하 충격량에 비례해 진동을 재생한다.
    public void PlayChargedSlamImpactRumble(float slamChargeRatio, float slamImpactRatio, bool isSuperCharge)
    {
        Gamepad gamepad = Gamepad.current;
        if (gamepad == null)
            return;

        float clampedChargeRatio = Mathf.Clamp01(slamChargeRatio);
        if (clampedChargeRatio < _minChargeRatioToVibrate)
            return;

        float clampedImpactRatio = Mathf.Clamp01(slamImpactRatio);
        float normalizedCharge = Mathf.InverseLerp(_minChargeRatioToVibrate, 1f, clampedChargeRatio);

        float intensity01 = Mathf.Clamp01(Mathf.Max(normalizedCharge, clampedImpactRatio));

        float lowFrequency = Mathf.Lerp(_minLowFrequency, _maxLowFrequency, intensity01);
        float highFrequency = Mathf.Lerp(_minHighFrequency, _maxHighFrequency, normalizedCharge);
        float duration = Mathf.Lerp(_minDuration, _maxDuration, intensity01);

        if (isSuperCharge)
        {
            lowFrequency = Mathf.Clamp01(lowFrequency * _superChargeMultiplier);
            highFrequency = Mathf.Clamp01(highFrequency * _superChargeMultiplier);
            duration *= _superChargeMultiplier;
        }

        if (_rumbleCoroutine != null)
            StopCoroutine(_rumbleCoroutine);

        _activeGamepad = gamepad;
        _rumbleCoroutine = StartCoroutine(CoPlayRumble(lowFrequency, highFrequency, duration));
    }

    // 현재 재생 중인 진동을 즉시 중지한다.
    public void StopRumble()
    {
        if (_rumbleCoroutine != null)
        {
            StopCoroutine(_rumbleCoroutine);
            _rumbleCoroutine = null;
        }

        if (_activeGamepad != null)
        {
            _activeGamepad.SetMotorSpeeds(0f, 0f);
            _activeGamepad = null;
        }
    }

    // 지정한 강도와 시간 동안 진동을 재생한 뒤 자동으로 종료한다.
    private IEnumerator CoPlayRumble(float lowFrequency, float highFrequency, float duration)
    {
        _activeGamepad.SetMotorSpeeds(lowFrequency, highFrequency);

        yield return new WaitForSecondsRealtime(Mathf.Max(0f, duration));

        if (_activeGamepad != null)
            _activeGamepad.SetMotorSpeeds(0f, 0f);

        _activeGamepad = null;
        _rumbleCoroutine = null;
    }
}