using UnityEngine;
using Photon.Pun;

public class StraightBullet : Bullet
{
    public int maxCollisions; 

    private int maxCollisionsCount;
    private float serverDelayValue;

    public override void OnEnableBullet() 
    {
        maxCollisionsCount = 0;
    }
    public override void SetInitialInformation(Vector3 _position, Quaternion _rotation, float _distance) { }

    public override void SetMoveInterpolation(float _lag)
    {
        serverDelayValue = _lag;
    }
    public override void OnUpdateMovement()
    {
        if(serverDelayValue > 0)
        {
            transform.Translate(0, 0, speed * Time.deltaTime * 0.5f);
            serverDelayValue -= Time.deltaTime * 0.5f;
        }
        transform.Translate(0, 0, speed * Time.deltaTime);
    }
    public override void OnMaxDistanceExceededt()
    {
        gameObject.SetActive(false);
    }

    protected virtual void DamageCharacter(Character _character, Vector3 crashPoint)
    {
        _character.photonView.RPC("Damage", RpcTarget.All, GetBulletDamage(), GetOwnerViewID());
    }

    private void OnTriggerEnter(Collider other)
    {
        Vector3 crashPoint = other.ClosestPointOnBounds(transform.position);

        if (other.CompareTag("Wall"))
        {
            gameObject.SetActive(false);
        }
        if (other.CompareTag("Character"))
        {
            Character character = other.GetComponentInParent<Character>();
            if (character.photonView.ViewID == GetOwnerViewID())
                return;
            if (targetCodesFromAttackDamage.IsTargetCode(character.characterCode))
            {
                CheckMaxCollisions();

                if (effectOfExplosionKey != "")
                    EffectPoolManager.Instance.SpawnObject(effectOfExplosionKey, crashPoint, transform.rotation);

                if (!PhotonNetwork.IsMasterClient)
                    return;

                DamageCharacter(character, crashPoint); 
                return;
            }
            else
                return;
        }

        if (effectOfExplosionKey != "")
            EffectPoolManager.Instance.SpawnObject(effectOfExplosionKey, crashPoint, transform.rotation);

    }
    private void CheckMaxCollisions()
    {
        if (++maxCollisionsCount >= maxCollisions)
        {
            gameObject.SetActive(false);
        }
    }

}
