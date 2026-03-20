using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

public class FadeController : MonoBehaviour // Panel 불투명도 조절해 페이드인 or 페이드아웃
{
    public bool IsFadeIn; // true=FadeIn, false=FadeOut
    public GameObject Panel; // 불투명도를 조절할 Panel 오브젝트
    private Action _onCompleteCallback; // FadeIn 또는 FadeOut 다음에 진행할 함수
    private bool _stageStart = false;
    void Start()
    {
        if (!Panel)
        {
            Debug.LogError("Panel 오브젝트를 찾을 수 없습니다.");
            throw new MissingComponentException();
        }

        if (IsFadeIn) // Fade In Mode -> 바로 코루틴 시작
        {
            Panel.SetActive(true); // Panel 활성화
            StartCoroutine(CoFadeIn());
        }
        else
        {
            Panel.SetActive(false); // Panel 비활성화
        }
    }

    private void Update()
    {
        if(_stageStart == true)
            FadeIn();
        else if (_stageStart == false)
            FadeOut();
    }
    private void OnEnable()
    {
        if (EventManager.Instance == null)
            return;

        EventManager.Instance.AddListener(MEventType.StageStarted, OnGameStart);
        EventManager.Instance.AddListener(MEventType.StageFailed, OnGameFail);
    }


    private void OnDisable()
    {
        if (EventManager.Instance == null)
            return;

        EventManager.Instance.RemoveListener(MEventType.StageStarted, this);
        EventManager.Instance.RemoveListener(MEventType.StageFailed, this);
    }
    public void FadeIn()
    {
        Panel.SetActive(true); // Panel 활성화
        Debug.Log("FadeCanvasController_ Fade In 시작");
        StartCoroutine(CoFadeIn());
        Debug.Log("FadeCanvasController_ Fade In 끝");
    }

    public void FadeOut()
    {
        Panel.SetActive(true); // Panel 활성화
        Debug.Log("FadeCanvasController_ Fade Out 시작");
        StartCoroutine(CoFadeOut());
        Debug.Log("FadeCanvasController_ Fade Out 끝");
    }

    IEnumerator CoFadeIn()
    {
        float elapsedTime = 0f; // 누적 경과 시간
        float fadedTime = 0.5f; // 총 소요 시간

        while (elapsedTime <= fadedTime)
        {
            Panel.GetComponent<CanvasRenderer>().SetAlpha(Mathf.Lerp(1f, 0f, elapsedTime / fadedTime));

            elapsedTime += Time.deltaTime;
            Debug.Log("Fade In 중...");
            yield return null;
        }
        Debug.Log("Fade In 끝");
        Panel.SetActive(false); // Panel을 비활성화
        _onCompleteCallback?.Invoke(); // 이후에 해야 하는 다른 액션이 있는 경우(null이 아님) 진행한다
        yield break;
    }

    IEnumerator CoFadeOut()
    {
        float elapsedTime = 0f; // 누적 경과 시간
        float fadedTime = 0.5f; // 총 소요 시간

        while (elapsedTime <= fadedTime)
        {
            Panel.GetComponent<CanvasRenderer>().SetAlpha(Mathf.Lerp(0f, 1f, elapsedTime / fadedTime));

            elapsedTime += Time.deltaTime;
            Debug.Log("Fade Out 중...");
            yield return null;
        }

        Debug.Log("Fade Out 끝");
        _onCompleteCallback?.Invoke(); // 이후에 해야 하는 다른 액션이 있는 경우(null이 아님) 진행한다
        yield break;
    }

    public void RegisterCallback(Action callback) // 다른 스크립트에서 콜백 액션 등록하기 위해 사용
    {
        _onCompleteCallback = callback;
    }


    private void OnGameFail(MEventType MEventType, Component Sender, EventArgs args)
    {
        //_stageStart = false;
    }

    private void OnGameStart(MEventType MEventType, Component Sender, EventArgs args)
    {
        //_stageStart = true;
    }
}