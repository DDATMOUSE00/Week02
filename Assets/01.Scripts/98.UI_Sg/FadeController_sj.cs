using System;
using System.Collections;
using UnityEngine;

public class FadeController_sj : MonoBehaviour // Panel 불투명도 조절해 페이드인 or 페이드아웃
{
    public bool IsFadeIn; // true=FadeIn, false=FadeOut
    //public GameObject Panel; // 불투명도를 조절할 Panel 오브젝트
    private Action _onCompleteCallback; // FadeIn 또는 FadeOut 다음에 진행할 함수

    // [추가] 코루틴 중복 실행을 막기 위한 변수
    private Coroutine _fadeCoroutine;

   
    // [수정] 매 프레임 무한 호출되던 Update() 함수 전체 삭제

    public void FadeIn(GameObject Panel)
    {
        if (Panel == null) return;
        Panel.SetActive(true);
        Debug.Log("FadeCanvasController_ Fade In 시작");

        // 실행 중인 페이드가 있다면 멈추고 새로 시작
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(CoFadeIn(Panel));
    }

    public void FadeOut(GameObject Panel)
    {
        if (Panel == null) return;
        Panel.SetActive(true);
        Debug.Log("FadeCanvasController_ Fade Out 시작");

        // 실행 중인 페이드가 있다면 멈추고 새로 시작
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(CoFadeOut(Panel));


    }

    IEnumerator CoFadeIn(GameObject Panel)
    {
        float elapsedTime = 0f;
        float fadedTime = 0.5f;

        while (elapsedTime <= fadedTime)
        {
            Panel.GetComponent<CanvasRenderer>().SetAlpha(Mathf.Lerp(1f, 0f, elapsedTime / fadedTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Debug.Log("Fade In 끝");
        Panel.SetActive(false);
        _onCompleteCallback?.Invoke();

        _fadeCoroutine = null; // 초기화
    }

    IEnumerator CoFadeOut(GameObject Panel)
    {
        float elapsedTime = 0f;
        float fadedTime = 0.5f;

        while (elapsedTime <= fadedTime)
        {
            Panel.GetComponent<CanvasRenderer>().SetAlpha(Mathf.Lerp(0f, 1f, elapsedTime / fadedTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
      
        Debug.Log("Fade Out 끝");
        Panel.SetActive(false);
        _onCompleteCallback?.Invoke();

        _fadeCoroutine = null; // 초기화
    }

    public void RegisterCallback(Action callback)
    {
        _onCompleteCallback = callback;
    }
}