using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SymbolDisocclusionVRA : MonoBehaviour
{
    private MirrorControllerA mirrorController; // 网络中控
    private GlobalUtils globalUtils; // 通用工具类

    public GameObject rotateSymbolPrefab;
    public GameObject pressSymbolPrefab;

    private GameObject rotateSymbolObject;
    private GameObject pressSymbolObject;

    // De occlusion parameters
    public float raiseStep = 0.1f;


    private void Start()
    {
        mirrorController = GetComponentInParent<MirrorControllerA>();
        globalUtils = GetComponentInParent<GlobalUtils>();

        rotateSymbolObject = Instantiate(rotateSymbolPrefab);
        pressSymbolObject = Instantiate(pressSymbolObject);

    }

    // 1.for symbol in server symbol list 
    // 
    private void Update()
    {
        // update rotation symbol
        for (int i = 0; i < mirrorController.syncRotationList.Count; ++i)
        {
            DPCSymbol curRotation = mirrorController.syncRotationList[i];
            // initialize symbol's transform
            rotateSymbolObject.transform.position = curRotation.position;
            rotateSymbolObject.transform.forward = curRotation.up;
            // de occlusion
            symbolDisocclusion(rotateSymbolObject);
            // update curRotation's transform
            curRotation.position_new = rotateSymbolObject.transform.position;
            curRotation.up_new = rotateSymbolObject.transform.forward;
            mirrorController.CmdUpdateDPCRotation(curRotation);
        }
        // update press symbol
        for (int i = 0; i < mirrorController.syncRotationList.Count; ++i)
        {
            DPCSymbol curPress = mirrorController.syncPressList[i];
            pressSymbolObject.transform.position = curPress.position;
            pressSymbolObject.transform.right = curPress.up;
            symbolDisocclusion(pressSymbolObject);
            curPress.position_new = pressSymbolObject.transform.position;
            curPress.up_new = pressSymbolObject.transform.right;
            mirrorController.CmdUpdateDPCPress(curPress);
        }
    }

    private void symbolDisocclusion(GameObject t)
    {
        // De occlusion calculation is performed here
        while (!globalUtils.GameObjectVisible(t))
        {
            t.transform.position += raiseStep * Vector3.up;
        }
    }
}
