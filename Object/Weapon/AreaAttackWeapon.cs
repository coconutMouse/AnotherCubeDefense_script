using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AreaAttackWeapon : Weapon
{
    [SerializeField]
    private TargetCharacterCode targetCodesFromAttackDamage;
    [SerializeField]
    private float areaRange;
    [SerializeField]
    private float damageIncreaseRate = 0;
    private List<Character> charactersToBeDamaged = new List<Character>();
    [SerializeField]
    [StringFromScriptableObject] 
    private string effectOfExplosionKey;

    public override void Attack(Vector3[] _positions, Quaternion[] _rotations, float _distance, float _lag)
    {
        for (int i = 0; i < _positions.Length; i++)
        {
            AreaAttack(_positions[i], _rotations[i]);
        }
    }
    protected virtual void DamageCharacters(List<Character> _charactersToBeDamaged)
    {
        Character owner = GetOwner();
        float damage = owner.AttackDamage + (owner.AttackDamage * damageIncreaseRate);
        int viewID = owner.photonView.ViewID;
        for (int i = 0; i < _charactersToBeDamaged.Count; i++)
        {
            _charactersToBeDamaged[i].photonView.RPC("Damage", RpcTarget.All, damage, viewID);
        }
    }

    private void AreaAttack(Vector3 _position, Quaternion _rotation)
    {
        if (effectOfExplosionKey != "")
            EffectPoolManager.Instance.SpawnObject(effectOfExplosionKey, _position, _rotation);

        if (!PhotonNetwork.IsMasterClient)
            return;

        List<Character> activityCharacters = CharacterManager.Instance.GetActivityCharacters();
        charactersToBeDamaged.Clear();
        for (int i = 0; i < activityCharacters.Count; i++)
        {
            Character character = activityCharacters[i];
            if (!targetCodesFromAttackDamage.IsTargetCode(character.characterCode))
                continue;

            if ((areaRange) >= Vector3.Distance(_position, character.transform.position))
            {
                charactersToBeDamaged.Add(character);
            }
        }
        DamageCharacters(charactersToBeDamaged);
    }
}
