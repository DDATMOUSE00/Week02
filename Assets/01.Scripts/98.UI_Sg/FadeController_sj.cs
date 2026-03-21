using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // DOTween 필수
using System;

public class FadeController_sj : Singleton<FadeController_sj>
{
    [Header("페이드에 사용할 검은 패널 (Image)")]
    [SerializeField] private Image _targetPanel;
    [SerializeField] private float _duration = 0.5f;

    public override void Init() { }

    // 화면이 밝아짐 (까만색 -> 투명)
    public void FadeOut(Action onComplete = null)
    {
        Debug.Log("페이드 왜 안돼2");
        if (_targetPanel == null) return;
        Debug.Log("페이드 왜 안돼3");
        _targetPanel.gameObject.SetActive(true);
        _targetPanel.DOFade(0f, _duration).OnComplete(() => {
            _targetPanel.gameObject.SetActive(false); // 다 밝아지면 클릭 방해 안되게 끔
            onComplete?.Invoke();
        });
    }

    // 화면이 어두워짐 (투명 -> 까만색)
    public void FadeIn(Action onComplete = null)
    {   
        if (_targetPanel == null) return;

        _targetPanel.gameObject.SetActive(true);
        _targetPanel.color = new Color(0, 0, 0, 0); // 시작은 투명하게
        _targetPanel.DOFade(1f, _duration).OnComplete(() => {
            onComplete?.Invoke();
        });
    }
}