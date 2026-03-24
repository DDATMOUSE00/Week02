using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class CutScenePressXTmp : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private string _keyboardText = "Press Spacebar";
    [SerializeField] private string _gamePadText = "Press X";

    private TMP_Text _tmp;

    // 에디터에서 컴포넌트가 바뀌면 TMP 참조를 다시 잡는다.
    private void OnValidate()
    {
        _tmp = GetComponent<TMP_Text>();
    }

    // 시작 전에 TMP 참조를 안전하게 캐싱한다.
    private void Awake()
    {
        _tmp = GetComponent<TMP_Text>();
    }

    // 활성화될 때 입력 타입 변경 이벤트를 구독하고 현재 상태를 즉시 반영한다.
    private void OnEnable()
    {
        SubscribeInputTypeEvent();
        RefreshText();
    }

    // 비활성화될 때 등록했던 이벤트를 해제한다.
    private void OnDisable()
    {
        UnsubscribeInputTypeEvent();
    }

    // InputTypeManager 이벤트를 구독한다.
    private void SubscribeInputTypeEvent()
    {
        if (InputTypeManager.Instance == null)
            return;

        InputTypeManager.Instance.OnInputTypeChanged -= HandleInputTypeChanged;
        InputTypeManager.Instance.OnInputTypeChanged += HandleInputTypeChanged;
    }

    // InputTypeManager 이벤트를 해제한다.
    private void UnsubscribeInputTypeEvent()
    {
        if (InputTypeManager.Instance == null)
            return;

        InputTypeManager.Instance.OnInputTypeChanged -= HandleInputTypeChanged;
    }

    // 입력 타입 변경 이벤트를 받아 텍스트를 갱신한다.
    private void HandleInputTypeChanged(bool isGamePad)
    {
        SetText(isGamePad);
    }

    // 현재 입력 타입을 기준으로 텍스트를 즉시 갱신한다.
    private void RefreshText()
    {
        if (_tmp == null)
            return;

        if (InputTypeManager.Instance == null)
        {
            SetText(false);
            return;
        }

        SetText(InputTypeManager.Instance.IsGamePad);
    }

    // 실제 TMP 문구를 입력 타입에 맞게 설정한다.
    private void SetText(bool isGamePad)
    {
        if (_tmp == null)
            return;

        _tmp.text = isGamePad ? _gamePadText : _keyboardText;
    }
}