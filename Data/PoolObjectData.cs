using UnityEngine;
using System;

[Serializable]
public class PoolObjectData<T> where T : Component
{
    public int minCount;
    public int maxCount;
    public T component;
}




