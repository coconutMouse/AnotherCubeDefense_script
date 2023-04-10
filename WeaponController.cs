using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public enum WeaponLocationCode
{
    RightWeapon,
    LeftWeapon,
    RightBackWeapon,
    LeftBackWeapon
}

[System.Serializable]
public class Armory
{
    public Transform weaponBundle;
    public Transform bodyWeaponPoint;
    public Transform shootingPosturePoint;

    private Transform weaponPoint;
    private List<Weapon> weapons = new List<Weapon>();
    private Weapon mainWeapon;

    public void Attack(Vector3[] _positions, Quaternion[] _rotations, float _distance, float _lag)
    {
        if (mainWeapon.gameObject.activeSelf) 
            mainWeapon.Attack(_positions, _rotations, _distance, _lag);
    }
    public void SettingWeapons(Character _owner)
    {
        for (int i = 0; i < weaponBundle.childCount; i++)
        {
            Weapon weapon = weaponBundle.GetChild(i).GetComponent<Weapon>();
            weapon.SetOwner(_owner);
            weapons.Add(weapon);
        }
        mainWeapon = weapons[0];
        SetWeaponsBodyPoint();
    }
    public void SetMainWeapon(WeaponCode _weaponCode)
    {
        mainWeapon.gameObject.SetActive(false);

        if (_weaponCode == WeaponCode.None) 
            return;

        foreach (var weapon in weapons)
        {
            if (weapon.weaponCode == _weaponCode)
            {
                mainWeapon = weapon;
                mainWeapon.gameObject.SetActive(true);
                return;
            }
        }
        Debug.LogError("no weapon \"" + _weaponCode.ToString() + "\"");
    }
    public void SetWeaponsShootPoint()
    {
        weaponBundle.position = shootingPosturePoint.position;
        weaponBundle.rotation = shootingPosturePoint.rotation;
        weaponPoint = shootingPosturePoint;
    }
    public void SetWeaponsBodyPoint()
    {
        weaponPoint = bodyWeaponPoint;
    }
    public void UpdateWeaponPoint(float _deltaAdjustPositionSpeed)
    {
        weaponBundle.position = Vector3.Lerp(weaponBundle.position, weaponPoint.position, _deltaAdjustPositionSpeed);
        weaponBundle.rotation = Quaternion.Lerp(weaponBundle.rotation, weaponPoint.rotation, _deltaAdjustPositionSpeed);
    }
    public Vector3[] GetAttackPointsPositions()
    {
        return mainWeapon.GetAttackPointsPositions();
    }
    public Quaternion[] GetAttackPointsRotations()
    {
        return mainWeapon.GetAttackPointsRotations();
    }
    public Weapon GetMainWeapon()
    {
        return mainWeapon;
    }
}



public class WeaponController : MonoBehaviour
{
    public Armory[] ArmoryDats;
    public float adjustPositionSpeed = 3f;
    public float adjustRotationSpeed = 3f;
    public float shootingPostureTime = 3;

    private Character owner;
    private int attackWeaponArmoryNumber;
    private List<Armory> attackWeaponArmoryDatas = new List<Armory>();
    private List<Armory> autoAttackWeaponArmoryDatas = new List<Armory>();
    private float autoAttackDelayTime;
    private float autoAttackDelayUpdateTime;
    private PhotonView photonView;
    private float postureUpdateTime = 0;
    private float attackDelayUpdateTime;
    private bool bShootingPosture = false;
    private bool bAttackReload;

    public void InitialSetting(Character _owner)
    {
        owner = _owner;
        foreach (var weaponsDataCommand in ArmoryDats)
        {
            weaponsDataCommand.SettingWeapons(_owner);
        }
    }
    public void SetWeaponsData(WeaponsData _weaponsData)
    {
        ArmoryDats[(int)WeaponLocationCode.RightWeapon].SetMainWeapon(_weaponsData.rightWeapon);
        ArmoryDats[(int)WeaponLocationCode.LeftWeapon].SetMainWeapon(_weaponsData.leftWeapon);
        ArmoryDats[(int)WeaponLocationCode.RightBackWeapon].SetMainWeapon(_weaponsData.rightBackWeapon);
        ArmoryDats[(int)WeaponLocationCode.LeftBackWeapon].SetMainWeapon(_weaponsData.leftBackWeapon);
        autoAttackDelayTime = _weaponsData.automaticWeaponAttackDelayTime;
        attackWeaponArmoryNumber = 0;
        SetWeaponCommands();
    }

    public void SetWeaponCommands()
    {
        attackWeaponArmoryDatas.Clear();
        autoAttackWeaponArmoryDatas.Clear();
        foreach (Armory weaponsDataCommand in ArmoryDats)
        {
            Weapon weapon = weaponsDataCommand.GetMainWeapon();
            if (weapon.gameObject.activeSelf)
            {
                if (weapon.weaponTypeCode == WeaponTypeCode.Attack)
                    attackWeaponArmoryDatas.Add(weaponsDataCommand);
                else if (weapon.weaponTypeCode == WeaponTypeCode.AutoAttack)
                {
                    weaponsDataCommand.SetWeaponsShootPoint();
                    autoAttackWeaponArmoryDatas.Add(weaponsDataCommand);
                }
            }
        }
    }
    public void Attack(float _targetPointDistance)
    {
        if (attackWeaponArmoryDatas.Count == 0)
            return;

        if (bAttackReload)
            return;
        StartAttackReload();

        Armory weaponsDataCommand = attackWeaponArmoryDatas[attackWeaponArmoryNumber];
        weaponsDataCommand.SetWeaponsShootPoint();

        Vector3[] positions = weaponsDataCommand.GetAttackPointsPositions();
        Quaternion[] rotations = weaponsDataCommand.GetAttackPointsRotations();
        photonView.RPC("ExecuteAttack", RpcTarget.All, attackWeaponArmoryNumber, (object)positions, (object)rotations, _targetPointDistance);
        attackWeaponArmoryNumber++;
        if (attackWeaponArmoryNumber >= attackWeaponArmoryDatas.Count)
            attackWeaponArmoryNumber = 0;
    }
    public void AutoAttack(float _targetPointDistance)
    {
        if (autoAttackWeaponArmoryDatas.Count == 0)
            return;

        autoAttackDelayUpdateTime += Time.deltaTime;
        if (autoAttackDelayUpdateTime >= autoAttackDelayTime)
        {
            autoAttackDelayUpdateTime = 0;
            for (int i = 0; i < autoAttackWeaponArmoryDatas.Count; i++)
            {
                Armory weaponsDataCommand = autoAttackWeaponArmoryDatas[i];
                weaponsDataCommand.SetWeaponsShootPoint();

                Vector3[] positions = weaponsDataCommand.GetAttackPointsPositions();
                Quaternion[] rotations = weaponsDataCommand.GetAttackPointsRotations();
                photonView.RPC("ExecuteAutoAttack", RpcTarget.AllViaServer, i, (object)positions, (object)rotations, _targetPointDistance);
            }
        }
    }
    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }
    private void OnEnable()
    {
        bAttackReload = false;
    }
    private void Update()
    {
        if (bShootingPosture)
        {
            postureUpdateTime -= Time.deltaTime;
            if (postureUpdateTime <= 0)
            {
                bShootingPosture = false;
                foreach (var weaponsDataCommand in attackWeaponArmoryDatas)
                {
                    weaponsDataCommand.SetWeaponsBodyPoint();
                }
            }
        }
        foreach (var weaponsDataCommand in attackWeaponArmoryDatas)
        {
            weaponsDataCommand.UpdateWeaponPoint(Time.deltaTime * adjustPositionSpeed);
        }
        if (photonView.IsMine)
            AttackReload(owner.AttackDelayTime);
    }
    private void StartAttackReload()
    {
        bAttackReload = true;
        attackDelayUpdateTime = 0;
    }
    private void AttackReload(float _attackDelayTime)
    {
        if (bAttackReload)
        {
            attackDelayUpdateTime += Time.deltaTime;
            if (attackDelayUpdateTime >= _attackDelayTime)
            {
                bAttackReload = false;
            }
        }
    }

    [PunRPC]
    private void ExecuteAttack(int _EventWeaponNumber, Vector3[] _positions, Quaternion[] _rotations, float _distance, PhotonMessageInfo _info)
    {
        bShootingPosture = true;
        postureUpdateTime = shootingPostureTime;

        float lag = Mathf.Abs((float)(PhotonNetwork.Time - _info.SentServerTime));

        attackWeaponArmoryDatas[_EventWeaponNumber].SetWeaponsShootPoint();
        attackWeaponArmoryDatas[_EventWeaponNumber].Attack(_positions, _rotations, _distance, lag);
    }
    [PunRPC]
    private void ExecuteAutoAttack(int _EventWeaponNumber, Vector3[] _positions, Quaternion[] _rotations, float _distance, PhotonMessageInfo _info)
    {
        float lag = Mathf.Abs((float)(PhotonNetwork.Time - _info.SentServerTime));

        autoAttackWeaponArmoryDatas[_EventWeaponNumber].Attack(_positions, _rotations, _distance, lag);
    }

}
