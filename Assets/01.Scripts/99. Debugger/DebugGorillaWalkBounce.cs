using DG.Tweening;
using UnityEngine;

public class DebugGorillaWalkBounce : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Transform _visualRoot;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    [Header("Walk Bounce")]
    [SerializeField] private float _bounceHeight = 0.06f;
    [SerializeField] private float _stepDuration = 0.12f;
    [SerializeField] private float _tiltAngle = 5f;

    [Header("Idle Reset")]
    [SerializeField] private float _resetDuration = 0.08f;

    private Vector3 _baseLocalPosition;
    private Vector3 _baseLocalEulerAngles;

    private Sequence _walkSequence;
    private Sequence _resetSequence;

    private bool _isWalking;

    private void Reset()
    {
        _visualRoot = transform;
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Awake()
    {
        if (_visualRoot == null)
            _visualRoot = transform;

        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();

        _baseLocalPosition = _visualRoot.localPosition;
        _baseLocalEulerAngles = _visualRoot.localEulerAngles;
    }

    private void OnDisable()
    {
        StopWalk(true);
    }

    public void SetFacing(int facingDirection)
    {
        if (_spriteRenderer == null)
            return;

        _spriteRenderer.flipX = facingDirection < 0;
    }

    public void SetWalking(bool shouldWalk)
    {
        if (_isWalking == shouldWalk)
            return;

        _isWalking = shouldWalk;

        if (_isWalking)
            StartWalk();
        else
            StopWalk(false);
    }

    private void StartWalk()
    {
        if (_visualRoot == null)
            return;

        KillTweens();

        _visualRoot.localPosition = _baseLocalPosition;
        _visualRoot.localEulerAngles = _baseLocalEulerAngles;

        float halfStep = _stepDuration * 0.5f;
        Vector3 leftTilt = new Vector3(_baseLocalEulerAngles.x, _baseLocalEulerAngles.y, _baseLocalEulerAngles.z + _tiltAngle);
        Vector3 rightTilt = new Vector3(_baseLocalEulerAngles.x, _baseLocalEulerAngles.y, _baseLocalEulerAngles.z - _tiltAngle);

        _walkSequence = DOTween.Sequence();

        // 왼발/오른발 번갈아 디디는 느낌으로,
        // local Y를 살짝 들고 회전을 번갈아 준다.
        _walkSequence.Append(_visualRoot.DOLocalMoveY(_baseLocalPosition.y + _bounceHeight, halfStep).SetEase(Ease.OutQuad));
        _walkSequence.Join(_visualRoot.DOLocalRotate(leftTilt, halfStep).SetEase(Ease.OutQuad));

        _walkSequence.Append(_visualRoot.DOLocalMoveY(_baseLocalPosition.y, halfStep).SetEase(Ease.InQuad));
        _walkSequence.Join(_visualRoot.DOLocalRotate(_baseLocalEulerAngles, halfStep).SetEase(Ease.InQuad));

        _walkSequence.Append(_visualRoot.DOLocalMoveY(_baseLocalPosition.y + _bounceHeight, halfStep).SetEase(Ease.OutQuad));
        _walkSequence.Join(_visualRoot.DOLocalRotate(rightTilt, halfStep).SetEase(Ease.OutQuad));

        _walkSequence.Append(_visualRoot.DOLocalMoveY(_baseLocalPosition.y, halfStep).SetEase(Ease.InQuad));
        _walkSequence.Join(_visualRoot.DOLocalRotate(_baseLocalEulerAngles, halfStep).SetEase(Ease.InQuad));

        _walkSequence.SetLoops(-1, LoopType.Restart);
    }

    private void StopWalk(bool immediate)
    {
        KillTweens();

        if (_visualRoot == null)
            return;

        if (immediate)
        {
            _visualRoot.localPosition = _baseLocalPosition;
            _visualRoot.localEulerAngles = _baseLocalEulerAngles;
            return;
        }

        _resetSequence = DOTween.Sequence();
        _resetSequence.Append(_visualRoot.DOLocalMove(_baseLocalPosition, _resetDuration).SetEase(Ease.OutQuad));
        _resetSequence.Join(_visualRoot.DOLocalRotate(_baseLocalEulerAngles, _resetDuration).SetEase(Ease.OutQuad));
    }

    private void KillTweens()
    {
        if (_walkSequence != null && _walkSequence.IsActive())
            _walkSequence.Kill();

        if (_resetSequence != null && _resetSequence.IsActive())
            _resetSequence.Kill();
    }
}