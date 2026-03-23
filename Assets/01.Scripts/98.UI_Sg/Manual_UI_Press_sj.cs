using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Manual_Ui_Press_sg : MonoBehaviour
{
    [Header("입력 액션 레퍼런스")]
    private InputActionReference _navigateActionReference;
    private InputActionReference _spaceActionReference;

    [Header("키보드 자판 아이콘")]
    private Image _a;
    private Image _d;
    private Image _spacebar;

    [Header("패드 자판 아이콘")]
    private Image _gamepadJump;

    [Header("A, D 아이콘 시각 효과 설정")]
    [SerializeField] private float _adDefaultSizeY = 100f;
    [SerializeField] private float _adPressedSizeY = 80f;
    [SerializeField] private Color _adDefaultColor = Color.white;
    [SerializeField] private Color _adPressedColor = new Color(123f / 255f, 123f / 255f, 123f / 255f, 1f);

    [Header("스페이스바 아이콘 시각 효과 설정")]
    [SerializeField] private float _spaceDefaultSizeY = 100f;
    [SerializeField] private float _spacePressedSizeY = 80f;
    [SerializeField] private Color _spaceDefaultColor = Color.white;
    [SerializeField] private Color _spacePressedColor = new Color(123f / 255f, 123f / 255f, 123f / 255f, 1f);

    [Header("Y Offset")]
    [SerializeField] private float yOffset = 8f;

    private bool _stageStart;

    private Vector2 _aDefaultAnchoredPosition;
    private Vector2 _dDefaultAnchoredPosition;
    private Vector2 _spacebarDefaultAnchoredPosition;
    private Vector2 _gamepadJumpDefaultAnchoredPosition;

    private void Awake()
    {
        _navigateActionReference = UIManager.Instance.NavigateActionReference;
        _spaceActionReference = UIManager.Instance.SpaceActionReference;

        _a = UIManager.Instance.A;
        _d = UIManager.Instance.D;
        _spacebar = UIManager.Instance.Spacebar;
        _gamepadJump = UIManager.Instance.GamepadJump;

        _aDefaultAnchoredPosition = GetAnchoredPosition(_a);
        _dDefaultAnchoredPosition = GetAnchoredPosition(_d);
        _spacebarDefaultAnchoredPosition = GetAnchoredPosition(_spacebar);
        _gamepadJumpDefaultAnchoredPosition = GetAnchoredPosition(_gamepadJump);
    }

    private void OnEnable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.AddListener(MEventType.StageStarted, OnGameStart);
            EventManager.Instance.AddListener(MEventType.StageFailed, OnGameFail);
        }

        if (_navigateActionReference != null)
        {
            _navigateActionReference.action.Enable();
            _navigateActionReference.action.performed += OnNavigatePerformed;
            _navigateActionReference.action.canceled += OnNavigateCanceled;
        }

        if (_spaceActionReference != null)
        {
            _spaceActionReference.action.Enable();
            _spaceActionReference.action.performed += OnSpacePerformed;
            _spaceActionReference.action.canceled += OnSpaceCanceled;
        }

        ResetLeftAction();
        ResetRightAction();
        ResetSpacebarAction();
    }

    private void OnDisable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.RemoveListener(MEventType.StageStarted, this);
            EventManager.Instance.RemoveListener(MEventType.StageFailed, this);
        }

        if (_navigateActionReference != null)
        {
            _navigateActionReference.action.performed -= OnNavigatePerformed;
            _navigateActionReference.action.canceled -= OnNavigateCanceled;
            _navigateActionReference.action.Disable();
        }

        if (_spaceActionReference != null)
        {
            _spaceActionReference.action.performed -= OnSpacePerformed;
            _spaceActionReference.action.canceled -= OnSpaceCanceled;
            _spaceActionReference.action.Disable();
        }
    }

    #region 입력 처리 (Navigate: A, D)

    private void OnNavigatePerformed(InputAction.CallbackContext context)
    {
        Vector2 navigationInput = context.ReadValue<Vector2>();

        if (navigationInput.x < -0.5f)
        {
            ExecuteLeftAction();
            ResetRightAction();
        }
        else if (navigationInput.x > 0.5f)
        {
            ExecuteRightAction();
            ResetLeftAction();
        }
    }

    private void OnNavigateCanceled(InputAction.CallbackContext context)
    {
        ResetLeftAction();
        ResetRightAction();
    }

    #endregion

    #region 입력 처리 (Space)

    private void OnSpacePerformed(InputAction.CallbackContext context)
    {
        ExecuteSpacebarAction();
    }

    private void OnSpaceCanceled(InputAction.CallbackContext context)
    {
        ResetSpacebarAction();
    }

    #endregion

    #region UI 시각 효과 실행 / 초기화

    private void ExecuteLeftAction()
    {
        if (_a == null) return;

        ApplyVisual(
            _a,
            _adPressedSizeY,
            _adPressedColor,
            new Vector2(_aDefaultAnchoredPosition.x, _aDefaultAnchoredPosition.y - yOffset));
    }

    private void ExecuteRightAction()
    {
        if (_d == null) return;

        ApplyVisual(
            _d,
            _adPressedSizeY,
            _adPressedColor,
            new Vector2(_dDefaultAnchoredPosition.x, _dDefaultAnchoredPosition.y - yOffset));
    }

    private void ExecuteSpacebarAction()
    {
        if (_spacebar != null)
        {
            ApplyVisual(
                _spacebar,
                _spacePressedSizeY,
                _spacePressedColor,
                new Vector2(_spacebarDefaultAnchoredPosition.x, _spacebarDefaultAnchoredPosition.y - yOffset));
        }

        if (_gamepadJump != null)
        {
            ApplyVisual(
                _gamepadJump,
                _spacePressedSizeY,
                _spacePressedColor,
                new Vector2(_gamepadJumpDefaultAnchoredPosition.x, _gamepadJumpDefaultAnchoredPosition.y - yOffset));
        }
    }

    private void ResetLeftAction()
    {
        if (_a == null) return;

        ApplyVisual(
            _a,
            _adDefaultSizeY,
            _adDefaultColor,
            _aDefaultAnchoredPosition);
    }

    private void ResetRightAction()
    {
        if (_d == null) return;

        ApplyVisual(
            _d,
            _adDefaultSizeY,
            _adDefaultColor,
            _dDefaultAnchoredPosition);
    }

    private void ResetSpacebarAction()
    {
        if (_spacebar != null)
        {
            ApplyVisual(
                _spacebar,
                _spaceDefaultSizeY,
                _spaceDefaultColor,
                _spacebarDefaultAnchoredPosition);
        }

        if (_gamepadJump != null)
        {
            ApplyVisual(
                _gamepadJump,
                _spaceDefaultSizeY,
                _spaceDefaultColor,
                _gamepadJumpDefaultAnchoredPosition);
        }
    }

    private void ApplyVisual(Image target, float sizeY, Color color, Vector2 anchoredPosition)
    {
        RectTransform rectTransform = target.rectTransform;
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, sizeY);
        rectTransform.anchoredPosition = anchoredPosition;
        target.color = color;
    }

    private Vector2 GetAnchoredPosition(Image target)
    {
        if (target == null)
            return Vector2.zero;

        return target.rectTransform.anchoredPosition;
    }

    #endregion

    #region 이벤트

    private void OnGameFail(MEventType mEventType, Component sender, EventArgs args)
    {
        _stageStart = false;
    }

    private void OnGameStart(MEventType mEventType, Component sender, EventArgs args)
    {
        _stageStart = true;
    }

    #endregion
}