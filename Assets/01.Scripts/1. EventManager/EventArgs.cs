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