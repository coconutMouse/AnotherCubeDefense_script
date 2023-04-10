using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class StraightExplosionBullet : StraightBullet
{
    public float explosionRange;

    protected override void DamageCharacter(Character _character, Vector3 crashPoint)
    {
        List<Character> activityCharacters = CharacterManager.Instance.GetActivityCharacters();
        for (int i = 0; i < activityCharacters.Count; i++)
        {
            Character character = activityCharacters[i];
            if (!targetCodesFromAttackDamage.IsTargetCode(character.characterCode))
                continue;

            if (explosionRange >= Vector3.Distance(crashPoint, character.transform.position))
            {
                character.photonView.RPC("Damage", RpcTarget.All, GetBulletDamage(), GetOwnerViewID());
            }
        }
    }
}
