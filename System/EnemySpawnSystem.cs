using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[System.Serializable]
public struct SpawnerLineData
{
    public Transform startPoint;
    public Transform endPoint;
    public Vector3 direction;
}

public class EnemySpawnSystem : MonoBehaviour
{
    public delegate void TriggerEvent();
    public static event TriggerEvent OnDangerSign;
    public SpawnerLineData[] spawnerLineDatas;
    public EnemySpawnSystemStorage[] enemySpawnSystemStorageByDifficulty;

    private List<EnemySpawnSystemData> enemySpawnSystemDatas;
    private EnemySpawnSystemData enemySpawnSystemDataToApply;
    private bool bSpawning;
    private int currentDifficultyNumber;
    private int enemySpawnPointNumber;
    private float[] spawningUpdateTimes;
    private PhotonView photonView;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        if (PhotonNetwork.CurrentRoom.PlayerCount > enemySpawnSystemStorageByDifficulty.Length)
            Debug.LogError("The number of players is greater than the number of Storage for each difficulty level specified. Please confirm");
        else
        {
            enemySpawnSystemDatas = enemySpawnSystemStorageByDifficulty[PhotonNetwork.CurrentRoom.PlayerCount - 1].GetValues();
            currentDifficultyNumber = 0;
            enemySpawnSystemDataToApply = enemySpawnSystemDatas[currentDifficultyNumber];
            int count = enemySpawnSystemDataToApply.enemySpawnLimitData.Length;
            spawningUpdateTimes = new float[count];
        }
    }
    private void OnEnable()
    {
        GameSystem.OnCountdownTimerHasExpired += OnCountdownTimerHasExpired;
        GameSystem.OnGameOver += OnGameOver;
    }
    private void OnDisable()
    {
        GameSystem.OnCountdownTimerHasExpired -= OnCountdownTimerHasExpired; 
        GameSystem.OnGameOver -= OnGameOver;
    }
    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (!bSpawning)
            return;

        CheckGameDifficulty();
        SpawnEnemys();
    }
    private void CheckGameDifficulty()
    {
        if (enemySpawnSystemDatas == null)
            return;

        int difficultyNumber = currentDifficultyNumber + 1;
        if (difficultyNumber >= enemySpawnSystemDatas.Count)
            return;

        float time = GameSystem.GetGamePlayTime();
        int minute = (int)time / 60;
        int second = (int)time % 60;
        int difficultyStartMinute = enemySpawnSystemDatas[difficultyNumber].difficultyStartMinute;
        int difficultyStartSecond = enemySpawnSystemDatas[difficultyNumber].difficultyStartSecond;

        if (difficultyStartMinute <= minute && difficultyStartSecond <= second)
        {
            if (enemySpawnSystemDataToApply.enemySpawnPointCode == EnemySpawnPointCode.OneRandomPoint)
                enemySpawnPointNumber = Random.Range(0, spawnerLineDatas.Length);
            photonView.RPC("ChangeGameDifficulty", RpcTarget.All, difficultyNumber, enemySpawnPointNumber);
        }
    }
    [PunRPC]
    private void ChangeGameDifficulty(int _difficultyNumber, int _enemySpawnPointNumber)
    {
        currentDifficultyNumber = _difficultyNumber;
        enemySpawnSystemDataToApply = enemySpawnSystemDatas[_difficultyNumber];
        int count = enemySpawnSystemDataToApply.enemySpawnLimitData.Length;
        spawningUpdateTimes = new float[count];

        enemySpawnPointNumber = _enemySpawnPointNumber;

        if (enemySpawnSystemDataToApply.bDangerSign)
            OnDangerSign();
    }
    private void SpawnEnemys()
    {
        if (spawningUpdateTimes == null)
        {
            Debug.LogError("spawningTimers is null. Please check if enemies in EnemySpawnSystemStorage are properly registered.");
            return;
        }

        for (int i = 0; i < spawningUpdateTimes.Length; i++)
        {
            spawningUpdateTimes[i] += Time.deltaTime;
            EnemySpawnLimitData enemySpawnLimitData = enemySpawnSystemDataToApply.enemySpawnLimitData[i];
            if (spawningUpdateTimes[i] >= enemySpawnLimitData.spawningTime)
            {
                spawningUpdateTimes[i] = 0;
                for (int j = 0; j < enemySpawnLimitData.numberOfSpawn; j++)
                {
                    if (enemySpawnSystemDataToApply.enemySpawnPointCode == EnemySpawnPointCode.RandomPoint)
                        enemySpawnPointNumber = Random.Range(0, spawnerLineDatas.Length);

                    SpawnerLineData spawnerLineData = spawnerLineDatas[enemySpawnPointNumber];
                    Vector3 position = Vector3.zero;
                    Vector3 start = spawnerLineData.startPoint.position;
                    Vector3 end = spawnerLineData.endPoint.position;
                    position.x = Random.Range(start.x, end.x);
                    position.z = Random.Range(start.z, end.z);
                    Quaternion rotation = Quaternion.LookRotation(spawnerLineData.direction);
                    EnemyNetworkPoolManager.Instance.SpawnObject(enemySpawnLimitData.enemyKey, position, rotation);
                }
            }
        }
    }
    private void SpawnStart()
    {
        bSpawning = true;
    }
    private void SpawnEnd()
    {
        bSpawning = false;
    }
    private void OnCountdownTimerHasExpired()
    {
        SpawnStart();
    }
    private void OnGameOver()
    {
        SpawnEnd();
    }
}
