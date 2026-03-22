using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    [SerializeField] private GameObject _restartarrow;
    [SerializeField] private GameObject _exitarrow;
    //public Sprite Hover_img;
    //public Sprite No_hover_img;
    //Image thisImg;

    // Start is called before the first frame update
    void Start()
    {
        //thisImg = GetComponent<Image>();
    }
    private void OnEnable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.AddListener(MEventType.StageCleared, OnGameCleared);
            EventManager.Instance.AddListener(MEventType.StageFailed, OnGameFailed);
        }

    }
    private void OnDisable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.RemoveListener(MEventType.StageStarted, this);
            EventManager.Instance.RemoveListener(MEventType.StageCleared, this);
        }

    }

    private void OnGameCleared(MEventType type, Component sender, System.EventArgs args)
    {
        _restartarrow.SetActive(false);
        _exitarrow.SetActive(false);
    }
    private void OnGameFailed(MEventType type, Component sender, System.EventArgs args)
    {
        _restartarrow.SetActive(false);
        _exitarrow.SetActive(false);
    }

    public void RestartButtonUI_In()
    {
        //thisImg.sprite = Hover_img;
        _restartarrow.SetActive(true);
   
    }

    public void RestartButtonUI_out()
    {
        //thisImg.sprite = No_hover_img;
        _restartarrow.SetActive(false);
    
    }
    public void ExitButtonUI_In()
    {
    
        _exitarrow.SetActive(true);
    }

    public void ExitButtonUI_out()
    {
     
        _exitarrow.SetActive(false);
    }
}