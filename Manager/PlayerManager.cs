using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerManager : Singleton<PlayerManager>
{
    public delegate void TriggerEvent(PlayerCharacter _playerCharacter);
    public PhotonView photonView;
    public PlayerCharacter palyerPrefab;
    public GameObject mouseGroundArrowPrefab;
    public MainTowerCharacter mainTowerCharacter;
    public PlayerSpawnPoint[] StartPoint;
    public event TriggerEvent OnMyPlayerCharacterActive;
    public float playerFixTime;

    private PlayerCharacter myplayerCharacter;
    private List<PlayerCharacter> playerCharacters = new List<PlayerCharacter>(4);

    public List<PlayerCharacter> GetPlayerCharacters()
    {
        return playerCharacters;
    }
    public PlayerCharacter GetMyPlayerCharacter()
    {
        return myplayerCharacter;
    }
    public PlayerCharacter GetPlayerCharacter(int _viewID)
    {
        foreach (PlayerCharacter character in playerCharacters)
        {
            if (character.photonView.ViewID == _viewID)
                return character;
        }
        Debug.LogError("There is no player with the following viewID : " + _viewID);
        return null;
    }
    public bool IsPlayerCharacterViewID(int _viewID)
    {
        foreach (PlayerCharacter character in playerCharacters)
        {
            if (character.photonView.ViewID == _viewID)
                return true;
        }
        return false;
    }
    public void AddPlayerCharacter(PlayerCharacter _player)
    {
        playerCharacters.Add(_player);
        if (_player.photonView.IsMine)
        {
            myplayerCharacter = _player;
            MouseGroundArrow mouseGroundArrow = GameObject.Instantiate(mouseGroundArrowPrefab).GetComponent<MouseGroundArrow>();
            mouseGroundArrow.SetPlayer(myplayerCharacter.transform);
            myplayerCharacter.SetToMyPlayerCharacter(mouseGroundArrow);
            if (OnMyPlayerCharacterActive != null)
                OnMyPlayerCharacterActive(myplayerCharacter);
        }
    }
    public void OutPlayerCharacter(PlayerCharacter _player)
    {
        if (playerCharacters.Contains(_player))
            playerCharacters.Remove(_player);
    }
    public void CreatePlayer()
    {
        int spownPointNumber = 0;
        int characterSpriteNumber = 0;

        object spownPointNumberObject;
        if (PhotonCustomProperties.TryGetPlayerCustomProperties("PlayerSpownPointNumber", out spownPointNumberObject))
            spownPointNumber = (int)spownPointNumberObject;
        else
            Debug.LogError("Key called \"PlayerSpownPointNumber\" does not exist in CustomProperties");

        object characterSpriteNumberObject;
        if (PhotonCustomProperties.TryGetPlayerCustomProperties("characterSpriteNumber", out characterSpriteNumberObject))
            characterSpriteNumber = (int)characterSpriteNumberObject;
        else
            Debug.LogError("Key called \"characterSpriteNumber\" does not exist in CustomProperties");

        int viewId = PhotonNetwork.AllocateViewID(PhotonNetwork.LocalPlayer.ActorNumber);
        photonView.RPC("NewPlayer", RpcTarget.AllBuffered, viewId, spownPointNumber, characterSpriteNumber);
    }
    public void StartFixPlayer(PlayerCharacter _palyer, int _spownPointNumber)
    {
        if (!mainTowerCharacter.gameObject.activeSelf)
            return;

        _palyer.transform.position = StartPoint[_spownPointNumber].transform.position;
        _palyer.transform.rotation = StartPoint[_spownPointNumber].transform.rotation;
        StartPoint[_spownPointNumber].FixPlayer(_palyer, playerFixTime);
    }
    public void StopFixPlayer(int _spownPointNumber)
    {
        StartPoint[_spownPointNumber].StopFixPlayer();
    }
    public void StopFixPlayerAll()
    {
        for (int i = 0; i < StartPoint.Length; i++)
            StartPoint[i].StopFixPlayer();
    }
    public void DestroyPlayerAll()
    {
        foreach (PlayerCharacter playerCharacter in playerCharacters)
        {
            playerCharacter.Death(photonView.ViewID);
        }
    }

    private void OnEnable()
    {
        GameSystem.OnCountdownTimerHasExpired += OnCountdownTimerHasExpired;
    }
    private void OnDisable()
    {
        GameSystem.OnCountdownTimerHasExpired -= OnCountdownTimerHasExpired;
    }
    private void OnCountdownTimerHasExpired()
    {
        CreatePlayer();
    }

    [PunRPC]
    private void NewPlayer(int _viewId, int _spownPointNumber, int _characterSpriteNumber)
    {
        palyerPrefab.transform.position = StartPoint[_spownPointNumber].transform.position;
        palyerPrefab.transform.rotation = StartPoint[_spownPointNumber].transform.rotation;
        palyerPrefab.gameObject.SetActive(true);
        PlayerCharacter palyer = GameObject.Instantiate(palyerPrefab);
        palyer.GetComponent<PhotonView>().ViewID = _viewId;
        palyer.GetComponent<PlayerCharacter>().spownPointNumber = _spownPointNumber;
        palyer.SetCharacterVisual(_characterSpriteNumber);
        AddPlayerCharacter(palyer);
    }
  
}
