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
    [SerializeField] private Sprite _keyboardSprite; // 키보드 입력 중일 때 표시할 스프라이트
    [SerializeField] private Sprite _gamePadSprite; // 게임패드 입력 중일 때 표시할 스프라이트

    private bool _isGamePad; // 마지막으로 감지된 입력 장치가 게임패드인지 저장한다.

    // Retry 액션을 안전하게 꺼내기 위한 프로퍼티다.
    private InputAction RetryAction => _retryActionReference != null ? _retryActionReference.action : null;

    // 오브젝트가 활성화될 때 입력 액션과 입력 장치 감지 이벤트를 등록한다.
    private void OnEnable()
    {
        RetryAction?.Enable();
        RetryAction.performed += OnRetryPerformed;

        InputSystem.onActionChange += OnActionChange;

        RefreshInputIcon();
    }

    // 오브젝트가 비활성화될 때 등록했던 입력 액션과 이벤트를 해제한다.
    private void OnDisable()
    {
        if (RetryAction != null)
            RetryAction.performed -= OnRetryPerformed;

        InputSystem.onActionChange -= OnActionChange;
        RetryAction?.Disable();
    }

    // 시작 시점에 현재 연결 상태를 기준으로 기본 이미지를 세팅한다.
    private void Start()
    {
        _isGamePad = Gamepad.current != null && Keyboard.current == null;
        RefreshInputIcon();
    }

    // Retry 입력이 실제로 수행되었을 때 게임 매니저를 통해 현재 씬을 재시작한다.
    private void OnRetryPerformed(InputAction.CallbackContext context)
    {
        UpdateDeviceFromControl(context.control);

        if (GameManager.Instance != null)
            GameManager.Instance.RestartGame();
    }

    // 입력 시스템에서 액션 변화가 발생할 때 마지막 입력 장치를 감지해 아이콘을 갱신한다.
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

    // 입력 컨트롤의 디바이스 종류를 보고 현재 입력 장치 상태를 갱신한다.
    private void UpdateDeviceFromControl(InputControl control)
    {
        if (control == null || control.device == null)
            return;

        bool nextIsGamePad = control.device is Gamepad;
        if (_isGamePad == nextIsGamePad)
            return;

        _isGamePad = nextIsGamePad;
        RefreshInputIcon();
    }

    // 현재 입력 장치 상태에 맞는 스프라이트를 이미지에 반영한다.
    private void RefreshInputIcon()
    {
        if (_image == null)
            return;

        _image.sprite = _isGamePad ? _gamePadSprite : _keyboardSprite;
    }
}