using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraUpdateARA : MonoBehaviour
{
    private MirrorControllerA mirrorController;
    private Camera arCamera;

    // Start is called before the first frame update
    void Start()
    {
        mirrorController = GetComponentInParent<MirrorControllerA>();
        
        arCamera = GameObject.Find("MixedRealityPlayspace/Main Camera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (arCamera!=null)
        {
            mirrorController.CmdUpdateDepthCamera(new CameraParams
            {
                position = arCamera.transform.position,
                rotation = arCamera.transform.rotation
            });
        }
        else
        {
            Debug.LogError("AR Camera not found");
        }
    }
}
