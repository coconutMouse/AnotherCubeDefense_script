using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using Hashtable = ExitGames.Client.Photon.Hashtable;
public class NetworkLobby : MonoBehaviourPunCallbacks
{
    public GameObject makeRoomWin;
    public InputField playerNameText;
    public Button[] roomListButtons;
    public Button roomListBackButton;
    public Button roomListNextButton;

    private string playerNameDataPath;
    private string saveDataPath;
    private List<RoomInfo> roomList = new List<RoomInfo>();
    private int currentPage = 1;
    private int multiple;
    private Text createRoomNameText;

    public void PlayerNameValueChanged()
    {
        if(playerNameText.text == "")
        {
            playerNameText.SetTextWithoutNotify("NoName");
        }
        if(!Directory.Exists(saveDataPath))
        {
            Directory.CreateDirectory(saveDataPath);
        }
        SaveManager.Save<string>(playerNameDataPath, playerNameText.text);
    }

    public void JoinRoomByNameButtonClick(Text name)
    {
        string roomId = "";
        foreach (var room in roomList) 
        {
            if ((string)room.CustomProperties["RoomName"] == name.text) 
            {
                roomId = room.Name;
            }
        }
        if(roomId == "")
        {
            MessageManager.Instance.Message("There is no room named \"" + name.text + "\". Please check again.");
            return;
        }
        PhotonNetwork.JoinRoom(roomId);
        MyListRenewal();
    }

    public void RoomListButtonClick(int num)
    {
        PhotonNetwork.LocalPlayer.NickName = playerNameText.text;
        PhotonNetwork.JoinRoom(roomList[multiple + num].Name);
        MyListRenewal();
    }
    public void RoomListBackButtonClick()
    {
        --currentPage;
        MyListRenewal();
    }
    public void RoomListNextButtonClick()
    {
        ++currentPage;
        MyListRenewal();
    }

    public void CreateRoom(Text name)
    {
        if(name.text == "")
        {
            MessageManager.Instance.Message("Please enter the room name.");
            return;
        }
        PhotonNetwork.LocalPlayer.NickName = playerNameText.text;
        createRoomNameText = name;
        RoomOptions roomOptions = new RoomOptions();
        string roomId = "ROOMID" + MakeRandomAlphanumericStrings();
        roomOptions.MaxPlayers = 4;
        roomOptions.CustomRoomProperties = new Hashtable() { { "RoomName", createRoomNameText.text }, { "InGame", " " } };
        roomOptions.CustomRoomPropertiesForLobby = new string[] { "RoomName", "InGame" };
        PhotonNetwork.CreateRoom(roomId, roomOptions, null);
        makeRoomWin.SetActive(false);
    }
    public void GameQuit()
    {
        Application.Quit();
    }
    public string MakeRandomAlphanumericStrings()
    {
        string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        char[] Charsarr = new char[12];

        for (int i = 0; i < Charsarr.Length; i++)
        {
            Charsarr[i] = characters[Random.Range(0, characters.Length)];
        }

        string resultString = new string(Charsarr);
        return resultString;
    }
    public override void OnConnectedToMaster() => PhotonNetwork.JoinLobby();
    public override void OnJoinedLobby()
    {
        PhotonNetwork.LocalPlayer.NickName = playerNameText.text;
        roomList.Clear();
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomInfos)
    {
        int roomCount = roomInfos.Count;
        for (int i = 0; i < roomCount; i++)
        {
            if (!roomInfos[i].RemovedFromList)
            {
                if (!roomList.Contains(roomInfos[i])) roomList.Add(roomInfos[i]);
                else roomList[roomList.IndexOf(roomInfos[i])] = roomInfos[i];
            }
            else if (roomList.IndexOf(roomInfos[i]) != -1) roomList.RemoveAt(roomList.IndexOf(roomInfos[i]));
        }
        MyListRenewal();
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        if(returnCode == 32766) //ErrorCode(32766) : 방 id가 중복일 경우
        {
            CreateRoom(createRoomNameText); 
            return;
        }
        MessageManager.Instance.Message(message);
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);

    }
    private void Awake()
    {
        saveDataPath = Path.Combine(Application.persistentDataPath, "SaveData");
        playerNameDataPath = Path.Combine(saveDataPath, "SaveFile" + ".edwz");

        if (File.Exists(playerNameDataPath))
        {
            playerNameText.SetTextWithoutNotify(SaveManager.Load<string>(playerNameDataPath));
        }
        else
        {
            playerNameText.SetTextWithoutNotify("NoName");
        }
    }
    private void MyListRenewal()
    {
        int maxPage = (roomList.Count % roomListButtons.Length == 0) ? roomList.Count / roomListButtons.Length : roomList.Count / roomListButtons.Length + 1;

        roomListBackButton.interactable = (currentPage <= 1) ? false : true;
        roomListNextButton.interactable = (currentPage >= maxPage) ? false : true;

        multiple = (currentPage - 1) * roomListButtons.Length;
        for (int i = 0; i < roomListButtons.Length; i++)
        {
            roomListButtons[i].interactable = (multiple + i < roomList.Count) ? true : false;
            roomListButtons[i].transform.GetChild(0).GetComponent<Text>().text = (multiple + i < roomList.Count) ? (string)(roomList[multiple + i].CustomProperties["RoomName"]) : "";
            roomListButtons[i].transform.GetChild(1).GetComponent<Text>().text = (multiple + i < roomList.Count) ? roomList[multiple + i].PlayerCount + "/" + roomList[multiple + i].MaxPlayers : "";
            roomListButtons[i].transform.GetChild(2).GetComponent<Text>().text = (multiple + i < roomList.Count) ? (string)(roomList[multiple + i].CustomProperties["InGame"]) : "";
        }
    }
}
