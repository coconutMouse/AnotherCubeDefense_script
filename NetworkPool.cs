using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using PunNetworkEvent;

public class NetworkPool<T> : Pool<T> where T : Component
{
    public T NewNetworkPoolObject(int _viewId)
    {
        T poolObject = NewPoolObject();
        PhotonView photonView = poolObject.GetComponent<PhotonView>();
        if (photonView == null)
        {
            photonView = poolObject.gameObject.AddComponent<PhotonView>();
            photonView.OwnershipTransfer = OwnershipOption.Fixed;
        }
        photonView.ViewID = _viewId;
        return poolObject;
    }
    public void CreatBasicNetworkPoolObjects(int[] _viewIds)
    {
        foreach (int viewId in _viewIds)
        {
            T obj = NewNetworkPoolObject(viewId);
            pool.Enqueue(obj);
        }
    }
    public void ChangedToMasterClient()
    {
        pool.Clear();
        foreach (T obj in allObjects)
        {
            if (!obj.gameObject.activeSelf)
                pool.Enqueue(obj);
        }
    }
    protected override void DespawnObject(Component _object)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            pool.Enqueue((T)_object);
        }
    }
    protected override void DestroyObject(Component _object)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            allObjects.Remove((T)_object);
        }
    }
}

