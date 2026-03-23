using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CutSceneController : MonoBehaviour
{
    private enum CutSceneType { None, Start, EndingA, EndingB }

    [Header("Cartoon Image")]
    [SerializeField] private Image[] _startCutScenes;
    [SerializeField] private Image[] _endingACutScenes;
    [SerializeField] private Image[] _endingBCutScenes;
    
    
    
    [Header("Cartoon Background")]
    [SerializeField] private GameObject _background;
        
    private CutSceneType _currentType = CutSceneType.None;
    private int _currentIndex = 0;
    private bool _isPlaying = false;
    

    private void OnEnable()
    {
        if (EventManager.Instance == null) return;
        
            EventManager.Instance.AddListener(MEventType.StartingCutScene, OnStartingCutScene);
            EventManager.Instance.AddListener(MEventType.StageCleared, OnStageCleared);
            EventManager.Instance.AddListener(MEventType.StageFailed, OnStageFailed); 
        
        
    }

    private void OnDisable()
    {
        if (EventManager.Instance == null) return;
        EventManager.Instance.RemoveListener(MEventType.StartingCutScene, this);
        EventManager.Instance.RemoveListener(MEventType.StageCleared, this);
        EventManager.Instance.RemoveListener(MEventType.StageFailed, this);
    }

    private void OnStartingCutScene(MEventType t, Component s, EventArgs a) => StartCutScene(CutSceneType.Start);
    private void OnStageCleared(MEventType t, Component s, EventArgs a) => StartCutScene(CutSceneType.EndingA);
    private void OnStageFailed(MEventType t, Component s, EventArgs a) => StartCutScene(CutSceneType.EndingB);

    private void Update()
        {   
            if (_isPlaying == false) return;
            if (!IsAnyInputPressedThisFrame()) return;

            ShowNextCutScene();
        }
    
    private void ShowNextCutScene()
    {
        Image[] frames = GetCurrentFrames();
        int count = frames == null ? 0 : frames.Length;

        if (_currentIndex >= count)
        {
            EndCutScene();
            return;
        }

        if (frames[_currentIndex] != null)
        {
            GameObject currentFrameObject = frames[_currentIndex].gameObject;
            currentFrameObject.SetActive(true);

            CutSceneFrameMotion frameMotion = currentFrameObject.GetComponent<CutSceneFrameMotion>();
            if (frameMotion != null) frameMotion.Play();
        }

        _currentIndex++;
    }
    private void StartCutScene(CutSceneType type)
    {
        _currentType = type;
        _currentIndex = 0;
        _isPlaying = true;

        SetAllInactive(_startCutScenes);
        SetAllInactive(_endingACutScenes);
        SetAllInactive(_endingBCutScenes);

        if (_background != null) _background.SetActive(true);
    }
    private void EndCutScene()
    {
        SetAllInactive(GetCurrentFrames());
        if (_background != null) _background.SetActive(false);

        _isPlaying = false;

        if (GameManager.Instance == null) return;

        if (_currentType == CutSceneType.Start) GameManager.Instance.TutorialStart();
        else if (_currentType == CutSceneType.EndingA) GameManager.Instance.ClearCutSceneFinished();
        else if (_currentType == CutSceneType.EndingB) GameManager.Instance.GameOverCutSceneFinished();

        _currentType = CutSceneType.None;
    }

    private Image[] GetCurrentFrames()
    {
        if (_currentType == CutSceneType.Start) return _startCutScenes;
        if (_currentType == CutSceneType.EndingA) return _endingACutScenes;
        if (_currentType == CutSceneType.EndingB) return _endingBCutScenes;
        return null;
    }

    private void SetAllInactive(Image[] arr)
    {
        if (arr == null) return;
        for (int i = 0; i < arr.Length; i++)
            if (arr[i] != null) arr[i].gameObject.SetActive(false);
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

    



