using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface IManuallyInstantiatedNetworkObject
{
    void SetInterpolation(double _sentServerTime);
    void SendActiveTrue();
}