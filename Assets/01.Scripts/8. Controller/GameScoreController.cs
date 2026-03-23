using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class GameScoreController : MonoBehaviour
{
    
    [SerializeField] private int KillScore = 0;
    [SerializeField] private int BuildingScore = 0;
    [SerializeField] private int BreadScore = 0;

    [Header("Typing")]
    [SerializeField] private float charsPerSecond = 28f;

    [SerializeField] private TMP_Text _typingText;
    private Coroutine _typingRoutine;
    private bool _isTyping;
    private bool _canSelect;
    private bool _isRestartSelected = true;

    void OnEnable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.AddListener(MEventType.DestroyEnemy, IncreaseKillScore );
            EventManager.Instance.AddListener(MEventType.DestroyBuilding, IncreaseBuildingScore);
            EventManager.Instance.AddListener(MEventType.DestroyBread, IncreaseBreadScore);
        }
    }

    void OnDisable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.RemoveListener(MEventType.DestroyEnemy, this);
            EventManager.Instance.RemoveListener(MEventType.DestroyBread, this);
            EventManager.Instance.RemoveListener(MEventType.DestroyBuilding, this);
        }
        if (_typingRoutine != null)
        {
            StopCoroutine(_typingRoutine);
            _typingRoutine = null;
        }

    }
    void Start()
    {
        _typingText = UIManager.Instance.TypingTextObject.GetComponent<TMP_Text>();
    }
    private void IncreaseKillScore(MEventType type, Component sender, System.EventArgs args) => KillScore++;
    private void IncreaseBuildingScore(MEventType type, Component sender, System.EventArgs args) => BuildingScore++;
    private void IncreaseBreadScore(MEventType type, Component sender, System.EventArgs args) => BreadScore++;
    private void Update()
    {
        if (!_canSelect || _isTyping)
            return;

        bool left = (Keyboard.current?.aKey.wasPressedThisFrame ?? false)
                    || (Keyboard.current?.leftArrowKey.wasPressedThisFrame ?? false)
                    || (Gamepad.current?.dpad.left.wasPressedThisFrame ?? false);

        bool right = (Keyboard.current?.dKey.wasPressedThisFrame ?? false)
                     || (Keyboard.current?.rightArrowKey.wasPressedThisFrame ?? false)
                     || (Gamepad.current?.dpad.right.wasPressedThisFrame ?? false);

        if (left)
            SetSelection(true);
        else if (right)
            SetSelection(false);

        bool confirm = (Keyboard.current?.spaceKey.wasPressedThisFrame ?? false)
                       || (Keyboard.current?.enterKey.wasPressedThisFrame ?? false)
                       || (Keyboard.current?.numpadEnterKey.wasPressedThisFrame ?? false)
                       || (Gamepad.current?.buttonSouth.wasPressedThisFrame ?? false);

        if (confirm)
        {
            if (_isRestartSelected) GameManager.Instance.RestartGame();
            else GameManager.Instance.ExitGame();
        }
    }




    public void EndingText()
    {
         if (_typingText == null)
            return;

        _isTyping = true;
        _canSelect = false;
        _isRestartSelected = true;

        UIManager.Instance.TypingTextObject.SetActive(true);
        UIManager.Instance.RestartButton.SetActive(false);
        UIManager.Instance.ExitButton.SetActive(false);
        UIManager.Instance.RestartArrowButton.SetActive(false);
        UIManager.Instance.EndArrowButton.SetActive(false);

        string endingMessage =

            $"당신이 살포시 보듬어준 시민들 : {KillScore}\n" +
            $"당신이 새 단장을 도와준 건물들 : {BuildingScore}\n" +
            $"당신이 \"합법적\"으로 얻은 붕어빵 : {BreadScore}";

        _typingText.text = string.Empty;

        if (_typingRoutine != null) StopCoroutine(_typingRoutine);
        float interval = 1f / Mathf.Max(1f, charsPerSecond);
        _typingRoutine = StartCoroutine(TypeTextRoutine(endingMessage, interval));

    }

    private IEnumerator TypeTextRoutine(string text, float interval)
        {
            _typingText.text = string.Empty;

            yield return new WaitForSeconds(1f);
            for (int i = 0; i < text.Length; i++)
            {
                _typingText.text += text[i];
                yield return new WaitForSeconds(interval);
            }

            UIManager.Instance.RetryButton.SetActive(true);

            UIManager.Instance.RestartButton.SetActive(true);
            UIManager.Instance.ExitButton.SetActive(true);
            UIManager.Instance.RestartArrowButton.SetActive(true);
            UIManager.Instance.EndArrowButton.SetActive(false);

            _isTyping = false;
            _canSelect = true;
            _typingRoutine = null;
        }
    private void SetSelection(bool restartSelected)
        {
            _isRestartSelected = restartSelected;

            UIManager.Instance.RestartArrowButton.SetActive(restartSelected);
            UIManager.Instance.EndArrowButton.SetActive(!restartSelected);
        }
}


