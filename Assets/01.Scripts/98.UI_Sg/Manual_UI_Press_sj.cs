using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Manual_Ui_Press_sg : MonoBehaviour
{
    [Header("입력 액션 레퍼런스")]
    [SerializeField] private InputActionReference _navigateActionReference;
    [SerializeField] private InputActionReference _spaceActionReference;

    [Header("키보드 자판 아이콘")]
    [SerializeField] private Image _a;
    [SerializeField] private Image _d;
    [SerializeField] private Image _spacebar;

    [Header("패드 자판 아이콘")]
    [SerializeField] private Image _jumpStick;

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

    private bool _stageStart = false;
    

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
            // 추가

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
      
        _a.rectTransform.sizeDelta = new Vector2(_a.rectTransform.sizeDelta.x, _adPressedSizeY);
        _a.color = _adPressedColor;
            
    }

    private void ExecuteRightAction()
    {
        if (_d == null) return;
         _d.rectTransform.sizeDelta = new Vector2(_d.rectTransform.sizeDelta.x, _adPressedSizeY);
         _d.color = _adPressedColor;
     
      
    }

    private void ExecuteSpacebarAction()
    {
        if (_spacebar == null) return;
        
         _spacebar.rectTransform.sizeDelta = new Vector2(_spacebar.rectTransform.sizeDelta.x, _spacePressedSizeY);
         _spacebar.color = _spacePressedColor;
        _jumpStick.rectTransform.sizeDelta = new Vector2(_jumpStick.rectTransform.sizeDelta.x, _spacePressedSizeY);
        _jumpStick.color = _spacePressedColor;


    }

    private void ResetLeftAction()
    {
        if (_a == null) return;
      
         _a.rectTransform.sizeDelta = new Vector2(_a.rectTransform.sizeDelta.x, _adDefaultSizeY);
         _a.color = _adDefaultColor;
     
      
    }

    private void ResetRightAction()
    {
        if (_d == null) return;

       
         _d.rectTransform.sizeDelta = new Vector2(_d.rectTransform.sizeDelta.x, _adDefaultSizeY);
         _d.color = _adDefaultColor;
 
     
    }

    private void ResetSpacebarAction()
    {
        if (_spacebar == null) return;
        _spacebar.rectTransform.sizeDelta = new Vector2(_spacebar.rectTransform.sizeDelta.x, _spaceDefaultSizeY);
        _spacebar.color = _spaceDefaultColor;
        _jumpStick.rectTransform.sizeDelta = new Vector2(_jumpStick.rectTransform.sizeDelta.x, _spaceDefaultSizeY);
        _jumpStick.color = _spaceDefaultColor;

    }

    #endregion

    #region 이벤트

    private void OnGameFail(MEventType MEventType, Component Sender, EventArgs args)
    {
        _stageStart = false;
    }

    private void OnGameStart(MEventType MEventType, Component Sender, EventArgs args)
    {
        _stageStart = true;
    }


    #endregion
}