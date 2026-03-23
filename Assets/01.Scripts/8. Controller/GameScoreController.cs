using UnityEngine;

public class GameScoreController : MonoBehaviour
{

    public int KillScore = 0;
    public int BuildingScore = 0;
    public int BreadScore = 0;

    void OnEnable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.AddListener(MEventType.DestroyEnemy, IncleaseKillScore );
            EventManager.Instance.AddListener(MEventType.DestroyBread, IncleaseBreadScore);
            EventManager.Instance.AddListener(MEventType.DestroyBuilding, IncleaseBuildingScore);
        }
    }

    void OnDisable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.RemoveListener(MEventType.DestroyEnemy, this);
            EventManager.Instance.RemoveListener(MEventType.DestroyBread, this);
            EventManager.Instance.RemoveListener(MEventType.DestroyBuilding, this);
        }
    }

    private void IncleaseKillScore(MEventType type, Component sender, System.EventArgs args) => KillScore++;
    private void IncleaseBuildingScore(MEventType type, Component sender, System.EventArgs args) => BuildingScore++;
    private void IncleaseBreadScore(MEventType type, Component sender, System.EventArgs args) => BreadScore++;

}
