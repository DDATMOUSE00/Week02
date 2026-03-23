using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CutSceneFrameMotion : MonoBehaviour
{
    public enum EnterType
    {
        FromLeft,
        FromRight,
        FromBottom
    }

    [Header("Target")]
    [SerializeField] private RectTransform _target;

    [Header("Enter")]
    [SerializeField] private EnterType _enterType = EnterType.FromLeft;
    [SerializeField] private float _enterDistance = 1200f;
    [SerializeField] private float _enterDuration = 0.45f;
    [SerializeField] private Ease _enterEase = Ease.OutCubic; //  두트윈 애니메이션 진행 속도감 , 초반은 빠르고 갈수록 감속

    [Header("Shake")]
    [SerializeField] private bool _usePunch = true;
    [SerializeField] private Vector2 _punch = new Vector2(20f, 0f);
    [SerializeField] private float _punchDuration = 0.22f;
    [SerializeField] private int _punchVibrato = 12;
    [SerializeField] private float _punchElasticity = 0.8f;

    [Header("Fade")]
    [SerializeField] private bool _useFade = true;
    [SerializeField] private float _fadeDuration = 0.2f;

    private CanvasGroup _canvasGroup;
    private Sequence _seq;
    private Vector2 _anchoredFinalPos;

    private void Awake()
    {
        if (_target == null) _target = GetComponent<RectTransform>();
        _anchoredFinalPos = _target.anchoredPosition;

        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Play()
    {
        Kill();

        gameObject.SetActive(true);

        Vector2 startPos = _anchoredFinalPos;
        if (_enterType == EnterType.FromLeft) startPos += Vector2.left * _enterDistance;
        else if (_enterType == EnterType.FromRight) startPos += Vector2.right * _enterDistance;
        else if (_enterType == EnterType.FromBottom) startPos += Vector2.down * _enterDistance;

        _target.anchoredPosition = startPos;
        if (_useFade) _canvasGroup.alpha = 0f;
        else _canvasGroup.alpha = 1f;

        _seq = DOTween.Sequence();
        _seq.Append(_target.DOAnchorPos(_anchoredFinalPos, _enterDuration).SetEase(_enterEase));

        if (_useFade)
            _seq.Join(_canvasGroup.DOFade(1f, _fadeDuration).SetEase(Ease.OutQuad));

        if (_usePunch)
            _seq.Append(_target.DOPunchAnchorPos(_punch, _punchDuration, _punchVibrato, _punchElasticity));
    }

    public void Kill()
    {
        if (_seq != null && _seq.IsActive()) _seq.Kill();
        _seq = null;
    }

    public void ResetToFinal()
    {
        Kill();
        _target.anchoredPosition = _anchoredFinalPos;
        if (_canvasGroup != null) _canvasGroup.alpha = 1f;
    }
}
