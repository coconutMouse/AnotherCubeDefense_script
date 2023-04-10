using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveTimer : MonoBehaviour
{
    public float activeTime;
    private float updateTime;
    private void OnEnable()
    {
        updateTime = activeTime;
    }
    private void OnDisable()
    {
        gameObject.SetActive(false);
    }
    private void Update()
    {
        updateTime -= Time.deltaTime;
        if (updateTime <= 0) 
            gameObject.SetActive(false);
    }
}
