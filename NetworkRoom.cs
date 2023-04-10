using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public struct PlayerPanelInfo 
{
    public PlayerPanelInfo(Image _characterImage, Text _nameText, GameObject _expulsion, GameObject _hostMark, GameObject _readyMark, GameObject _hostAppointment,
                           GameObject _selectCharacterLeft, GameObject _selectCharacterRight)
    {
        characterImage = _characterImage;
        nameText = _nameText;
        expulsion = _expulsion;
        hostMark = _hostMark;
        readyMark = _readyMark;
        hostAppointment = _hostAppointment;
        selectCharacterLeft = _selectCharacterLeft;
        selectCharacterRight = _selectCharacterRight;
    }
    public Image characterImage;
    public Text nameText;
    public GameObject expulsion;
    public GameObject hostMark;
    public GameObject readyMark;
    public GameObject hostAppointment;
    public GameObject selectCharacterLeft;
    public GameObject selectCharacterRight;
}

public class NetworkRoom : MonoBehaviourPunCallbacks
{
    public Text roomNameText;
    public GameObject roomWin;
    public GameObject destroyTheRoomButtonObject;
    public GameObject changeRoomNameButtonObject;
    public GameObject startButtonObject;
    public GameObject readyButtonObject;
    public GameObject nonTouchScreenObject;
    public GameObject[] playerPanels;
    public List<PlayerPanelInfo> playerPanelInfos;
    public Sprite[] characterSprites;

    private bool bMasterClient = false;

    public void BreakRoom()
    {
        photonView.RPC("Expulsion", RpcTarget.OthersBuffered, "The room is gone.");
        LeaveRoom();
    }
    public void ExpulsionPlayer(int num)
    {
        photonView.RPC("Expulsion", PhotonNetwork.PlayerList[num], "You were forced out of the room.");
    }
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
    public void OpenRoomPanel()
    {
        object roomNameObject;
        if (PhotonCustomProperties.TryGetRoomCustomProperties("RoomName", out roomNameObject))
        {
            roomNameText.text = (string)roomNameObject;
        }
        else
        {
            Debug.LogError("Key called \"RoomName\" does not exist in CustomProperties");
            roomNameText.text = "Missing";
        }
        roomWin.SetActive(true);
        bMasterClient = PhotonNetwork.IsMasterClient;
        destroyTheRoomButtonObject.SetActive(bMasterClient);
        changeRoomNameButtonObject.SetActive(bMasterClient);
        startButtonObject.SetActive(bMasterClient);
        readyButtonObject.SetActive(!bMasterClient);
        RoomRenewal();
    }

    public void ChangeMasterClient(int num)
    {
        if (PhotonCustomProperties.CheckPlayerCustomPropertiesKey("Ready"))
            PhotonCustomProperties.SetPlayerCustomProperties("Ready", false);
        PhotonNetwork.SetMasterClient(PhotonNetwork.PlayerList[num]);
    }

    public void ChangeRoomName(Text reNameText)
    {
        PhotonCustomProperties.SetRoomCustomProperties("RoomName", reNameText.text);
        photonView.RPC("SetRoomName", RpcTarget.AllBuffered, reNameText.text);
    }

    public void RoomRenewal()
    {
        for(int i = 0; i < 4; i++)
        {
            if (i < PhotonNetwork.PlayerList.Length)
            {
                playerPanels[i].SetActive(true);
                Player player = PhotonNetwork.PlayerList[i];
                if (PhotonNetwork.LocalPlayer == player)
                {
                    if (PhotonCustomProperties.CheckPlayerCustomPropertiesKey("PlayerSpownPointNumber"))
                        PhotonCustomProperties.SetPlayerCustomProperties("PlayerSpownPointNumber", i);
                    else
                        PhotonCustomProperties.AddPlayerCustomProperties("PlayerSpownPointNumber", i);

                    int characterSpriteNumber = 0;
                    object characterSpriteNumberObject;
                    if (PhotonCustomProperties.TryGetPlayerCustomProperties("characterSpriteNumber", out characterSpriteNumberObject))
                        characterSpriteNumber = (int)characterSpriteNumberObject;
                    else
                        PhotonCustomProperties.AddPlayerCustomProperties("characterSpriteNumber", 0);

                    bool bReady = false;
                    object readyObject;
                    if (PhotonCustomProperties.TryGetPlayerCustomProperties(player, "Ready", out readyObject))
                        bReady = (bool)readyObject;

                    SetPlayerPanel(player.NickName, i, characterSpriteNumber, bReady);
                }
                else
                {
                    int characterSpriteNumber = 0;
                    object characterSpriteNumberObject;
                    if (PhotonCustomProperties.TryGetPlayerCustomProperties(player, "characterSpriteNumber", out characterSpriteNumberObject))
                        characterSpriteNumber = (int)characterSpriteNumberObject;

                    bool bReady = false;
                    object readyObject;
                    if (PhotonCustomProperties.TryGetPlayerCustomProperties(player, "Ready", out readyObject))
                        bReady = (bool)readyObject;

                    SetPlayerPanelOther(player.NickName, i, characterSpriteNumber, bReady);
                }
            }
            else
            {
                playerPanels[i].SetActive(false);
            }
        }
    }
    public void GameStart()
    {
        if (!PhotonNetwork.CurrentRoom.IsOpen)
            return;

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player == PhotonNetwork.LocalPlayer)
                continue;
            object readyObject;
            if (PhotonCustomProperties.TryGetPlayerCustomProperties(player, "Ready", out readyObject))
            {
                if ((bool)readyObject == false)
                {
                    MessageManager.Instance.Message("All players must be ready to start the game.");
                    return;
                }
            }
            else
            {
                MessageManager.Instance.Message("All players must be ready to start the game.");
                return;
            }
        }

        PhotonCustomProperties.SetRoomCustomProperties("InGame", "In Game");
        PhotonNetwork.CurrentRoom.IsOpen = false;
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("LoadGameScene", RpcTarget.All);
        }
    }
    public void Ready()
    {
        object readyObject;
        if (PhotonCustomProperties.TryGetPlayerCustomProperties("Ready", out readyObject))
        {
            bool bReady = !(bool)readyObject;
            photonView.RPC("SetReadyUi", RpcTarget.All, bReady, PhotonNetwork.LocalPlayer.ActorNumber);
            PhotonCustomProperties.SetPlayerCustomProperties("Ready", bReady);
        }
        else
        {
            photonView.RPC("SetReadyUi", RpcTarget.All, true, PhotonNetwork.LocalPlayer.ActorNumber);
            PhotonCustomProperties.AddPlayerCustomProperties("Ready", true);
        }
    }
    public void ChangeCharacterSpriteNumber(int _valueToAdd)
    {
        object characterSpriteNumberObject;
        if (PhotonCustomProperties.TryGetPlayerCustomProperties("characterSpriteNumber", out characterSpriteNumberObject))
        {
            int number = (int)characterSpriteNumberObject + _valueToAdd;
            if (number >= characterSprites.Length)
                number = 0;
            else if (number < 0)
                number = characterSprites.Length - 1;

            PhotonCustomProperties.SetPlayerCustomProperties("characterSpriteNumber", number);
            photonView.RPC("ChangeCharacterImage", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
        }
        else
            Debug.LogError("Key called \"characterSpriteNumber\" does not exist in CustomProperties");
    }
    [PunRPC]
    public void Expulsion(string message)
    {
        LeaveRoom();
        MessageManager.Instance.Message(message);
    }
    [PunRPC]
    public void SetRoomName(string name)
    {
        roomNameText.text = name;
    }
    [PunRPC]
    public void LoadGameScene()
    {
        nonTouchScreenObject.SetActive(true);

        if (PhotonCustomProperties.CheckPlayerCustomPropertiesKey("Ready"))
            PhotonCustomProperties.SetPlayerCustomProperties("Ready", false);

        PhotonNetwork.LoadLevel("GameScene");
    }
    [PunRPC]
    public void SetReadyUi(bool _ready, int _changePlayerActorNumber)
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].ActorNumber == _changePlayerActorNumber)
            {
                playerPanelInfos[i].readyMark.SetActive(_ready);
                break;
            }
        }

    }
    public override void OnJoinedRoom()
    {
        if (PhotonCustomProperties.CheckPlayerCustomPropertiesKey("Ready"))
            PhotonCustomProperties.SetPlayerCustomProperties("Ready", false);
        OpenRoomPanel();
    }
    public override void OnLeftRoom()
    {
        if (!this.gameObject.scene.isLoaded)
            return;
        roomWin.SetActive(false);
    }
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        RoomRenewal();
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        RoomRenewal();
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RoomRenewal();
    }

    private void Awake()
    {
        playerPanelInfos = new List<PlayerPanelInfo>();

        for (int i = 0; i < playerPanels.Length; i++)
        {
            Image characterImage = playerPanels[i].transform.GetChild(0).GetComponent<Image>();
            Text nameText = playerPanels[i].transform.GetChild(1).GetComponent<Text>();
            GameObject expulsion = playerPanels[i].transform.GetChild(2).gameObject;
            GameObject hostMark = playerPanels[i].transform.GetChild(3).gameObject;
            GameObject readyMark = playerPanels[i].transform.GetChild(4).gameObject;
            GameObject hostAppointment = playerPanels[i].transform.GetChild(5).gameObject;
            GameObject selectCharacterLeft = playerPanels[i].transform.GetChild(6).gameObject;
            GameObject selectCharacterRight = playerPanels[i].transform.GetChild(7).gameObject;

            PlayerPanelInfo playerPanelInfo = new PlayerPanelInfo(characterImage, nameText, expulsion, hostMark, readyMark, hostAppointment, selectCharacterLeft, selectCharacterRight);
            playerPanelInfos.Add(playerPanelInfo);
        }
    }
    private void Start()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonCustomProperties.SetRoomCustomProperties("InGame", " ");
            PhotonNetwork.CurrentRoom.IsOpen = true;

            OpenRoomPanel();
        }
    }
    private void Update()
    {
        if (bMasterClient != PhotonNetwork.IsMasterClient)
        {
            bMasterClient = PhotonNetwork.IsMasterClient;
            destroyTheRoomButtonObject.SetActive(bMasterClient);
            changeRoomNameButtonObject.SetActive(bMasterClient);
            startButtonObject.SetActive(bMasterClient);
            readyButtonObject.SetActive(!bMasterClient);
        }
    }
    private void SetPlayerPanel(string _name, int _playerPanelNumber, int _spriteNumber, bool _ready)
    {
        playerPanelInfos[_playerPanelNumber].characterImage.sprite = characterSprites[_spriteNumber];
        playerPanelInfos[_playerPanelNumber].nameText.text = _name;
        playerPanelInfos[_playerPanelNumber].expulsion.SetActive(false);
        playerPanelInfos[_playerPanelNumber].hostAppointment.SetActive(false);
        playerPanelInfos[_playerPanelNumber].selectCharacterLeft.SetActive(true);
        playerPanelInfos[_playerPanelNumber].selectCharacterRight.SetActive(true);

        if (PhotonNetwork.IsMasterClient)
        {
            playerPanelInfos[_playerPanelNumber].hostMark.SetActive(true);
            playerPanelInfos[_playerPanelNumber].readyMark.SetActive(false);
        }
        else
        {
            playerPanelInfos[_playerPanelNumber].hostMark.SetActive(false);
            playerPanelInfos[_playerPanelNumber].readyMark.SetActive(_ready);
        }
    }
    private void SetPlayerPanelOther(string _name, int _playerPanelNumber, int _spriteNumber, bool _ready)
    {
        playerPanelInfos[_playerPanelNumber].characterImage.sprite = characterSprites[_spriteNumber];
        playerPanelInfos[_playerPanelNumber].nameText.text = _name;
        playerPanelInfos[_playerPanelNumber].selectCharacterLeft.SetActive(false);
        playerPanelInfos[_playerPanelNumber].selectCharacterRight.SetActive(false);

        if (PhotonNetwork.IsMasterClient)
        {
            playerPanelInfos[_playerPanelNumber].expulsion.SetActive(true);
            playerPanelInfos[_playerPanelNumber].hostAppointment.SetActive(true);
        }
        else
        {
            playerPanelInfos[_playerPanelNumber].expulsion.SetActive(false);
            playerPanelInfos[_playerPanelNumber].hostAppointment.SetActive(false);
        }

        if (PhotonNetwork.PlayerList[_playerPanelNumber].IsMasterClient)
        {
            playerPanelInfos[_playerPanelNumber].hostMark.SetActive(true);
            playerPanelInfos[_playerPanelNumber].readyMark.SetActive(false);
        }
        else
        {
            playerPanelInfos[_playerPanelNumber].hostMark.SetActive(false);
            playerPanelInfos[_playerPanelNumber].readyMark.SetActive(_ready);
        }
    }

    private void SetCharacterImage(int _playerSpownPointNumbe, int _characterSpriteNumber)
    {
        playerPanelInfos[_playerSpownPointNumbe].characterImage.sprite = characterSprites[_characterSpriteNumber];
    }
    [PunRPC]
    private void ChangeCharacterImage(int _changePlayerActorNumber)
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].ActorNumber == _changePlayerActorNumber)
            {
                object characterSpriteNumberObject;
                if (PhotonCustomProperties.TryGetPlayerCustomProperties(PhotonNetwork.PlayerList[i], "characterSpriteNumber", out characterSpriteNumberObject))
                    SetCharacterImage(i, (int)characterSpriteNumberObject);
                else
                    Debug.LogError("Key called \"characterSpriteNumber\" does not exist in CustomProperties");
                break;
            }
        }
    }
}
