using UnityEngine;

public abstract class Bullet : MonoBehaviour
{
    public float damageIncreaseRate;
    public float speed;
    public float maxDistance;

    [SerializeField]
    protected TargetCharacterCode targetCodesFromAttackDamage;
    [SerializeField][StringFromScriptableObject]
    protected string effectOfExplosionKey;

    private int ownerViewID;
    private float bulletDamage;
    private float increasedMaxDistance;
    private Vector3 spawnPosition;
    private bool bMaxDistanceExceededt;

    public abstract void SetInitialInformation(Vector3 _position, Quaternion _rotation, float _distance);
    public abstract void SetMoveInterpolation(float _lag);
    public abstract void OnUpdateMovement();
    public abstract void OnEnableBullet();
    public abstract void OnMaxDistanceExceededt();

    public float GetMaxDistance()
    {
        return maxDistance;
    }
    public float GetBulletDamage()
    {
        return bulletDamage;
    }
    public void SetOwnerViewID(int _ownerViewID)
    {
        ownerViewID = _ownerViewID;
    }
    public int GetOwnerViewID()
    {
        return ownerViewID;
    }
    public void SetBulletDamage(float _attackDamage)
    {
        bulletDamage = (_attackDamage + (_attackDamage * damageIncreaseRate));
    }
    public void SetMaxDistance(float _distance)
    {
        maxDistance = _distance;
        increasedMaxDistance = maxDistance;
    }
    public void IncreaseMaxDistance(float _increaseRate)
    {
        increasedMaxDistance = maxDistance + (maxDistance * _increaseRate);
    }

    private void OnEnable()
    {
        increasedMaxDistance = maxDistance;
        bMaxDistanceExceededt = false;
        spawnPosition = transform.position;
        OnEnableBullet();
    }
    private void Update()
    {
        OnUpdateMovement();

        if (bMaxDistanceExceededt)
            return;

        float moveDistance = Vector3.Distance(spawnPosition, transform.position);
        if (moveDistance >= increasedMaxDistance)
        {
            bMaxDistanceExceededt = true;
            OnMaxDistanceExceededt();
        }
    }
}




