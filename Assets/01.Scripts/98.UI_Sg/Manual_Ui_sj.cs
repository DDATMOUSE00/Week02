using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Manual_Ui_sg : MonoBehaviour
{
    [Header("키보드 UI 세트 (Image 객체)")]
    private Image _a;
    private Image _d;
    private Image _spacebar;

    [Header("패드 UI 세트 (Image 객체)")]
    private Image _gamepadLeft;
    private Image _gamepadRight;
    private Image _gamepadJump;

    [Header("위치 추적")]
    private GameObject _player;
    private RectTransform _manualRect;
    private Vector3 _manualOffset;

    [Header("Blink")]
    [SerializeField] private float _switchInterval = 0.5f;

    private float _switchTimer = 0f;
    private bool _showLeftNow = true;

    private bool _isGamepad = false;
    private bool _showAD = false;
    private bool _showSpace = false;

    private Camera _mainCamera;
    private RectTransform _parentRect;
    private Camera _uiCamera;

    // UIManager에서 필요한 레퍼런스를 가져오고 메인 카메라를 캐싱한다.
    private void Awake()
    {
        _mainCamera = Camera.main;

        _a = UIManager.Instance.A;
        _d = UIManager.Instance.D;
        _spacebar = UIManager.Instance.Spacebar;

        _gamepadLeft = UIManager.Instance.GamepadLeft;
        _gamepadRight = UIManager.Instance.GamepadRight;
        _gamepadJump = UIManager.Instance.GamepadJump;

        _player = UIManager.Instance.Player;
        _manualRect = UIManager.Instance.ManualRect;
        _manualOffset = UIManager.Instance.ManualOffset;
    }

    // 시작 시 부모 Rect와 UI 카메라를 캐싱하고 현재 입력 장치 상태를 반영한다.
    private void Start()
    {
        if (_manualRect != null)
        {
            _parentRect = _manualRect.parent as RectTransform;
            Canvas canvas = _manualRect.GetComponentInParent<Canvas>();
            _uiCamera = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? _mainCamera : null;
        }

        ApplyCurrentInputType();
    }

    // 활성화될 때 입력 타입 변경 이벤트를 구독하고 현재 장치 상태를 즉시 반영한다.
    private void OnEnable()
    {
        SubscribeInputTypeEvent();
        ApplyCurrentInputType();
    }

    // 비활성화될 때 입력 타입 변경 이벤트를 해제한다.
    private void OnDisable()
    {
        UnsubscribeInputTypeEvent();
    }

    // AD 안내가 켜져 있을 때만 타이머를 돌려 좌우 아이콘을 번갈아 표시한다.
    private void Update()
    {
        if (!_showAD)
            return;

        _switchTimer += Time.deltaTime;
        if (_switchTimer < _switchInterval)
            return;

        _switchTimer = 0f;
        _showLeftNow = !_showLeftNow;
        RefreshUI();
    }

    // LateUpdate에서 수동 안내 UI의 위치를 플레이어 기준으로 갱신한다.
    private void LateUpdate()
    {
        UpdateUIPos();
    }

    // InputTypeManager의 입력 타입 변경 이벤트를 구독한다.
    private void SubscribeInputTypeEvent()
    {
        if (InputTypeManager.Instance == null)
            return;

        InputTypeManager.Instance.OnInputTypeChanged -= OnInputTypeChanged;
        InputTypeManager.Instance.OnInputTypeChanged += OnInputTypeChanged;
    }

    // InputTypeManager의 입력 타입 변경 이벤트를 해제한다.
    private void UnsubscribeInputTypeEvent()
    {
        if (InputTypeManager.Instance == null)
            return;

        InputTypeManager.Instance.OnInputTypeChanged -= OnInputTypeChanged;
    }

    // 입력 장치가 바뀌면 현재 표시 중인 안내 UI를 새 장치 기준으로 다시 그린다.
    private void OnInputTypeChanged(bool isGamepad)
    {
        UpdateVisualsByDevice(isGamepad);
    }

    // 현재 입력 타입을 매니저에서 가져와 UI에 반영한다.
    private void ApplyCurrentInputType()
    {
        UpdateVisualsByDevice(ResolveCurrentIsGamePad());
    }

    // 현재 입력 타입을 구해온다.
    private bool ResolveCurrentIsGamePad()
    {
        if (InputTypeManager.Instance != null)
            return InputTypeManager.Instance.IsGamePad;

        bool hasGamePad = Gamepad.current != null;
        bool hasKeyboardOrMouse = Keyboard.current != null || Mouse.current != null;
        return hasGamePad && !hasKeyboardOrMouse;
    }

    // 외부나 매니저에서 전달한 입력 장치 상태를 내부 UI 상태로 반영한다.
    public void UpdateVisualsByDevice(bool isGamepad)
    {
        _isGamepad = isGamepad;
        RefreshUI();
    }

    // 현재 장치와 현재 안내 상태에 맞게 실제 아이콘 표시를 갱신한다.
    private void RefreshUI()
    {
        _a?.gameObject.SetActive(false);
        _d?.gameObject.SetActive(false);
        _spacebar?.gameObject.SetActive(false);
        _gamepadLeft?.gameObject.SetActive(false);
        _gamepadRight?.gameObject.SetActive(false);
        _gamepadJump?.gameObject.SetActive(false);

        if (!_isGamepad)
        {
            if (_showAD)
            {
                _a?.gameObject.SetActive(true);
                _d?.gameObject.SetActive(true);
            }

            if (_showSpace)
                _spacebar?.gameObject.SetActive(true);
        }
        else
        {
            if (_showAD)
            {
                if (_showLeftNow)
                    _gamepadLeft?.gameObject.SetActive(true);
                else
                    _gamepadRight?.gameObject.SetActive(true);
            }

            if (_showSpace)
                _gamepadJump?.gameObject.SetActive(true);
        }
    }

    // AD 안내만 켜고 좌우 번갈이 표시 상태를 초기화한다.
    public void ShowOnlyAD()
    {
        _showAD = true;
        _showSpace = false;
        _switchTimer = 0f;
        _showLeftNow = true;
        RefreshUI();
    }

    // 점프/슬램 안내만 켠다.
    public void ShowOnlySpace()
    {
        _showAD = false;
        _showSpace = true;
        RefreshUI();
    }

    // 모든 수동 안내 UI를 끈다.
    public void HideAll()
    {
        _showAD = false;
        _showSpace = false;
        RefreshUI();
    }

    // 플레이어 머리 위 기준으로 수동 안내 UI의 위치를 따라가게 한다.
    private void UpdateUIPos()
    {
        if (_player == null || _manualRect == null || _parentRect == null || _mainCamera == null)
            return;

        Vector3 targetPos = _player.transform.position + _manualOffset;
        Vector2 screenPos = _mainCamera.WorldToScreenPoint(targetPos);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRect, screenPos, _uiCamera, out Vector2 localPoint))
            _manualRect.localPosition = localPoint;
    }
}