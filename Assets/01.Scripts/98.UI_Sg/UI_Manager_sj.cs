using System;
using UnityEngine;

public class UIManager_sj : MonoBehaviour
{
    [Header("���̾��Ű���� ���� �׷�� �Ҵ�")]
 
    [SerializeField] private GameObject _startCutSceneGroup;
    [SerializeField] private GameObject _tutorialGroup;


    // EventManager�� ���� ���� ��ȣ�� ��� �ڵ����� �����
    private void OnStateChanged(MEventType type, Component sender, EventArgs args)
    {
        if (args is GameStateChangedEventArgs stateArgs)
        {
            // 1. �ϴ� ��� UI �׷��� �� (�迭 �밡�� ���� �׷� 3���� ���� ��)
          
            if (_startCutSceneGroup != null) _startCutSceneGroup.SetActive(false);
            if (_tutorialGroup != null) _tutorialGroup.SetActive(false);

            // 2. ���� ���¿� �´� �׷� �� �ϳ��� ��
            //�����ƾ� ���߿� gameclear�� game over�� �߰��ض�
            switch (stateArgs.current)
            {

                case GameState.StartCutScene:
                    if (_startCutSceneGroup != null) _startCutSceneGroup.SetActive(true);
                    break;
                case GameState.Tutorial:
                    //Debug.Log("���̵� �� �ȵ�4");
                    if (_tutorialGroup != null){
                        //Debug.Log("���̵� �� �ȵ�");
                        //FadeController_sj.Instance.FadeOut();
                        _tutorialGroup.SetActive(true); 
                    
                    }
                    break;
                case GameState.Play:
                    // ������ ���� �� Ʃ�丮�� UI�� ������ �ΰ��� ���� ��� Ȱ��ȭ
                    break;
            }
        }
    }
}