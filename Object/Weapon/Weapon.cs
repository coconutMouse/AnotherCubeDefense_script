using UnityEngine;

public enum WeaponCode
{
    None,
    NormalGunWeapon_1,
    NormalGunWeapon_2,
    NormalGunWeapon_3, 

    MachineGunWeapon_1,
    MachineGunWeapon_2,
    RailGunWeapon_1,
    RailGunWeapon_2,
    CannonGunWeapon_1,
    CannonGunWeapon_2,

    RailMachineGunWeapon,
    RailCannonGunWeapon,

    RecoveryGunWeapon_1,
    RecoveryGunWeapon_2,

    BuffAreaTotemWeapon_1,
    BuffAreaTotemWeapon_2,

    BuffGunWeapon_1,
    BuffGunWeapon_2,

    WallCustomWeapon_1,
    WallCustomWeapon_2,
    WallCustomWeapon_3,
}
public enum WeaponTypeCode
{
    Attack,
    AutoAttack,
    Custom
}


public class Weapon : MonoBehaviour
{
    [StringFromScriptableObject] 
    public string effectOfMuzzleFlashKey;
    public WeaponCode weaponCode;
    public WeaponTypeCode weaponTypeCode;

    protected Animator animator;

    [SerializeField]
    private Transform[] attackPoints;
    private Character owner;
    private Vector3[] attackPointPositions;
    private Quaternion[] attackPointRotations;

    public void SetOwner(Character _owner)
    {
        owner = _owner;
    }
    public Character GetOwner()
    {
        return owner;
    }
    public Transform[] GetAttackPoints()
    {
       return attackPoints;
    }
    public Vector3[] GetAttackPointsPositions()
    {
        for (int i = 0; i < attackPoints.Length; i++) 
        {
            attackPointPositions[i] = attackPoints[i].position;
        }
        return attackPointPositions;
    }
    public Quaternion[] GetAttackPointsRotations()
    {
        for (int i = 0; i < attackPoints.Length; i++)
        {
            attackPointRotations[i] = attackPoints[i].rotation;
        }
        return attackPointRotations;
    }

    public virtual void Attack(Vector3[] _positions, Quaternion[] _rotations, float _distance, float _lag) {}

    private void Awake()
    {
        animator = GetComponent<Animator>();
        attackPointPositions = new Vector3[attackPoints.Length];
        attackPointRotations = new Quaternion[attackPoints.Length];
    }
}
