using UnityEngine;
using FiniteStateMachine.EnemyState;

public class ShooterSelfDestructEnemy : SelfDestructEnemy
{
    public WeaponController weaponController;
    public WeaponsData weaponsData;
    public float shootingDistance;
    protected override void Awake()
    {
        base.Awake();
        weaponController.InitialSetting(this);
        weaponController.SetWeaponsData(weaponsData);
    }
    protected override void SetupStateLayer()
    {
        MoveToTargetWithoutOverlapping moveTargetWithoutOverlapping = new MoveToTargetWithoutOverlapping(this);
        SelfDestruct selfDestruct = new SelfDestruct(this, selfDestructTime);
        Shooting shooting = new Shooting(this, weaponController);

        moveTargetWithoutOverlapping.MakeTransition(() => { return bCollided; }, selfDestruct);
        moveTargetWithoutOverlapping.MakeTransition(CheckWithinShootingRange, shooting);

        shooting.MakeTransition(() => { return bCollided; }, selfDestruct);
        shooting.MakeTransition(CheckNotWithinShootingRange, moveTargetWithoutOverlapping);

        defaultState = moveTargetWithoutOverlapping;
    }
 
    private bool CheckWithinShootingRange()
    {
        Transform target = GetTarget();
        if (target == null)
            return false;

        float targetDistance = Vector3.Distance(transform.position, target.position);
        if (targetDistance <= shootingDistance)
            return true;
        return false;
    }
    private bool CheckNotWithinShootingRange()
    {
        Transform target = GetTarget();
        if (target == null)
            return true;

        float targetDistance = Vector3.Distance(transform.position, target.position);
        if (targetDistance > shootingDistance)
            return true;
        return false;
    }
}
