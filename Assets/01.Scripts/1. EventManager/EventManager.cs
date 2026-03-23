
using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : Singleton<EventManager>
{
    protected EventManager() { }

    // 이벤트 대리자
    public delegate void OnEvent(MEventType MEventType, Component Sender, EventArgs args = null);

    private Dictionary<MEventType, List<OnEvent>> Listeners = new Dictionary<MEventType, List<OnEvent>>();

    // 리스너 등록
    public void AddListener(MEventType MEventType, OnEvent Listener)
    {
        if (Listener == null)
            return;

        if (!Listeners.TryGetValue(MEventType, out List<OnEvent> listenList))
        {
            listenList = new List<OnEvent>();
            Listeners.Add(MEventType, listenList);
        }

        listenList.Add(Listener);
    }

    // 이벤트 호출
    public void PostNotification(MEventType MEventType, Component Sender, EventArgs args = null)
    {   
        if (!Listeners.TryGetValue(MEventType, out List<OnEvent> listenList))
            return;

        if (listenList == null || listenList.Count == 0)
            return;

        OnEvent[] invokeList = listenList.ToArray();

        for (int i = 0; i < invokeList.Length; i++)
        {
            OnEvent listener = invokeList[i];

            if (IsInvalidListener(listener))
                continue;

            try
            {
                listener(MEventType, Sender, args);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EventManager] Exception while invoking '{MEventType}'.\n{ex}");
            }
        }
    }

    // 특정 타겟이 등록한 리스너 제거
    public void RemoveListener(MEventType MEventType, object target)
    {
        if (!Listeners.TryGetValue(MEventType, out List<OnEvent> listenList))
            return;

        for (int i = listenList.Count - 1; i >= 0; i--)
        {
            OnEvent listener = listenList[i];

            if (listener == null)
            {
                listenList.RemoveAt(i);
                continue;
            }

            if (target == listener.Target)
                listenList.RemoveAt(i);
        }

        if (listenList.Count == 0)
            Listeners.Remove(MEventType);
    }

    // 이벤트 통째로 제거
    public void RemoveEvent(MEventType MEventType)
    {
        Listeners.Remove(MEventType);
    }

    // 죽은 리스너/불필요한 이벤트 정리
    public void RemoveRedundancies()
    {
        Dictionary<MEventType, List<OnEvent>> newListeners = new Dictionary<MEventType, List<OnEvent>>();

        foreach (KeyValuePair<MEventType, List<OnEvent>> item in Listeners)
        {
            List<OnEvent> validListeners = new List<OnEvent>();

            for (int i = 0; i < item.Value.Count; i++)
            {
                OnEvent listener = item.Value[i];

                if (IsInvalidListener(listener))
                    continue;

                validListeners.Add(listener);
            }

            if (validListeners.Count > 0)
                newListeners.Add(item.Key, validListeners);
        }

        Listeners = newListeners;
    }

    public override void Init()
    {
        Debug.Log("EventManager Init Complete!");
    }

    // Unity에서 파괴된 오브젝트/비정상 리스너 판별
    private bool IsInvalidListener(OnEvent listener)
    {
        if (listener == null)
            return true;

        // static 메서드는 Target이 null이어도 정상
        if (listener.Method != null && listener.Method.IsStatic)
            return false;

        object target = listener.Target;

        if (target == null)
            return true;

        if (target is UnityEngine.Object unityObject && unityObject == null)
            return true;

        return false;
    }
}
