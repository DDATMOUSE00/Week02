using UnityEngine;

public class GameStartTrigger : MonoBehaviour
{

    public enum DebugTriggerSet
    {
        StartCutScene,
        StartTutorial,
        EndingCutScene,
        GameOVerTest,
        GameClearTest
    }
    [SerializeField] private DebugTriggerSet debugTriggerSet;



    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {

            if(gameObject.GetComponent<GameStartTrigger>().debugTriggerSet == DebugTriggerSet.StartTutorial)
            {
                GameManager.Instance.TutorialStart();

            }
        }
    }
}
