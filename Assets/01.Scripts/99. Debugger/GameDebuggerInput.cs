using UnityEngine;
using UnityEngine.InputSystem;

public class GameDebuggerInput : MonoBehaviour
{
    [SerializeField] private SpawnManager spawnManager;
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
        else if (Keyboard.current.digit5Key.wasPressedThisFrame)
        {
            spawnManager.Spawn(1);
        }
        else if (Keyboard.current.digit6Key.wasPressedThisFrame)
        {
            spawnManager.Spawn(10);
        }
        else if (Keyboard.current.digit7Key.wasPressedThisFrame)
        {
            EnemyHealth[] enemies = Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);

            foreach (var enemy in enemies)
            {
                enemy.Kill();
            }
        }
    }
}
