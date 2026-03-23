using DG.Tweening;
using TMPro;
using UnityEngine;

public class TextRainbow : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private TMP_Text[] _tmpText;

    [Header("Rainbow")]
    [SerializeField, Min(0.1f)] private float _cycleDuration = 2.5f;
    [SerializeField, Range(0f, 1f)] private float _rightHueOffset = 0.15f;
    [SerializeField, Range(0f, 1f)] private float _saturation = 1f;
    [SerializeField, Range(0f, 1f)] private float _value = 1f;

    private Tween _rainbowTween;
    private float _hue;

    private void OnEnable()
    {
        PlayRainbow();
    }

    private void OnDisable()
    {
        //KillRainbow();
    }

    private void PlayRainbow()
    {
        if (_tmpText == null)
            return;

        KillRainbow();
        for(int i=0; i< _tmpText.Length; i++)
            _tmpText[i].enableVertexGradient = true;

        _hue = 0f;
        ApplyGradient(_hue);

        _rainbowTween = DOTween.To(
                () => _hue,
                value =>
                {
                    _hue = value;
                    ApplyGradient(_hue);
                },
                1f,
                _cycleDuration
            )
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart)
            .SetUpdate(true);
    }

    private void KillRainbow()
    {
        if (_rainbowTween != null && _rainbowTween.IsActive())
            _rainbowTween.Kill();

        _rainbowTween = null;

        ResetGradientToWhite();
    }

    private void ResetGradientToWhite()
    {
        if (_tmpText == null)
            return;

        Color white = Color.white;
        for (int i = 0; i < _tmpText.Length; i++)
            _tmpText[i].colorGradient = new VertexGradient(
            white,   // topLeft
            white,   // topRight
            white,   // bottomLeft
            white    // bottomRight
        );
    }

    private void ApplyGradient(float baseHue)
    {
        float leftHue = Mathf.Repeat(baseHue, 1f);
        float rightHue = Mathf.Repeat(baseHue + _rightHueOffset, 1f);

        Color leftColor = Color.HSVToRGB(leftHue, _saturation, _value);
        Color rightColor = Color.HSVToRGB(rightHue, _saturation, _value);

        VertexGradient gradient = new VertexGradient(
            leftColor,   // topLeft
            rightColor,  // topRight
            leftColor,   // bottomLeft
            rightColor   // bottomRight
        );
        for (int i = 0; i < _tmpText.Length; i++)
            _tmpText[i].colorGradient = gradient;
    }
}