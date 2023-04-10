using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
 
public enum CharacterCode
{
    Player, Enemy, MainTower
}
public enum Bufftype
{
    Buff,
    Debuff
}
[System.Serializable]
public class TargetCharacterCode
{
    [SerializeField]
    private CharacterCode[] TargetCodes;
    public bool IsTargetCode(CharacterCode _characterCode)
    {
        foreach (CharacterCode characterCode in TargetCodes)
        {
            if (_characterCode == characterCode)
                return true;
        }
        return false;
    }
}

public abstract class Character : MonoBehaviourPunCallbacks
{
    public CharacterCode characterCode;

    [StringFromScriptableObject] public string effectOfDeathKey;
    [StringFromScriptableObject] public string effectOfActiveKey;
    public float hp;

    [SerializeField]
    private float maxHp;
    [SerializeField]
    private float hpRegen;
    [SerializeField]
    private float attackDamage;
    [SerializeField]
    private float attackDelayTime;
    [SerializeField]
    private float movementSpeed;
    [SerializeField]
    private bool bHpRegen = false;
    private float hpRegenUpdateTime = 0;
    private bool bCharacterUpdate = true;
    [SerializeField]
    private ParticleSystem buffParticleSystem;
    [SerializeField]
    private ParticleSystem debuffParticleSystem;
    private LinkedList<AppliedBuff> appliedBuffs = new LinkedList<AppliedBuff>();
    private int[] buffCounts = new int[2];
    private BuffStackData characterBuffStack = new BuffStackData();

    public float MaxHp
    {
        set => maxHp = value;
        get => maxHp + (maxHp * characterBuffStack.maxHpBuffValue);
    }
    public float HpRegen
    {
        set => hpRegen = value;
        get => hpRegen + (hpRegen * characterBuffStack.hpRegenBuffValue);
    }
    public float AttackDamage
    {
        set => attackDamage = value;
        get => attackDamage + (attackDamage * characterBuffStack.attackDamageBuffValue);
    }
    public float AttackDelayTime
    {
        set => attackDelayTime = value;
        get => attackDelayTime + (attackDelayTime * -characterBuffStack.attackDelayTimeBuffValue);
    }
    public float MovementSpeed
    {
        set => movementSpeed = value;
        get => movementSpeed + (movementSpeed * characterBuffStack.movementSpeedBuffValue);
    }
    public override void OnEnable()
    {
        base.OnEnable();
        CharacterManager.Instance.ActivityCharacter(this);
        if(effectOfActiveKey != "")
            EffectPoolManager.Instance.SpawnObject(effectOfActiveKey, transform.position);
    }
    public override void OnDisable()
    {
        base.OnDisable();
        if (!this.gameObject.scene.isLoaded)
            return;
        CharacterManager.Instance.InactiveCharacter(this);   
    }
    public void SetCharacterUpdate(bool active)
    {
        bCharacterUpdate = active;
    }
    public void ActiveBuffParticle(Bufftype _bufftype, bool _active)
    {
        if (_bufftype == Bufftype.Buff)
        {
            if (_active)
                buffParticleSystem.Play();
            else
                buffParticleSystem.Stop();
        }
        else if (_bufftype == Bufftype.Debuff)
        {
            if (_active)
                debuffParticleSystem.Play();
            else
                debuffParticleSystem.Stop();
        }
    }

    [PunRPC]
    public void ApplieBuff(string _buffKey, float _buffTime)
    {
        foreach (AppliedBuff node in appliedBuffs)
        {
            if(node.buffKey == _buffKey)
            {
                node.buffUpdateTime = _buffTime;
                return;
            }
        }
        AppliedBuff appliedBuff = new AppliedBuff();
        appliedBuff.buffKey = _buffKey;
        appliedBuff.buffUpdateTime = _buffTime;
        appliedBuff.buffStackData = CharacterManager.Instance.GetCharacterBuffStackData(_buffKey);
        characterBuffStack += appliedBuff.buffStackData;
        appliedBuffs.AddLast(appliedBuff);

        Bufftype bufftype = appliedBuff.buffStackData.bufftype;
        buffCounts[(int)bufftype]++;
        if (buffCounts[(int)bufftype] == 1) 
            ActiveBuffParticle(bufftype, true);
    }

    [PunRPC]
    public void Recovery(float _recoveryValue, bool _effect) 
    {
        if(_effect)
            EffectPoolManager.Instance.SpawnObject("HealExplosion", transform.position, transform.rotation);

        if (MaxHp <= (hp + _recoveryValue))
            hp = MaxHp;
        else
            hp += _recoveryValue;
        OnRecovery(_recoveryValue);
    }


    [PunRPC]
    public void Damage(float _damage, int _damageOwnerViewID)
    {
        hp -= _damage;
        if (hp <= 0 && photonView.IsMine)
        {
            photonView.RPC("Death", RpcTarget.All, _damageOwnerViewID);
        }
        OnDamage(_damage, _damageOwnerViewID);
    }

    [PunRPC]
    public void Death(int _killerViewID)
    {
        if(effectOfDeathKey != "")
            EffectPoolManager.Instance.SpawnObject(effectOfDeathKey, transform.position, transform.rotation);

        characterBuffStack.ResetBuffStack();
        appliedBuffs.Clear();

        gameObject.SetActive(false);
        OnDeath(_killerViewID);
    }
    [PunRPC]
    public void Death(int _killerViewID, string _effectOfDeathKey)
    {
        if (_effectOfDeathKey != "")
            EffectPoolManager.Instance.SpawnObject(_effectOfDeathKey, transform.position, transform.rotation);

        characterBuffStack.ResetBuffStack();
        appliedBuffs.Clear();

        gameObject.SetActive(false);
        OnDeath(_killerViewID);
    }
    protected abstract void OnUpdate();
    protected abstract void OnFixedUpdate();
    protected virtual void OnDamage(float _damage, int _damageOwnerViewID) {}
    protected virtual void OnDeath(int _killerViewID) {}
    protected virtual void OnRecovery(float _recoveryValue) {}

    private void Update()
    {
        if (!bCharacterUpdate)
            return;
        BuffStackUpdate();
        HpRegenUpdate();
        OnUpdate();
    }
    private void FixedUpdate()
    {
        if (!bCharacterUpdate)
            return;
        OnFixedUpdate();
    }
    private void HpRegenUpdate()
    {
        if (!bHpRegen || !photonView.IsMine)
            return;
        if (hp == MaxHp)
            return;
        hpRegenUpdateTime += Time.deltaTime;
        if (hpRegenUpdateTime >= 1)
        {
            hpRegenUpdateTime = 0;
            photonView.RPC("Recovery", RpcTarget.All, HpRegen, false);
        }
    }
    private void BuffStackUpdate()
    {
        var node = appliedBuffs.First;
        while (node != null)
        {
            var next = node.Next;
            node.Value.buffUpdateTime -= Time.deltaTime;
            if (node.Value.buffUpdateTime <= 0)
            {
                characterBuffStack -= node.Value.buffStackData;
                appliedBuffs.Remove(node);

                Bufftype bufftype = node.Value.buffStackData.bufftype;
                buffCounts[(int)bufftype]--;
                if (buffCounts[(int)bufftype] == 0)
                {
                    ActiveBuffParticle(bufftype, false);
                }
            }
            node = next;
        }
    }
}


