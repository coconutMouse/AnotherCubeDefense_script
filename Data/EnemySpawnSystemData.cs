using System;

public enum EnemySpawnPointCode
{
    RandomPoint,
    OneRandomPoint
}

[Serializable]
public struct EnemySpawnSystemData
{
    public int difficultyStartMinute;
    public int difficultyStartSecond;
    public bool bDangerSign;
    public EnemySpawnPointCode enemySpawnPointCode;
    public EnemySpawnLimitData[] enemySpawnLimitData;
}

[Serializable]
public struct EnemySpawnLimitData
{
    [StringFromScriptableObject] 
    public string enemyKey;
    public int numberOfSpawn;
    public float spawningTime;
}
