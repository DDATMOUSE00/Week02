using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

// 1. Singleton<T>�� ��ӹ޵��� �����մϴ�.
public class CutScene_sj : MonoBehaviour
{
    [Header("ī�� ���빰")]
    [SerializeField] private Image[] _start_cutSceneObjects;
    [SerializeField] private Image[] _ending_cutSceneObjects;

    
    private int _start_cutSceneNumber = 5;
    private int _ending_cutSceneNumber = 5;

    [Header("���� ���")]
    [SerializeField] private GameObject _cutScene_Background;

    [Header("�ӽ�)���� �ƾ����� �� �ƾ����� true�� ����")]
    [SerializeField] private bool _isEndingCutScene = false;

    

    private int _currentCutScene = 0;

    // [�߰�1] �ƾ��� ������ ������ Update�� 1�ʿ� 60���� Ʃ�丮���� �θ��� �� ���� �ڹ���
    private bool _isCutSceneFinished = false;

    // 2. Singleton Ŭ�������� �䱸�ϴ� �߻� �޼��� Init�� �����մϴ�.
   

    private void OnEnable()
    {
        if (EventManager.Instance == null) return;
        //EventManager.Instance.AddListener(MEventType.GameStateChanged, OnCutsceneEnd);
        //EventManager.Instance.AddListener(MEventType.StartCutScene, OnCutsceneStart);
    }

    private void OnDisable()
    {
        if (EventManager.Instance == null) return;
        //EventManager.Instance.RemoveListener(MEventType.GameStateChanged, this);
        //EventManager.Instance.RemoveListener(MEventType.StartingCutScene, this);
    }

    void Start() {
        _cutScene_Background.SetActive(true);
    }

    void Update()
    {   // [�߰�] ���� ���� ���°� �ƾ� ���°� �ƴϸ� �� ��ũ��Ʈ�� �۵���Ű�� ����
        if (GameManager.Instance.CurrentState != GameState.StartCutScene) return;
        // [�߰�2] �ƾ��� �� ���� ���¸� Update ������ ������ ������Ŵ
        if (_isCutSceneFinished) return;

        CutSceneUpdate();
        PressAnyButton();
    }

    private void PressAnyButton()
    {
        if (Input.anyKeyDown)
        {
            _currentCutScene++;
        }
    }

    private void CutSceneUpdate()
    {
        // [����1] = �� == �� ��ħ (�� ����)
        if (_isEndingCutScene == false && _currentCutScene < _start_cutSceneNumber)
        {
            _start_cutSceneObjects[_currentCutScene].gameObject.SetActive(true);
        }
        else if (_isEndingCutScene == true && _currentCutScene < _ending_cutSceneNumber)
        {
            _ending_cutSceneObjects[_currentCutScene].gameObject.SetActive(true);
        }
        else
        {
            // ��� �ƾ��� ���� �� �� �� �� ������ else�� �Ѿ���� ��
            // �ڹ��踦 �ɾ� ���� �ݺ� ������ ����
            _isCutSceneFinished = true;

            if (_isEndingCutScene)
            {
                for (int i = 0; i < _ending_cutSceneNumber; i++)
                {
                    _ending_cutSceneObjects[i].gameObject.SetActive(false);
                }

                //���� �ƾ��� ������ �� ���� ����
                //GameManager.Instance.GameClear();
            }
            //��ŸƮ �ƾ��� ������ �� Ʃ�丮�� ���� �� ���̵� �ƿ��� ���⼭ ó��
            else
            {
                for (int i = 0; i < _start_cutSceneNumber; i++)
                {
                    _start_cutSceneObjects[i].gameObject.SetActive(false);
                }
                // _cutScene_Background�� GameObject�� ��
                //_cutScene_Background.GetComponent<Image>().DOFade(1f, 1f); // �ƾ��� ���� �� ��� ���̵� ��
                //// [�߰�3] �ƾ��� ������ �� ���̵� �ƿ� �� Ʃ�丮�� ����
                //FadeController_sj fade = FindObjectOfType<FadeController_sj>();
                //if (fade != null) fade.FadeOut(_cutScene_Background);

                if (GameManager.Instance != null)

                {
                    Debug.Log("�ƾ� ��, Ʃ�丮�� ����");
                    GameManager.Instance.TutorialStart();
                    TutorialManager.Instance.StartTutorial();
                }

                _currentCutScene = 0; // �ƾ� �ʱ�ȭ
            }
        }
    }

    private void OnCutsceneStart(MEventType MEventType, Component Sender, EventArgs args)
    {
        _currentCutScene = 0;
        _isCutSceneFinished = false;

        // 1. ���� ������Ʈ�� �մϴ�.
        //_cutScene_Background.SetActive(true);

        // 2. ��� �̹����� ã�Ƽ�
        //Image bgImage = _cutScene_Background.GetComponent<Image>();

        // 3. ���� '��Ÿ����' �ϰ� �ʹٸ� (���� 0 -> 1)
        //bgImage.color = new Color(bgImage.color.r, bgImage.color.g, bgImage.color.b, 0f);
        //bgImage.DOFade(0f, 1f);
        foreach (var img in _start_cutSceneObjects) if (img != null) img.gameObject.SetActive(false);
        foreach (var img in _ending_cutSceneObjects) if (img != null) img.gameObject.SetActive(false);

        CutSceneUpdate();
    }

    private void OnCutsceneEnd(MEventType MEventType, Component Sender, EventArgs args)
    {
        _currentCutScene = 0;

        //// ��ŸƮ �ƾ��̸� �ƾ� ���� ���� �� ���̵� �ƿ�
        //if (_isEndingCutScene == false)
        //{
        //    // ���⼭ FadeOut�� �� �θ��� Update ���̶� �ߺ� ����ǹǷ� 
        //    // ���� �����ϰ� ���� CutSceneUpdate�� else �ȿ����� ó���ϵ��� �����
        //}

        foreach (var img in _start_cutSceneObjects) if (img != null) img.gameObject.SetActive(false);
        foreach (var img in _ending_cutSceneObjects) if (img != null) img.gameObject.SetActive(false);
    }

    // [�߰�4] StartScene_sj ���� ��ư���� �ƾ��� �θ��� ���� �ٸ� ����
  
}