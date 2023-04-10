using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameSystem : MonoBehaviourPunCallbacks
{
    public delegate void TriggerEvent();
    public delegate void TriggerEvent<T>(T _value1, T _value2);
    public static event TriggerEvent OnCountdownTimerHasExpired;
    public static event TriggerEvent OnGameOver;
    public static event TriggerEvent<int> OnKillMonsterScoreChange;
    public static bool bWaitLoadingPlayers;
    public Text countdownTimeBeforeStartText;

    private static readonly float countdownTimeBeforeStart = 5.9f;
    private bool bRunningCountdownTimeBeforeStart;
    private static int startTime;
    private static int killMonsterScore;

    public static int KillMonsterScore
    {
        get => killMonsterScore;
        set
        {
            int extraPoints = value - killMonsterScore;
            killMonsterScore = value;
            if (OnKillMonsterScoreChange != null)
                OnKillMonsterScoreChange(killMonsterScore, extraPoints);
        }
    }
    public void GoBackToRoom()
    {
        PhotonCustomProperties.SetRoomCustomProperties("StartTime", -1);

        if (PhotonCustomProperties.CheckPlayerCustomPropertiesKey("LoadingComplete"))
            PhotonCustomProperties.SetPlayerCustomProperties("LoadingComplete", false);

        LoadScene("RoomScene");
    }
    public void GoBackToLobby()
    {
        if (PhotonCustomProperties.CheckPlayerCustomPropertiesKey("LoadingComplete"))
            PhotonCustomProperties.SetPlayerCustomProperties("LoadingComplete", false);

        PhotonNetwork.LeaveRoom();
        LoadScene("RoomScene");
    }

    public void GameOver()
    {
        OnGameOver();
    }
    [PunRPC]
    public void LoadScene(string sceneName)
    {
        PhotonNetwork.LoadLevel(sceneName);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (CheckPlayersLoadingCompleteAll())
        {
            PlayerLoadingCompleteAll();
        }
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (CheckPlayersLoadingCompleteAll())
        {
            PlayerLoadingCompleteAll();
        }
    }
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        StartTimerInitialize();
    }
    public static float GetGamePlayTime()
    {
        return ((PhotonNetwork.ServerTimestamp - startTime) / 1000.0f) - countdownTimeBeforeStart;
    }
    public bool TryGetStartTime(out int startTimestamp)
    {
        startTimestamp = PhotonNetwork.ServerTimestamp;

        object startTimeFromPropsObject;
        if (PhotonCustomProperties.TryGetRoomCustomProperties("StartTime", out startTimeFromPropsObject))
        {
            startTimestamp = (int)startTimeFromPropsObject;

            if (startTimestamp == -1)
                return false;
            else
                return true;
        }
        return false;
    }
    public void SetStartTime()
    {
        int time = 0;
        bool wasSet = TryGetStartTime(out time);

        if (!wasSet && time != -1)
        {
            PhotonCustomProperties.AddRoomCustomProperties("StartTime", time);
        }
        else if(time == -1)
        {
            PhotonCustomProperties.SetRoomCustomProperties("StartTime", PhotonNetwork.ServerTimestamp);
        }
    }

    private void Start()
    {
        startTime = 0;
        bWaitLoadingPlayers = true;
        LoadingComplete();
        if (CheckPlayersLoadingCompleteAll())
        {
            PlayerLoadingCompleteAll();
        }
    }
    private void Update()
    {
        TimerUpdate();
    }

    private void LoadingComplete()
    {
        if (PhotonCustomProperties.CheckPlayerCustomPropertiesKey("LoadingComplete"))
            PhotonCustomProperties.SetPlayerCustomProperties("LoadingComplete", true);
        else
            PhotonCustomProperties.AddPlayerCustomProperties("LoadingComplete", true);
    }
    private bool CheckPlayersLoadingCompleteAll()
    {
        if (!bWaitLoadingPlayers)
            return false;

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            object loadingCompleteObject;
            if (PhotonCustomProperties.TryGetPlayerCustomProperties(player, "LoadingComplete", out loadingCompleteObject))
            {
                if (!(bool)loadingCompleteObject)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        return true;
    }
    private void PlayerLoadingCompleteAll()
    {
        bWaitLoadingPlayers = false;
        SetStartTime();
    }
    private float GetCountdowntimeBeforeStart()
    {
        int time = PhotonNetwork.ServerTimestamp - startTime;
        return countdownTimeBeforeStart - time / 1000f;
    }

    private void StartTimerInitialize()
    {
        if (startTime != 0)
            return;
        int propStartTime;
        if (TryGetStartTime(out propStartTime))
        {
            startTime = propStartTime;
            bRunningCountdownTimeBeforeStart = GetCountdowntimeBeforeStart() > 0;

            if (!bRunningCountdownTimeBeforeStart)
                DisableCountdownTimeBeforeStart();
        }
    }
    private void TimerUpdate()
    {
        if (!bRunningCountdownTimeBeforeStart)
            return;

        float countdown = GetCountdowntimeBeforeStart();
        countdownTimeBeforeStartText.text = string.Format("Game starts in {0} seconds", countdown.ToString("F0"));

        if (countdown > 0.0f) 
            return;

        DisableCountdownTimeBeforeStart();
    }
    private void DisableCountdownTimeBeforeStart()
    {
        bRunningCountdownTimeBeforeStart = false;

        countdownTimeBeforeStartText.text = string.Empty;

        if (OnCountdownTimerHasExpired != null) 
            OnCountdownTimerHasExpired();
    }
}
