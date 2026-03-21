using System;
using UnityEngine;

[Serializable]
public class TransformEventArgs : EventArgs
{
    public Transform transform;
    public object[] value;

    public TransformEventArgs(Transform transform, params object[] value)
    {
        this.transform = transform;
        this.value = value;
    }

    
}


[Serializable]
public class ComboHitEventArgs : EventArgs
{
    public int ComboCount;
    public float ComboRatio;

    public ComboHitEventArgs(int comboCount, float comboRatio)
    {
        ComboCount = comboCount;
        ComboRatio = comboRatio;
    }
}

[Serializable]
public class TimerEventArgs : EventArgs
{
    public float remaining;
    public float total;

    public TimerEventArgs(float remaining, float total)
    {
        this.remaining = remaining;
        this.total = total;
    }
}

[Serializable]
public class GameStateChangedEventArgs : EventArgs
{
    public GameState previous;
    public GameState current;

    public GameStateChangedEventArgs(GameState previous, GameState current)
    {
        this.previous = previous;
        this.current = current;
    }
}