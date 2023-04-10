using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class NetworkPoolManager<T> : PoolManager<T>, IInRoomCallbacks where T : Component, IManuallyInstantiatedNetworkObject
{
    private PhotonView photonView;

    public override T SpawnObject(string _key, Vector3 _position)
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

            int viewId = PhotonNetwork.AllocateViewID(0);
            NetworkPool<T> networkPool = (NetworkPool<T>)pools[_key];
            poolObject = networkPool.NewNetworkPoolObject(viewId);
            poolObject.transform.position = _position;
            poolObject.gameObject.SetActive(true);
            photonView.RPC("SetNewPoolObject", RpcTarget.Others, _key, viewId, _position);
        }
        else
        {
            poolObject.SendActiveTrue();
        }
        return poolObject;
    }
    public override T SpawnObject(string _key, Vector3 _position, Quaternion _rotation)
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

            int viewId = PhotonNetwork.AllocateViewID(0);
            NetworkPool<T> networkPool = (NetworkPool<T>)pools[_key];
            poolObject = networkPool.NewNetworkPoolObject(viewId);
            poolObject.transform.position = _position;
            poolObject.transform.rotation = _rotation;
            poolObject.gameObject.SetActive(true);
            photonView.RPC("SetNewPoolObject", RpcTarget.Others, _key, viewId, _position, _rotation);
        }
        else
        {
            poolObject.SendActiveTrue();
        }
        return poolObject;
    }
    public override T SpawnObject(string _key, Vector3 _position, Quaternion _rotation, Transform _parent)
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

            int viewId = PhotonNetwork.AllocateViewID(0);
            NetworkPool<T> networkPool = (NetworkPool<T>)pools[_key];
            poolObject = networkPool.NewNetworkPoolObject(viewId);
            poolObject.transform.position = _position;
            poolObject.transform.rotation = _rotation;
            poolObject.transform.SetParent(_parent);
            poolObject.gameObject.SetActive(true);
            photonView.RPC("SetNewPoolObject", RpcTarget.Others, _key, viewId, _position, _rotation, _parent);
        }
        else
        {
            poolObject.SendActiveTrue();
        }
        return poolObject;
    }


    [PunRPC]
    protected void SetCreatBasicNetworkPoolObjects(string _poolKey, int[] _viewIds)
    {
        if (pools == null)
            SettingPools();
        NetworkPool<T> networkPool = (NetworkPool<T>)pools[_poolKey];
        networkPool.CreatBasicNetworkPoolObjects(_viewIds);
    }
    [PunRPC]
    protected void SetNewPoolObject(string _key, int _viewId, Vector3 _position, PhotonMessageInfo _info)
    {
        NetworkPool<T> networkPool = (NetworkPool<T>)pools[_key];
        T poolObject = networkPool.NewNetworkPoolObject(_viewId);
        poolObject.transform.position = _position;
        poolObject.gameObject.SetActive(true);
        poolObject.SetInterpolation(_info.SentServerTime);
    }
    [PunRPC]
    protected void SetNewPoolObject(string _key, int _viewId, Vector3 _position, Quaternion _rotation, PhotonMessageInfo _info)
    {
        NetworkPool<T> networkPool = (NetworkPool<T>)pools[_key];
        T poolObject = networkPool.NewNetworkPoolObject(_viewId);
        poolObject.transform.position = _position;
        poolObject.transform.rotation = _rotation;
        poolObject.gameObject.SetActive(true);
        poolObject.SetInterpolation(_info.SentServerTime);
    }
    [PunRPC]
    protected void SetNewPoolObject(string _key, int _viewId, Vector3 _position, Quaternion _rotation, Transform _parent, PhotonMessageInfo _info)
    {
        NetworkPool<T> networkPool = (NetworkPool<T>)pools[_key];
        T poolObject = networkPool.NewNetworkPoolObject(_viewId);
        poolObject.transform.position = _position;
        poolObject.transform.rotation = _rotation;
        poolObject.transform.SetParent(_parent);
        poolObject.gameObject.SetActive(true);
        poolObject.SetInterpolation(_info.SentServerTime);
    }

    private void Start()
    {
        photonView = GetComponent<PhotonView>();

        if (PhotonNetwork.IsMasterClient)
        {
            SettingPools();
            SendPoolObjectViewIds();
        }
    }

    private void SettingPools()
    {
        pools = new Dictionary<string, Pool<T>>();
        Dictionary<string, PoolObjectData<T>> PoolDatas = poolStorage.GetDataAll();
        foreach (KeyValuePair<string, PoolObjectData<T>> pair in PoolDatas)
        {
            NetworkPool<T> pool = new NetworkPool<T>();
            pool.SettingPool(pair.Value.component, transform, pair.Value.minCount, pair.Value.maxCount);
            pools.Add(pair.Key, pool);
        }
    }
    private void SendPoolObjectViewIds()
    {
        foreach (KeyValuePair<string, Pool<T>> pair in pools)
        {
            int[] viewIds = new int[pair.Value.minCount];
            for (int i = 0; i < viewIds.Length; i++)
            {
                viewIds[i] = PhotonNetwork.AllocateViewID(0);
            }
            photonView.RPC("SetCreatBasicNetworkPoolObjects", RpcTarget.AllViaServer, pair.Key, viewIds);
        }
    }


    void IInRoomCallbacks.OnPlayerEnteredRoom(Player newPlayer) {}

    void IInRoomCallbacks.OnPlayerLeftRoom(Player otherPlayer){}

    void IInRoomCallbacks.OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) {}

    void IInRoomCallbacks.OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps) {}

    void IInRoomCallbacks.OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            foreach (KeyValuePair<string, Pool<T>> pair in pools)
            {
                NetworkPool<T> networkPool = (NetworkPool<T>)pair.Value;
                networkPool.ChangedToMasterClient();
            }
        }
    }
}
