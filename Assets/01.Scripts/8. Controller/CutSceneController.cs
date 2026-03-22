using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CutSceneController : MonoBehaviour
{
    [Header("Cartoon Image")]
    [SerializeField] private Image[] _start_cutSceneObjects;
    //[SerializeField] private Image[] _ending_cutSceneObjects;


    
    [Header("Cartoon Background")]
    [SerializeField] private GameObject _cutScene_Background;
        
    private int StartCutSceneCount => _start_cutSceneObjects?.Length ?? 0;
    //private int EndingCutSceneCount => _ending_cutSceneObjects?.Length ?? 0;
    
    private int _currentCutScene = 0;
    private bool _isCutSceneFinished = false;

    private void OnEnable()
    {
        if (EventManager.Instance == null) return;
        EventManager.Instance.AddListener(MEventType.StartingCutScene, OnCutsceneStart); // 씬 시작했을떄
        EventManager.Instance.AddListener(MEventType.TutorialStarted, OnCutsceneEnd); // 컷씬 종료 분기

    }

    private void OnDisable()
    {
        if (EventManager.Instance == null) return;
        EventManager.Instance.RemoveListener(MEventType.StartingCutScene, this);
        EventManager.Instance.RemoveListener(MEventType.TutorialStarted, this);

    }

    private void OnCutsceneStart(MEventType MEventType, Component Sender, EventArgs args)
    {
        _currentCutScene = 0;
        _isCutSceneFinished = false;

        if (_cutScene_Background != null)
            _cutScene_Background.SetActive(true);

        SetAllCutScenesActive(false);
        
        // 처음 시작했을때 첫번째 컷씬을 나오게 할거임?
        /*if (StartCutSceneCount > 0 && _start_cutSceneObjects[0] != null)
           _start_cutSceneObjects[0].gameObject.SetActive(true);*/
    }
    private void OnCutsceneEnd(MEventType MEventType, Component Sender, EventArgs args)
    {

        _isCutSceneFinished = true;
        _currentCutScene = 0;

        SetAllCutScenesActive(false);

        if (_cutScene_Background != null)
            _cutScene_Background.SetActive(false);
    }

    private void Update()
        {   
            if (GameManager.Instance == null) return;

            if (_isCutSceneFinished) return;
            
            if (IsAnyInputPressedThisFrame())
            {
                ShowNextCutScene();
            }

        }
    private void ShowNextCutScene()
    {
        if (_currentCutScene >= StartCutSceneCount)
        {
            _isCutSceneFinished = true;
            GameManager.Instance.TutorialStart();
            return;
        }

        if (_start_cutSceneObjects[_currentCutScene] != null)
        {
            _start_cutSceneObjects[_currentCutScene].gameObject.SetActive(true);
        }
        _currentCutScene++;
    }
    private void SetAllCutScenesActive(bool isActive)
    {
        if (_start_cutSceneObjects == null) return;

        for (int i = 0; i < _start_cutSceneObjects.Length; i++)
        {
            if (_start_cutSceneObjects[i] != null)
                _start_cutSceneObjects[i].gameObject.SetActive(isActive);
        }
    }
    private bool IsAnyInputPressedThisFrame()
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) return true;
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) return true;
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame) return true;

        if (Gamepad.current != null)
        {
            if (Gamepad.current.buttonSouth.wasPressedThisFrame) return true;
            if (Gamepad.current.startButton.wasPressedThisFrame) return true;
            if (Gamepad.current.selectButton.wasPressedThisFrame) return true;
            if (Gamepad.current.dpad.up.wasPressedThisFrame) return true;
            if (Gamepad.current.dpad.down.wasPressedThisFrame) return true;
            if (Gamepad.current.dpad.left.wasPressedThisFrame) return true;
            if (Gamepad.current.dpad.right.wasPressedThisFrame) return true;
        }

        return false;
    }
}

    



