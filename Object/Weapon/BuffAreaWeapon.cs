using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BuffAreaWeapon : AreaAttackWeapon
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

    protected override void DamageCharacters(List<Character> _charactersToBeDamaged)
    {
        for (int i = 0; i < _charactersToBeDamaged.Count; i++)
        {
            Vector3 position = _charactersToBeDamaged[i].transform.position;
            Quaternion rotation = _charactersToBeDamaged[i].transform.rotation;
            if (targetCodesFromBuff.IsTargetCode(_charactersToBeDamaged[i].characterCode))
            {
                if (effectOfBuffExplosionKey != "")
                    EffectPoolManager.Instance.SpawnObject(effectOfBuffExplosionKey, position, rotation);

                if (buffKey != "")
                    _charactersToBeDamaged[i].photonView.RPC("ApplieBuff", RpcTarget.All, buffKey, buffTime);
                if (recoveryValue > 0)
                    _charactersToBeDamaged[i].photonView.RPC("Recovery", RpcTarget.All, recoveryValue, true);
            }
            else
            {
                if (effectOfDebuffExplosionKey != "")
                    EffectPoolManager.Instance.SpawnObject(effectOfDebuffExplosionKey, position, rotation);

                if (debuffKey != "")
                    _charactersToBeDamaged[i].photonView.RPC("ApplieBuff", RpcTarget.All, debuffKey, debuffTime);
             }
        }
    }
}
