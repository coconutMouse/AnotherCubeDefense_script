using System.Collections.Generic;
using UnityEngine;

public class PoolManager<T> : Singleton<PoolManager<T>> where T : Component
{
    public Storage<PoolObjectData<T>> poolStorage;

    protected Dictionary<string, Pool<T>> pools = null;

    public virtual T SpawnObject(string _key, Vector3 _position)
    {
        if (!pools.ContainsKey(_key))
        {
            Debug.LogError(poolStorage.name + " does not have a\"" + _key + "\" key.");
            return null;
        }

        T poolObject = pools[_key].SpawnObject(_position);
        if (poolObject == null)
        {
            if (pools[_key].IsMaxCount())
                return null;

            poolObject = pools[_key].NewPoolObject();
            poolObject.transform.position = _position;
            poolObject.gameObject.SetActive(true);
        }
        return poolObject;
    }
    public virtual T SpawnObject(string _key, Vector3 _position, Quaternion _rotation)
    {
        if (!pools.ContainsKey(_key))
        {
            Debug.LogError(poolStorage.name + " does not have a\"" + _key + "\" key.");
            return null;
        }

        T poolObject = pools[_key].SpawnObject(_position, _rotation);
        if (poolObject == null)
        {
            if (pools[_key].IsMaxCount())
                return null;
            poolObject = pools[_key].NewPoolObject();
            poolObject.transform.position = _position;
            poolObject.transform.rotation = _rotation;
            poolObject.gameObject.SetActive(true);
        }
        return poolObject;
    }
    public virtual T SpawnObject(string _key, Vector3 _position, Quaternion _rotation, Transform _parent)
    {
        if (!pools.ContainsKey(_key))
        {
            Debug.LogError(poolStorage.name + " does not have a\"" + _key + "\" key.");
            return null;
        }

        T poolObject = pools[_key].SpawnObject(_position, _rotation, _parent);
        if (poolObject == null)
        {
            if (pools[_key].IsMaxCount())
                return null;

            poolObject = pools[_key].NewPoolObject();
            poolObject.transform.position = _position;
            poolObject.transform.rotation = _rotation;
            poolObject.transform.SetParent(_parent);
            poolObject.gameObject.SetActive(true);
        }
        return poolObject;
    }
    private void Start()
    {
        SettingPools();
    }
    private void SettingPools()
    {
        pools = new Dictionary<string, Pool<T>>();
        Dictionary<string, PoolObjectData<T>> PoolDatas = poolStorage.GetDataAll();
        foreach (KeyValuePair<string, PoolObjectData<T>> pair in PoolDatas)
        {
            Pool<T> pool = new Pool<T>();
            pool.SettingPool(pair.Value.component, transform, pair.Value.minCount, pair.Value.maxCount);
            pool.CreatBasicPoolObjects();
            pools.Add(pair.Key, pool);
        }
    }

}
