using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BuffStraightBullet : StraightBullet
{
    [SerializeField]
    protected TargetCharacterCode targetCodesFromBuff;
    [SerializeField]
    [StringFromScriptableObject] 
    protected string effectOfBuffExplosionKey; 
    [SerializeField]
    [StringFromScriptableObject] 
    protected string effectOfDebuffExplosionKey;
    [SerializeField]
    private float recoveryValue;
    [SerializeField]
    [StringFromScriptableObject] 
    private string buffKey;
    [SerializeField]
    private float buffTime;
    [SerializeField]
    [StringFromScriptableObject] 
    private string debuffKey;
    [SerializeField]
    private float debuffTime;

    protected override void DamageCharacter(Character _character, Vector3 crashPoint)
    {
        if (targetCodesFromBuff.IsTargetCode(_character.characterCode))
        {
            if (effectOfBuffExplosionKey != "")
                EffectPoolManager.Instance.SpawnObject(effectOfBuffExplosionKey, crashPoint, transform.rotation);

            if (buffKey != "")
                _character.photonView.RPC("ApplieBuff", RpcTarget.All, buffKey, buffTime);
            if (recoveryValue > 0)
                _character.photonView.RPC("Recovery", RpcTarget.All, recoveryValue, true);
        }
        else
        {
            if (effectOfDebuffExplosionKey != "")
                EffectPoolManager.Instance.SpawnObject(effectOfDebuffExplosionKey, crashPoint, transform.rotation);

            if (debuffKey != "")
                _character.photonView.RPC("ApplieBuff", RpcTarget.All, debuffKey, debuffTime);
            _character.photonView.RPC("Damage", RpcTarget.All, GetBulletDamage(), GetOwnerViewID());
        }
    }
}
