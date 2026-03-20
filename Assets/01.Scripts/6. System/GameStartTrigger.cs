using UnityEngine;

public class GameStartTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            GameManager.Instance.GameStart();
        }
    }
}
