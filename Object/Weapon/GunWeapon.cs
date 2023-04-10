using UnityEngine;

public class GunWeapon : Weapon
{
    [SerializeField]
    [StringFromScriptableObject] 
    private string bulletKey;
    [SerializeField]
    private float distanceIncreaseRate = 0f;

    public override void Attack(Vector3[] _positions, Quaternion[] _rotations, float _distance, float _lag)
    {
        if (animator != null)
            animator.SetBool("Shoot", true);
        if (effectOfMuzzleFlashKey != "")
        {
            Transform[] attackPoints = GetAttackPoints();
            foreach (Transform point in attackPoints)
            {
                EffectPoolManager.Instance.SpawnObject(effectOfMuzzleFlashKey, point.position, point.rotation, point);
            }
        }

        if (bulletKey == "")
            return;

        Character owner = GetOwner();
        for (int i = 0; i < _positions.Length; i++)
        {
            Bullet bullet = BulletPoolManager.Instance.SpawnObject(bulletKey, _positions[i], _rotations[i]);
            if (bullet == null)
                return;

            bullet.SetOwnerViewID(owner.photonView.ViewID);
            bullet.IncreaseMaxDistance(distanceIncreaseRate);
            bullet.SetInitialInformation(_positions[i], _rotations[i], _distance);
            bullet.SetMoveInterpolation(_lag);
            bullet.SetBulletDamage(owner.AttackDamage);
        }
    }
}
