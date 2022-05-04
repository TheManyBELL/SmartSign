using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.Extras;

public class DynamicEnableVR : MonoBehaviour
{
    public SteamVR_Action_Boolean ChangeReceiveFrameState;
    public SteamVR_Action_Boolean changeCurrentServer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (ChangeReceiveFrameState.GetStateDown(SteamVR_Input_Sources.RightHand))
        {
            GlobleInfo.isReceiveStateChanged = true;
            Debug.Log("Server now receive frame state CHANGED");
        }
        if (changeCurrentServer.GetStateDown(SteamVR_Input_Sources.RightHand))
        {
            GlobleInfo.CurentServer = (ServerNumber)(((int)GlobleInfo.CurentServer + 1) % 4);
            Debug.Log("Now TCP Server is :"+ GlobleInfo.CurentServer);
        }
        
    }
}
