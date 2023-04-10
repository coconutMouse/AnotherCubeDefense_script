using UnityEngine;
using FiniteStateMachine.EnemyState;

public class GiantSelfDestructEnemy : SelfDestructEnemy
{
    protected override void SetupStateLayer()
    {
        MoveToTarget moveToTarget = new MoveToTarget(this);
        SelfDestruct selfDestruct = new SelfDestruct(this, selfDestructTime);

        moveToTarget.MakeTransition(() => { return bCollided; }, selfDestruct);

        defaultState = moveToTarget;
    }

    protected override void UpdateTarget()
    {
        Character mainTower = (Character)PlayerManager.Instance.mainTowerCharacter;
        Transform targetPlayer = mainTower.transform;
        target = targetPlayer;
    }
}
