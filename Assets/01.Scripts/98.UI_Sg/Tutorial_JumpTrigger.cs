using UnityEngine;

public class Tutorial_JumpTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 하이어라키의 플레이어 오브젝트 Tag가 "Player"인지 확인하세요.
        if (collision.CompareTag("Player"))
        {
            // 2. 싱글톤 인스턴스를 통해 매니저 호출
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.OnTrigger1Entered();
                Debug.Log("Trigger 1: Player Detected, Step Changed.");
            }

            // 3. 중복 방지를 위해 오브젝트 비활성화
            gameObject.SetActive(false);
        }
    }
}