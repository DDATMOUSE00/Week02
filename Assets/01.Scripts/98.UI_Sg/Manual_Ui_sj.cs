using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Manual_Ui_sg : MonoBehaviour
{
    private InputSystem_Actions _controls;
    [Header("Ű���� Ű �Ҵ�")]
    [SerializeField] private Image _a;
    [SerializeField] private Image _d;
    [SerializeField] private Image _spacebar;

    private bool _stageStart = false;

    private void Awake()
    {

        _controls = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        if (EventManager.Instance == null)
            return;

        EventManager.Instance.AddListener(MEventType.StageStarted, OnGameStart);
        EventManager.Instance.AddListener(MEventType.StageFailed, OnGameFail);

        _controls.UI.Enable();


        _controls.UI.Navigate.performed += OnNavigatePerformed;
        _controls.UI.Navigate.canceled += OnNavigatePerformed;



    }

    private void OnDisable()
    {
        if (EventManager.Instance == null)
            return;

        EventManager.Instance.RemoveListener(MEventType.StageStarted, this);
        EventManager.Instance.RemoveListener(MEventType.StageFailed, this);

        _controls.UI.Navigate.performed -= OnNavigatePerformed;
        _controls.UI.Navigate.canceled -= OnNavigatePerformed;
        _controls.UI.Disable();
    }

    private void OnNavigatePerformed(InputAction.CallbackContext context)
    {

        Vector2 navigationInput = context.ReadValue<Vector2>();

        if (navigationInput.x < -0.5f)
        {
            Debug.Log("Left Input");
            ExecuteLeftAction();
        }
        else if (navigationInput.x > 0.5f)
        {
            Debug.Log("Right Input");
            ExecuteRightAction();
        }
        else if (navigationInput.x >= -0.5f && navigationInput.x <= 0.5f)
        {
            ResetLeftAction();
            ResetRightAction();
        }
    }

    private void ExecuteLeftAction()
    {
        RectTransform rect = _a.GetComponent<RectTransform>();

        rect.sizeDelta = new Vector2(rect.sizeDelta.x, 80f);
        Image img = _a.GetComponent<Image>();
        img.color = new Color(123f / 255f, 123f / 255f, 123f / 255f, 1f);
    }

    private void ResetLeftAction()
    {
        if (_a == null) return;
        RectTransform rect = _a.GetComponent<RectTransform>();
        if (rect != null) rect.sizeDelta = new Vector2(rect.sizeDelta.x, 100f);
        Image img = _a.GetComponent<Image>();
        if (img != null) img.color = new Color(1f, 1f, 1f, 1f);
    }

    private void ResetRightAction()
    {
        if (_d == null) return;
        RectTransform rect = _d.GetComponent<RectTransform>();
        if (rect != null) rect.sizeDelta = new Vector2(rect.sizeDelta.x, 100f);
        Image img = _d.GetComponent<Image>();
        if (img != null) img.color = new Color(1f, 1f, 1f, 1f);
    }

    private void ExecuteRightAction()
    {
        RectTransform rect = _d.GetComponent<RectTransform>();

        rect.sizeDelta = new Vector2(rect.sizeDelta.x, 80f);
        Image img = _d.GetComponent<Image>();
        img.color = new Color(123f / 255f, 123f / 255f, 123f / 255f, 1f);
    }
    private void OnGameFail(MEventType MEventType, Component Sender, EventArgs args)
    {
        _stageStart = false;
    }

    private void OnGameStart(MEventType MEventType, Component Sender, EventArgs args)
    {
        _stageStart = true;
    }
}

