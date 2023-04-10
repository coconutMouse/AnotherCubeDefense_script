using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PhotonCustomProperties
{
    public static void SetRoomCustomProperties(string _key, object _data)
    {
        Hashtable customProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        customProperties[_key] = _data;
        PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
    }
    public static void AddRoomCustomProperties(string _key, object _data)
    {
        Hashtable customProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        customProperties.Add(_key, _data);
        PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
    }
    public static bool TryGetRoomCustomProperties(string _key, out object _data)
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(_key, out _data))
        {
            return true;
        }
        return false;
    }
    public static bool CheckRoomCustomPropertiesKey(string _key)
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(_key))
        {
            return true;
        }
        return false;
    }

    public static void SetPlayerCustomProperties(string _key, object _data)
    {
        Hashtable customProperties = PhotonNetwork.LocalPlayer.CustomProperties;
        customProperties[_key] = _data;
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);
    }
    public static void AddPlayerCustomProperties(string _key, object _data)
    {
        Hashtable customProperties = PhotonNetwork.LocalPlayer.CustomProperties;
        customProperties.Add(_key, _data);
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);
    }
    public static bool TryGetPlayerCustomProperties(string _key, out object _data)
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(_key, out _data))
        {
            return true;
        }
        return false;
    }
    public static bool CheckPlayerCustomPropertiesKey(string _key)
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(_key))
        {
            return true;
        }
        return false;
    }

    public static void SetPlayerCustomProperties(Player _player, string _key, object _data)
    {
        Hashtable customProperties = _player.CustomProperties;
        customProperties[_key] = _data;
        _player.SetCustomProperties(customProperties);
    }
    public static void AddPlayerCustomProperties(Player _player, string _key, object _data)
    {
        Hashtable customProperties = _player.CustomProperties;
        customProperties.Add(_key, _data);
        _player.SetCustomProperties(customProperties);
    }
    public static bool TryGetPlayerCustomProperties(Player _player, string _key, out object _data)
    {
        if (_player.CustomProperties.TryGetValue(_key, out _data))
        {
            return true;
        }
        return false;
    }
    public static bool CheckPlayerCustomPropertiesKey(Player _player, string _key)
    {
        if (_player.CustomProperties.ContainsKey(_key))
        {
            return true;
        }
        return false;
    }
}
