using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test_SoundManager : MonoBehaviour
{
    [SerializeField] InputActionReference testInput;
    [SerializeField] InputAction testAction;

    [SerializeField] bool playerIsPressed;
    void Start()
    {
        testAction = testInput.action;
    }

    void Update()
    {
        if (testAction.IsPressed())
        {
            playerIsPressed = !playerIsPressed;
        }

        if (playerIsPressed)
        {
            SoundManager.Instance.PlayBGM(MusicTrack.GameScene);
        }
        else
        {
            SoundManager.Instance.StopBGM();
        }
    }
}
