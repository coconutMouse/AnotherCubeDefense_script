using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pool<T> where T : Component 
{
    public int maxCount;
    public int minCount;

    protected T poolPrefab;
    protected Transform parent;
    protected List<T> allObjects = new List<T>();
    protected Queue<T> pool = new Queue<T>();

    public T SpawnObject(Vector3 _position)
    {
        if (pool.Count != 0)
        {
            T poolObject = pool.Dequeue();
            if (poolObject == null)
                return SpawnObject(_position);

            poolObject.transform.position = _position;
            poolObject.gameObject.SetActive(true);
            return poolObject;
        }
        else
            return null;
    }
    public T SpawnObject(Vector3 _position, Quaternion _rotation)
    {
        if (pool.Count != 0)
        {
            T poolObject = pool.Dequeue();
            if (poolObject == null)
                return SpawnObject(_position, _rotation);

            poolObject.transform.position = _position;
            poolObject.transform.rotation = _rotation;
            poolObject.gameObject.SetActive(true);
            return poolObject;
        }
        else
            return null;
    }
    public T SpawnObject(Vector3 _position, Quaternion _rotation, Transform _parent)
    {
        if (pool.Count != 0)
        {
            T poolObject = pool.Dequeue();
            if (poolObject == null)
                return SpawnObject(_position, _rotation);

            poolObject.transform.position = _position;
            poolObject.transform.rotation = _rotation;
            poolObject.transform.SetParent(_parent);
            poolObject.gameObject.SetActive(true);
            return poolObject;
        }
        else
            return null;
    }
    public void SettingPool(T _object, Transform _parent, int _minCount, int _maxCount)
    {
        poolPrefab = _object;
        parent = _parent;
        maxCount = _maxCount;
        minCount = _minCount;
    }
    public T NewPoolObject()
    {
        poolPrefab.gameObject.SetActive(false);
        T poolObject = Object.Instantiate(poolPrefab);
        poolObject.transform.SetParent(parent);
        poolObject.gameObject.AddComponent<PoolObject>().SettingTriggerEvent((Component)poolObject, DespawnObject, DestroyObject);
        allObjects.Add(poolObject);
        return poolObject;
    }
    public void CreatBasicPoolObjects()
    {
        for (int i = 0; i < minCount; i++)
            DespawnObject(NewPoolObject());
    }
    public List<T> GetAllObjects()
    {
        return allObjects;
    }
    public bool IsMaxCount()
    {
        if (maxCount <= allObjects.Count)
            return true;
        return false;
    }


    protected virtual void DespawnObject(Component _object)
    {
        pool.Enqueue((T)_object);
    }
    protected virtual void DestroyObject(Component _object)
    {
        allObjects.Remove((T)_object);
    }


}