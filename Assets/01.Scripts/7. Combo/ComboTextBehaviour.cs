using DG.Tweening;
using TMPro;
using UnityEngine;

public class ComboTextBehaviour : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private TMP_Text _tmp;
    [SerializeField] private CanvasGroup _cg;

    [Header("Motion")]
    [SerializeField] private float _moveY = 1.2f;
    [SerializeField] private float _duration = 0.45f;
    [SerializeField] private float _startScale = 0.8f;
    [SerializeField] private float _punchScale = 1.15f;
    [SerializeField] private Ease _moveEase = Ease.OutCubic;
    [SerializeField] private Ease _scaleEase = Ease.OutBack;

    private Sequence _sequence;

    private void OnDisable()
    {
        KillSequence();
    }

    // 외부에서 월드 좌표와 콤보 값을 받아 텍스트를 재생한다.
    public void Play(Vector3 worldPosition, int comboCount)
    {
        KillSequence();

        transform.position = worldPosition;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one * _startScale;

        if (_tmp != null)
            _tmp.text = $"{comboCount} COMBO";

        if (_cg != null)
            _cg.alpha = 1f;

        Vector3 targetPosition = worldPosition + Vector3.up * _moveY;

        _sequence = DOTween.Sequence();
        _sequence.Append(transform.DOMove(targetPosition, _duration).SetEase(_moveEase));
        _sequence.Join(transform.DOScale(Vector3.one * _punchScale, _duration * 0.35f).SetEase(_scaleEase));
        _sequence.Join(transform.DOScale(Vector3.one, _duration * 0.65f).SetDelay(_duration * 0.35f).SetEase(Ease.OutQuad));

        if (_cg != null)
            _sequence.Join(_cg.DOFade(0f, _duration).SetEase(Ease.OutQuad));

        _sequence.OnComplete(ReturnToPool);
    }

    // 외부에서 Vector2도 바로 넘길 수 있게 오버로드를 제공한다.
    public void Play(Vector2 worldPosition, int comboCount)
    {
        Play((Vector3)worldPosition, comboCount);
    }

    // 재생 중인 시퀀스를 안전하게 종료한다.
    private void KillSequence()
    {
        if (_sequence == null)
            return;

        if (_sequence.IsActive())
            _sequence.Kill();

        _sequence = null;
    }

    // 연출이 끝나면 풀로 되돌린다.
    private void ReturnToPool()
    {
        KillSequence();

        if (ComboPoolManager.Instance != null)
            ComboPoolManager.Instance.ReturnQueue(this);
        else
            gameObject.SetActive(false);
    }
}