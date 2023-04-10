using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializeDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField]
    List<TKey> keys = new List<TKey>();
    [SerializeField]
    List<TValue> values = new List<TValue>();

    public List<TValue> GetValues()
    {
        return values;
    }
    public List<TKey> GetKeys()
    {
        return keys;
    }
    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();

        foreach (KeyValuePair<TKey, TValue> pair in this)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }
    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        this.Clear();

        for (int i = 0; i < keys.Count; i++)
            this.Add(keys[i], values[i]);
    }
}
