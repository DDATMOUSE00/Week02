using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    public override void Init() { }

    //모든 UI 레퍼런스가 여기에 들어가야함.

    [SerializeField] private GameObject ClearUI;
    [SerializeField] private GameObject GameOverUI;


    public void GameClearUIActivate()
    {
        ClearUI.SetActive(true);
    }

    public void GameOverUIActivate()
    {
        GameOverUI.SetActive(true);
    }

}
