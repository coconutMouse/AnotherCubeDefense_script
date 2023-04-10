
using UnityEngine;
using Photon.Pun;
using FiniteStateMachine.EnemyState;

public class SelfDestructEnemy : EnemyCharacter
{
    public float selfDestructTime;

    protected bool bCollided = false;

    protected override void SetupStateLayer() 
    {
        MoveToTargetWithoutOverlapping moveTargetWithoutOverlapping = new MoveToTargetWithoutOverlapping(this);
        SelfDestruct selfDestruct = new SelfDestruct(this, selfDestructTime);

        moveTargetWithoutOverlapping.MakeTransition(() => { return bCollided; }, selfDestruct);

        defaultState = moveTargetWithoutOverlapping;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        bCollided = false;
    }

    [PunRPC]
    protected void CollideTarget()
    {
        bCollided = true;
    }
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient || !other.CompareTag("Character"))
            return;

        CharacterCode characterCode = other.GetComponentInParent<Character>().characterCode;
        if (targetCodesFromAttack.IsTargetCode(characterCode))
            photonView.RPC("CollideTarget", RpcTarget.AllViaServer); 
    }
}
