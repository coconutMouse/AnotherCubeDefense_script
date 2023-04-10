using UnityEngine;
using System;

[Serializable]
public class BuffStackData
{
    public Bufftype bufftype;
    public float maxHpBuffValue;
    public float hpRegenBuffValue;
    public float attackDamageBuffValue;
    public float attackDelayTimeBuffValue;
    public float movementSpeedBuffValue;

    public static BuffStackData operator +(BuffStackData _data1, BuffStackData _data2)
    {
        BuffStackData data = new BuffStackData();
        data.maxHpBuffValue = _data1.maxHpBuffValue + _data2.maxHpBuffValue;
        data.hpRegenBuffValue = _data1.hpRegenBuffValue + _data2.hpRegenBuffValue;
        data.attackDamageBuffValue = _data1.attackDamageBuffValue + _data2.attackDamageBuffValue;
        data.attackDelayTimeBuffValue = _data1.attackDelayTimeBuffValue + _data2.attackDelayTimeBuffValue;
        data.movementSpeedBuffValue = _data1.movementSpeedBuffValue + _data2.movementSpeedBuffValue;
        return data;
    }
    public static BuffStackData operator -(BuffStackData _data1, BuffStackData _data2)
    {
        BuffStackData data = new BuffStackData();
        data.maxHpBuffValue = _data1.maxHpBuffValue - _data2.maxHpBuffValue;
        data.hpRegenBuffValue = _data1.hpRegenBuffValue - _data2.hpRegenBuffValue;
        data.attackDamageBuffValue = _data1.attackDamageBuffValue - _data2.attackDamageBuffValue;
        data.attackDelayTimeBuffValue = _data1.attackDelayTimeBuffValue - _data2.attackDelayTimeBuffValue;
        data.movementSpeedBuffValue = _data1.movementSpeedBuffValue - _data2.movementSpeedBuffValue;
        return data;
    }
    public void ResetBuffStack()
    {
        maxHpBuffValue = 0;
        hpRegenBuffValue = 0;
        attackDamageBuffValue = 0;
        attackDelayTimeBuffValue = 0;
        movementSpeedBuffValue = 0;
    }
}

[Serializable]
public class AppliedBuff
{
    public string buffKey;
    public float buffUpdateTime;
    public BuffStackData buffStackData;
}
