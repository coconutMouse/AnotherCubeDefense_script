using UnityEngine;
using System;

[Serializable]
public class JobData : WeaponsData
{
    public Sprite jobIcon; 
    public Color jobColor;
    public string tag;
    public int promotionLevel;
    [StringFromScriptableObject]
    public string[] promotedJobs;
    public float maxHp;
    public float hpRegen;
    public float attackDamage;
    public float attackDelayTime;
    public float movementSpeed;
}
[Serializable]
public class WeaponsData
{
    public WeaponCode rightWeapon;
    public WeaponCode leftWeapon;
    public WeaponCode rightBackWeapon;
    public WeaponCode leftBackWeapon;
    public float automaticWeaponAttackDelayTime;
}