
using UnityEngine;
using UnityEngine.InputSystem;

public class Test_SoundManager : MonoBehaviour
{

    void Update()
    {
        // Get the current keyboard state
        var keyboard = Keyboard.current;
        if (keyboard == null) return; // Case where no keyboard is plugged in

        // --- BGM Testing ---

        if (keyboard.digit2Key.wasPressedThisFrame)
            SoundManager.Instance.PlayBGM(BGM.GameScene);

        if (keyboard.digit3Key.wasPressedThisFrame)
            SoundManager.Instance.PlayBGM(BGM.GameOver);

        if (keyboard.digit4Key.wasPressedThisFrame)
            SoundManager.Instance.PlayBGM(BGM.GameClear);

        if (keyboard.digit4Key.wasPressedThisFrame)
            SoundManager.Instance.PlayBGM(BGM.Tutorial);


        // --- SFX Testing ---
        if (keyboard.spaceKey.wasPressedThisFrame)
            SoundManager.Instance.PlaySFX(PlayerSFX.Jump);

        if (keyboard.fKey.wasPressedThisFrame)
            SoundManager.Instance.PlaySFX(PlayerSFX.SlamHeavy);

        if (keyboard.rKey.wasPressedThisFrame)
            SoundManager.Instance.PlaySFX(PlayerSFX.SlamLight);

        if (keyboard.gKey.wasPressedThisFrame)
            SoundManager.Instance.PlaySFX(PlayerSFX.Charge);

        if (keyboard.hKey.wasPressedThisFrame)
            SoundManager.Instance.PlaySFX(PlayerSFX.Levelup);
            
        if (keyboard.jKey.wasPressedThisFrame)
            SoundManager.Instance.PlaySFX(PlayerSFX.Ping);

        // --- SFX Testing ---
        if (keyboard.zKey.wasPressedThisFrame)
            SoundManager.Instance.PlaySFX(EnemySFX.Panic);

        if (keyboard.xKey.wasPressedThisFrame)
            SoundManager.Instance.PlaySFX(EnemySFX.Dead);

        if (keyboard.cKey.wasPressedThisFrame)
            SoundManager.Instance.PlaySFX(EnemySFX.Explosion);

        // --- Utility Testing ---
        if (keyboard.escapeKey.wasPressedThisFrame)
            SoundManager.Instance.StopBGM();
    }

}
