using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolObject : MonoBehaviour
{
    public delegate void TriggerEvent(Component _component);
    public event TriggerEvent OnDespawnTrigger;
    public event TriggerEvent OnDestroyTrigger;

    private Component component;

    public void SettingTriggerEvent(Component _component, TriggerEvent _onDespawnEvent, TriggerEvent _onDestroyTrigger)
    {
        component = _component;
        OnDespawnTrigger += _onDespawnEvent;
        OnDestroyTrigger += _onDestroyTrigger;
    }

    private void OnDisable()
    {
        OnDespawnTrigger(component);
    }
    private void OnDestroy()
    {
        OnDestroyTrigger(component);
    }
}