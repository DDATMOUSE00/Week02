using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameRestartSystem : MonoBehaviour
{
    [Header("Input Reference")]
    [SerializeField] private InputActionReference _retryActionReference;

    [Header("UI Image")]
    [SerializeField] private Image _image; // 현재 입력 장치에 따라 표시 이미지를 바꿀 대상 이미지
    [Space(10)]
    [SerializeField] private Sprite _keyboardSprite; // 키보드/마우스 입력 중일 때 표시할 스프라이트
    [SerializeField] private Sprite _gamePadSprite; // 게임패드 입력 중일 때 표시할 스프라이트

    // Retry 액션을 안전하게 꺼내기 위한 프로퍼티다.
    private InputAction RetryAction => _retryActionReference != null ? _retryActionReference.action : null;

    // 오브젝트가 활성화될 때 입력 액션과 입력 타입 변경 이벤트를 등록한다.
    private void OnEnable()
    {
        RetryAction?.Enable();

        if (RetryAction != null)
            RetryAction.performed += OnRetryPerformed;

        SubscribeInputTypeEvent();
        ApplyCurrentInputType();
    }

    // 오브젝트가 비활성화될 때 등록했던 입력 액션과 이벤트를 해제한다.
    private void OnDisable()
    {
        if (RetryAction != null)
            RetryAction.performed -= OnRetryPerformed;

        UnsubscribeInputTypeEvent();
        RetryAction?.Disable();
    }

    // 시작 시점에도 현재 입력 장치 기준으로 아이콘을 한 번 더 보정한다.
    private void Start()
    {
        ApplyCurrentInputType();
    }

    // InputTypeManager의 입력 타입 변경 이벤트를 구독한다.
    private void SubscribeInputTypeEvent()
    {
        if (InputTypeManager.Instance == null)
            return;

        InputTypeManager.Instance.OnInputTypeChanged -= OnInputTypeChanged;
        InputTypeManager.Instance.OnInputTypeChanged += OnInputTypeChanged;
    }

    // InputTypeManager의 입력 타입 변경 이벤트 구독을 해제한다.
    private void UnsubscribeInputTypeEvent()
    {
        if (InputTypeManager.Instance == null)
            return;

        InputTypeManager.Instance.OnInputTypeChanged -= OnInputTypeChanged;
    }

    // Retry 입력이 실제로 수행되었을 때 마지막 입력 장치를 갱신하고 게임을 재시작한다.
    private void OnRetryPerformed(InputAction.CallbackContext context)
    {
        if (InputTypeManager.Instance != null)
            InputTypeManager.Instance.UpdateDeviceFromControl(context.control);

        if (GameManager.Instance != null)
            GameManager.Instance.RestartGame();
    }

    // 입력 타입이 바뀌면 전달받은 상태를 기준으로 아이콘을 갱신한다.
    private void OnInputTypeChanged(bool isGamePad)
    {
        RefreshInputIcon(isGamePad);
    }

    // 현재 InputTypeManager 상태를 읽어 아이콘을 반영한다.
    private void ApplyCurrentInputType()
    {
        RefreshInputIcon(ResolveCurrentIsGamePad());
    }

    // 현재 입력 장치가 게임패드인지 판별한다.
    private bool ResolveCurrentIsGamePad()
    {
        if (InputTypeManager.Instance != null)
            return InputTypeManager.Instance.IsGamePad;

        bool hasGamePad = Gamepad.current != null;
        bool hasKeyboardOrMouse = Keyboard.current != null || Mouse.current != null;
        return hasGamePad && !hasKeyboardOrMouse;
    }

    // 전달받은 입력 장치 상태에 맞는 스프라이트를 이미지에 반영한다.
    private void RefreshInputIcon(bool isGamePad)
    {
        if (_image == null)
            return;

        _image.sprite = isGamePad ? _gamePadSprite : _keyboardSprite;
    }
}