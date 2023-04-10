using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ParabolicExplosionBullet : Bullet
{
    public float explosionRange;
    public Rigidbody myRigidbody;

    private Vector3 currentPosition;
    private Vector3 targetPosition;
    private float shootAngle;
    private List<Character> charactersToBeDamaged = new List<Character>();

    public void SetVelocity(Vector3 _velocity) 
    {
        myRigidbody.velocity = _velocity;
    }
    public override void OnEnableBullet() { }
    public override void OnUpdateMovement() { }
    public override void OnMaxDistanceExceededt()
    {
        gameObject.SetActive(false);
    }
    public override void SetInitialInformation(Vector3 _position, Quaternion _rotation, float _distance)
    {
        Vector3 direction = (_rotation * Vector3.forward);

        currentPosition = _position;
        Vector3 shootDirection = direction;
        shootDirection.Normalize();

        Vector3 targetPointDirection = direction;
        targetPointDirection.y = 0;
        targetPointDirection.Normalize();

        shootAngle = Vector3.Angle(targetPointDirection, shootDirection) * Mathf.Deg2Rad;
        targetPosition = currentPosition + (targetPointDirection * _distance);
        targetPosition.y = 0;
    }
    public override void SetMoveInterpolation(float _lag)
    {
        float gravity = Physics.gravity.magnitude;

        Vector3 planarTarget = new Vector3(targetPosition.x, 0, targetPosition.z);
        Vector3 planarPosition = new Vector3(currentPosition.x, 0, currentPosition.z);

        float distance = Vector3.Distance(planarTarget, planarPosition);
        float yOffset = currentPosition.y - targetPosition.y;

        float initialVelocity = (1 / Mathf.Cos(shootAngle)) * Mathf.Sqrt((0.5f * gravity * Mathf.Pow(distance, 2)) / (distance * Mathf.Tan(shootAngle) + yOffset));

        Vector3 velocity = new Vector3(0f, initialVelocity * Mathf.Sin(shootAngle), initialVelocity * Mathf.Cos(shootAngle));

        float angleBetweenObjects = Vector3.Angle(Vector3.forward, planarTarget - planarPosition) * (targetPosition.x > currentPosition.x ? 1 : -1);
        Vector3 finalVelocity = Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * velocity;

        Vector3 lagPositionVector = new Vector3(finalVelocity.x * _lag, finalVelocity.y * _lag - (0.5f * gravity * Mathf.Pow(_lag, 2)), finalVelocity.z * _lag);
        transform.position += lagPositionVector;

        Vector3 finalLagVelocity = new Vector3(finalVelocity.x, finalVelocity.y - (gravity * _lag), finalVelocity.z);
        myRigidbody.velocity = finalLagVelocity;
    }
    protected virtual void DamageCharacters(List<Character> _charactersToBeDamaged)
    {
        for (int i = 0; i < _charactersToBeDamaged.Count; i++)
        {
            _charactersToBeDamaged[i].photonView.RPC("Damage", RpcTarget.All, GetBulletDamage(), GetOwnerViewID());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Character"))
        {
            Character character = other.GetComponentInParent<Character>();
            if (!targetCodesFromAttackDamage.IsTargetCode(character.characterCode))
                return;
        }

        if (effectOfExplosionKey != "")
            EffectPoolManager.Instance.SpawnObject(effectOfExplosionKey, transform.position);

        gameObject.SetActive(false);
        if (!PhotonNetwork.IsMasterClient)
            return;

        List<Character> activityCharacters = CharacterManager.Instance.GetActivityCharacters();
        charactersToBeDamaged.Clear();
        for (int i = 0; i < activityCharacters.Count; i++) 
        {
            Character character = activityCharacters[i];
            if (!targetCodesFromAttackDamage.IsTargetCode(character.characterCode)) 
                continue;

            if (explosionRange >= Vector3.Distance(transform.position, character.transform.position))
            {
                charactersToBeDamaged.Add(character);
            }
        }
        DamageCharacters(charactersToBeDamaged);
    }
}
