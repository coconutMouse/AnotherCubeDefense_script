using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeRotationValue : MonoBehaviour
{
    private Quaternion originalRotation;
    private void Awake()
    {
        originalRotation = transform.rotation;
    }
    private void LateUpdate()
    {
        transform.rotation = originalRotation;
    }
}
