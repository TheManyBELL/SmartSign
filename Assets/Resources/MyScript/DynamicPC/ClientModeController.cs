using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientModeController : MonoBehaviour
{
    public GameObject MixedRealityToolkit;
    public GameObject MixedRealityPlayspace;
    public GameObject MixedRealitySceneContent;
    public GameObject CameraRig;

    public GameObject DepthCamera;

    void Start()
    {
        if (GlobleInfo.ClientMode.Equals(CameraMode.VR))
        {
            CameraRig.SetActive(true);
            MixedRealityToolkit.SetActive(false);
            MixedRealityPlayspace.SetActive(false);
            MixedRealitySceneContent.SetActive(false);
            DepthCamera.SetActive(true);
        }
        if (GlobleInfo.ClientMode.Equals(CameraMode.AR))
        {
            CameraRig.SetActive(false);
            MixedRealityToolkit.SetActive(true);
            MixedRealityPlayspace.SetActive(true);
            MixedRealitySceneContent.SetActive(true);
            DepthCamera.SetActive(true);
        }
    }
}
