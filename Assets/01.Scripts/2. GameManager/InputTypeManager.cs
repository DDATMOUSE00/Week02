using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum InputType
{
    KeyboardMouse,
    GamePad
}

public class InputTypeManager : Singleton<InputTypeManager>
{
    private InputType _currentInputType = InputType.KeyboardMouse;
    private bool _isSubscribed;

    public InputType CurrentInputType => _currentInputType;
    public bool IsGamePad => _currentInputType == InputType.GamePad;

    public event Action<bool> OnInputTypeChanged;

    // 입력 장치 감지 이벤트를 등록하고 현재 연결 상태를 기준으로 초기 입력 타입을 판별한다.
    public override void Init()
    {
        SubscribeInputSystem();
        RefreshCurrentInputType();
    }

    // Awake 시 싱글톤 초기화와 입력 타입 초기화를 수행한다.
    protected override void Awake()
    {
        base.Awake();
        Init();
    }

    // 파괴될 때 전역 입력 이벤트를 해제한다.
    private void OnDestroy()
    {
        UnsubscribeInputSystem();
    }

    // InputSystem 전역 액션 변경 이벤트를 한 번만 구독한다.
    private void SubscribeInputSystem()
    {
        if (_isSubscribed)
            return;

        InputSystem.onActionChange += OnActionChange;
        _isSubscribed = true;
    }

    // 등록했던 InputSystem 전역 이벤트를 안전하게 해제한다.
    private void UnsubscribeInputSystem()
    {
        if (!_isSubscribed)
            return;

        InputSystem.onActionChange -= OnActionChange;
        _isSubscribed = false;
    }

    // 현재 연결 상태를 기준으로 기본 입력 타입을 판별한다.
    public void RefreshCurrentInputType()
    {
        bool hasGamePad = Gamepad.current != null;
        bool hasKeyboardOrMouse = Keyboard.current != null || Mouse.current != null;

        bool isGamePad = hasGamePad && !hasKeyboardOrMouse;
        SetInputType(isGamePad ? InputType.GamePad : InputType.KeyboardMouse, true);
    }

    // 액션 시작/실행 시점의 activeControl을 보고 마지막 입력 장치를 갱신한다.
    private void OnActionChange(object obj, InputActionChange change)
    {
        if (change != InputActionChange.ActionStarted &&
            change != InputActionChange.ActionPerformed)
            return;

        if (obj is not InputAction action)
            return;

        if (action.activeControl == null)
            return;

        UpdateDeviceFromControl(action.activeControl);
    }

    // 전달받은 컨트롤의 디바이스 종류를 보고 입력 타입을 갱신한다.
    public void UpdateDeviceFromControl(InputControl control)
    {
        if (control == null || control.device == null)
            return;

        if (control.device is Gamepad)
            SetInputType(InputType.GamePad);
        else
            SetInputType(InputType.KeyboardMouse);
    }

    // 입력 타입이 실제로 바뀌었을 때만 상태를 저장하고 이벤트를 발행한다.
    private void SetInputType(InputType nextType, bool forceNotify = false)
    {
        if (!forceNotify && _currentInputType == nextType)
            return;

        _currentInputType = nextType;
        OnInputTypeChanged?.Invoke(IsGamePad);
    }
}