using UnityEngine;
using Photon.Pun;
using FiniteStateMachine.EnemyState;

public class TwinSelfDestructEnemy : SelfDestructEnemy
{
    [StringFromScriptableObject] public string SpawneEnemyKey;
    protected override void SetupStateLayer() 
    {
        MoveToTargetWithoutOverlapping moveTargetWithoutOverlapping = new MoveToTargetWithoutOverlapping(this);
        SelfDestruct selfDestruct = new SelfDestruct(this, selfDestructTime);

        moveTargetWithoutOverlapping.MakeTransition(() => { return bCollided; }, selfDestruct);
        selfDestruct.SetPostStateEvent(SpawnEnemy);

        defaultState = moveTargetWithoutOverlapping;
    }
    protected override void OnDeath(int _killerViewID)
    {
        base.OnDeath(_killerViewID);
        SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        if (SpawneEnemyKey != "") 
            EnemyNetworkPoolManager.Instance.SpawnObject(SpawneEnemyKey, transform.position, transform.rotation);
        else
            Debug.LogWarning("\"SpawnEnemyKey\" is "+ SpawneEnemyKey + ". Please confirm.");
    }

}
