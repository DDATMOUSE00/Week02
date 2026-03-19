using UnityEngine;
using UnityEngine.InputSystem;

public class GameDebuggerInput : MonoBehaviour
{
    private void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            GameManager.Instance.DebugSetState(GameState.Ready);
        }
        else if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            GameManager.Instance.GameStart();
        }
        else if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            GameManager.Instance.GameClear();
        }
        else if (Keyboard.current.digit4Key.wasPressedThisFrame)
        {
            GameManager.Instance.GameOver();
        }
    }
}
