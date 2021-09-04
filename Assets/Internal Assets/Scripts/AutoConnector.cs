#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParrelSync;

public class AutoConnector : MonoBehaviour
{
    private void Start()
    {
        NetworkHUD network = GetComponent<NetworkHUD>();
        if (ClonesManager.IsClone())
            network.StartClient();
        else
            network.StartServer();
    }
}

#endif