using UnityEngine;

public class PlayerCharacterVisual : MonoBehaviour
{
    #region Inspector

    [Header("Reference | Inspector 연결 필요")]
    [SerializeField] private PlayerControllerVersionTwo _controller;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    [Header("Sprite")]
    [SerializeField] private Sprite _defaultSprite;
    [SerializeField] private Sprite _jumpSprite;
    [SerializeField] private Sprite _slamSprite;

    [Header("Walk Visual | 선택 사항")]
    [SerializeField] private GorillaWalkBounce _gorillaWalkBounce;

    [Header("Walk Dust FX | 선택 사항")]
    [SerializeField] private ParticleSystem _walkDustParticle;
    [SerializeField] private float _walkDustInterval = 0.12f;

    #endregion

    #region Runtime Fields

    private float _walkDustTimer;

    #endregion

    #region Unity Lifecycle

    // 인스펙터에서 반드시 연결해야 하는 참조를 검증한다.
    private void Awake()
    {
        if (!ValidateRequiredReferences())
        {
            enabled = false;
            return;
        }
    }

    // 활성화 시 임시 상태를 초기화한다.
    private void OnEnable()
    {
        _walkDustTimer = 0f;
        _gorillaWalkBounce?.SetWalking(false);
    }

    // 비활성화 시 보행 상태와 파티클을 정리한다.
    private void OnDisable()
    {
        _gorillaWalkBounce?.SetWalking(false);
        StopWalkDust();
    }

    // 현재 플레이어 상태에 맞게 비주얼을 갱신한다.
    private void Update()
    {
        UpdateSpriteByState();
        UpdateWalkVisual();
        UpdateWalkDust();
    }

    #endregion

    #region Setup Validation

    // 인스펙터 참조를 검증한다.
    private bool ValidateRequiredReferences()
    {
        bool isValid = true;

        if (_controller == null)
        {
            Debug.LogError($"{nameof(PlayerCharacterVisual)}: _controller 가 비어 있습니다.", this);
            isValid = false;
        }

        if (_spriteRenderer == null)
        {
            Debug.LogError($"{nameof(PlayerCharacterVisual)}: _spriteRenderer 가 비어 있습니다.", this);
            isValid = false;
        }

        return isValid;
    }

    #endregion

    #region Visual Update

    // 현재 상태에 따라 표시할 스프라이트를 선택한다.
    private void UpdateSpriteByState()
    {
        if (_spriteRenderer == null || _controller == null)
            return;

        Sprite targetSprite = _defaultSprite;

        if (_controller.IsSlamming)
            targetSprite = _slamSprite ?? _jumpSprite ?? _defaultSprite;
        else if (!_controller.IsGrounded)
            targetSprite = _jumpSprite ?? _defaultSprite;
        else
            targetSprite = _defaultSprite;

        if (_spriteRenderer.sprite != targetSprite)
            _spriteRenderer.sprite = targetSprite;
    }

    // 걷기 바운스와 좌우 반전을 갱신한다.
    private void UpdateWalkVisual()
    {
        if (_controller == null)
            return;

        int facing = _controller.VisualFacingDirection;

        if (_gorillaWalkBounce != null)
        {
            _gorillaWalkBounce.SetFacing(facing);
            _gorillaWalkBounce.SetWalking(_controller.IsWalking);
            return;
        }

        if (_spriteRenderer != null)
            _spriteRenderer.flipX = facing < 0;
    }

    // 걷기 상태일 때 주기적으로 먼지 파티클을 재생한다.
    private void UpdateWalkDust()
    {
        if (_controller == null || _walkDustParticle == null)
            return;

        if (!_controller.IsWalking)
        {
            _walkDustTimer = 0f;
            return;
        }

        _walkDustTimer -= Time.deltaTime;
        if (_walkDustTimer > 0f)
            return;

        _walkDustTimer = _walkDustInterval;
        _walkDustParticle.Play();
    }

    // 걷기 먼지 파티클을 즉시 정지하고 비운다.
    private void StopWalkDust()
    {
        if (_walkDustParticle == null)
            return;

        _walkDustParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    #endregion
}