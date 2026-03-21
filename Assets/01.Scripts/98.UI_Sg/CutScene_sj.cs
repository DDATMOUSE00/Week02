using UnityEngine;
using UnityEngine.UI;

public class CutScene_sj : MonoBehaviour
{
    [Header("Ä«Å÷ ³»¿ë¹°")]
    [SerializeField] private Image[] _start_cutSceneObjects;
    [SerializeField] private Image[] _ending_cutSceneObjects;

    [Header("ÄÆ¼ö ¼ıÀÚ")]
    [SerializeField] private int _start_cutSceneNumber=5;
    [SerializeField] private int _ending_cutSceneNumber=5;

    [Header("ÀÓ½Ã)½ÃÀÛ ÄÆ¾ÀÀÎÁö ³¡ ÄÆ¾ÀÀÎÁö true¸é ¿£µù")]
    [SerializeField] private bool _isEndingCutScene=false;

    private int _currentCutScene=0;

    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CutSceneUpdate();
        PressAnyButton();
        
    }

    private void PressAnyButton()
    {
        if(Input.anyKeyDown)
        {
            _currentCutScene++;
        }
    }
    private void CutSceneUpdate()
    {
        if(_isEndingCutScene=false&&_currentCutScene<_start_cutSceneNumber)
        {
            _start_cutSceneObjects[_currentCutScene].gameObject.SetActive(true);
        }
        else if(_isEndingCutScene=true&&_currentCutScene<_ending_cutSceneNumber)
        {
            _ending_cutSceneObjects[_currentCutScene].gameObject.SetActive(true);
        }
        else
        {
            if (_isEndingCutScene)
            {
                for (int i = 0; i < _ending_cutSceneNumber; i++)
                {
                    _ending_cutSceneObjects[i].gameObject.SetActive(false);
                }

                //¿£µù ÄÆ¾ÀÀÌ ³¡³µÀ» ¶§ °ÔÀÓ Á¾·á
                //GameManager.Instance.GameClear();
            }
            else
            {
                for (int i = 0; i < _start_cutSceneNumber; i++) { 
                    _start_cutSceneObjects[i].gameObject.SetActive(false);
                }

                //½ÃÀÛ ÄÆ¾ÀÀÌ ³¡³µÀ» ¶§ °ÔÀÓ ½ÃÀÛ
                //GameManager.Instance.TutorialStart();
                _currentCutScene = 0; // ÄÆ¾À ÃÊ±âÈ­
            }
        }
    }
}
