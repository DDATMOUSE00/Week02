using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Manual_Ui_sg : MonoBehaviour
{
    [Header("입력 액션 레퍼런스")]
    [SerializeField] private InputActionReference _navigateActionReference;
    [SerializeField] private InputActionReference _spaceActionReference;

    [Header("키보드 UI 세트 (Image 객체)")]
    [SerializeField] private Image _a;
    [SerializeField] private Image _d;
    [SerializeField] private Image _spacebar;

    [Header("패드 UI 세트 (Image 객체)")]
    [SerializeField] private Image _gamepadLeft;
    [SerializeField] private Image _gamepadRight;
    [SerializeField] private Image _gamepadJump;

    [Header("깜빡이 설정")]
    [SerializeField] private float _switchInterval = 0.5f; // 깜빡임 속도
    private float _switchTimer = 0f;
    private bool _showLeftNow = true;

    [Header("입력 감도 설정")]
    [SerializeField] private float _stickDeadzone = 0.5f; // 쏠림 방지를 위해 데드존 상향

    private bool _isGamepad = false;
    private bool _showAD = false;
    private bool _showSpace = false;

    [Header("위치 추적")]
    [SerializeField] private GameObject _player;
    [SerializeField] private RectTransform _manualRect;
    [SerializeField] private Vector3 _manualOffset;

 

    private Camera _mainCamera;
    private RectTransform _parentRect;
    private Camera _uiCamera;

    private void Awake() { _mainCamera = Camera.main; }

    private void Start()
    {
        if (_manualRect != null)
        {
            _parentRect = _manualRect.parent as RectTransform;
            Canvas canvas = _manualRect.GetComponentInParent<Canvas>();
            _uiCamera = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? _mainCamera : null;
        }
        UpdateVisualsByDevice(false);
    }

    private void OnEnable()
    {
        InputSystem.onActionChange += OnActionChange;
        if (_navigateActionReference != null) _navigateActionReference.action.Enable();
        if (_spaceActionReference != null) _spaceActionReference.action.Enable();
    }

    private void OnDisable() { InputSystem.onActionChange -= OnActionChange; }

    private void OnActionChange(object obj, InputActionChange change)
    {
        if (change == InputActionChange.ActionStarted || change == InputActionChange.ActionPerformed)
        {
            var action = obj as InputAction;
            if (action == null || action.activeControl == null) return;

            bool isNowGamepad = action.activeControl.device is Gamepad;

            // 패드 쏠림으로 인한 강제 전환 방지
            if (isNowGamepad)
            {
                var val = action.ReadValueAsObject();
                float magnitude = (val is Vector2 v2) ? v2.magnitude : (val is float f ? Mathf.Abs(f) : 1f);
                if (magnitude < _stickDeadzone) return;
            }

            if (isNowGamepad != _isGamepad)
            {
                Debug.Log($"<color=lime>[Manual_UI]</color> 기기 전환: <b>{(isNowGamepad ? "패드" : "키보드")}</b> (원인: {action.name})");
                UpdateVisualsByDevice(isNowGamepad);
            }
        }
    }

    public void UpdateVisualsByDevice(bool isGamepad)
    {
        _isGamepad = isGamepad;
        RefreshUI();
    }

    private void Update()
    {
        // [핵심] 조작 중이든 아니든 _showAD가 켜져 있으면 무조건 타이머 가동
        if (_showAD)
        {
            _switchTimer += Time.deltaTime;
            if (_switchTimer >= _switchInterval)
            {
                _switchTimer = 0f;
                _showLeftNow = !_showLeftNow;
                RefreshUI(); // 상태 바뀔 때마다 UI 갱신
            }
        }
    }

    private void RefreshUI()
    {
        // 1. 모든 UI 초기화 (끔)
        _a?.gameObject.SetActive(false);
        _d?.gameObject.SetActive(false);
        _spacebar?.gameObject.SetActive(false);
        _gamepadLeft?.gameObject.SetActive(false);
        _gamepadRight?.gameObject.SetActive(false);
        _gamepadJump?.gameObject.SetActive(false);

        // 2. 현재 단계 및 기기에 따른 표시
        if (!_isGamepad) // 키보드 모드
        {
            if (_showAD)
            {
                // 키보드는 깜빡이지 않고 A, D 둘 다 보여줌 (원하면 여기도 타이머 적용 가능)
                _a?.gameObject.SetActive(true);
                _d?.gameObject.SetActive(true);
            }
            if (_showSpace) _spacebar?.gameObject.SetActive(true);
        }
        else // 패드 모드: 조작 여부 상관없이 타이머에 따라 번갈아 표시
        {
            if (_showAD)
            {
                if (_showLeftNow) _gamepadLeft?.gameObject.SetActive(true);
                else _gamepadRight?.gameObject.SetActive(true);
            }
            if (_showSpace) _gamepadJump?.gameObject.SetActive(true);
        }
    }

    // 트리거 등 외부에서 호출하여 UI를 켜고 끄는 함수들
    public void ShowOnlyAD()
    {
        _showAD = true;
        _showSpace = false;
        _switchTimer = 0f;
        _showLeftNow = true;
        RefreshUI();
        Debug.Log("<color=yellow>[Manual_UI]</color> AD 가이드 시작 (무한 깜빡이)");
    }

    public void ShowOnlySpace() { _showAD = false; _showSpace = true; RefreshUI(); }

    public void HideAll()
    {
        _showAD = false;
        _showSpace = false;
        RefreshUI();
        Debug.Log("<color=red>[Manual_UI]</color> 모든 가이드 비활성화 (트리거 발동)");
    }

    private void LateUpdate() { UpdateUIpos(); }
    private void UpdateUIpos()
    {
        if (_player == null || _manualRect == null || _parentRect == null) return;
        Vector3 targetPos = _player.transform.position + _manualOffset;
        Vector2 screenPos = _mainCamera.WorldToScreenPoint(targetPos);
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRect, screenPos, _uiCamera, out Vector2 localPoint))
            _manualRect.localPosition = localPoint;
    }






}