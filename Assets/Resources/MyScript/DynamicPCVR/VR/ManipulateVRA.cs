using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.Extras;

public class ManipulateVRA : MonoBehaviour
{
    public GameObject targetObj;    // manipulate target object
    public GameObject grabObj;      // for rotate
    public GameObject VRHandLeft;
    public GameObject VRHandRight;  
    private Vector3 VRhandtPosPre;  // for translate

    public SteamVR_Action_Boolean manipulate;

    private MirrorControllerA myController;

    // Start is called before the first frame update
    void Start()
    {
        myController = GetComponentInParent<MirrorControllerA>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!VRHandLeft)
        {
            VRHandLeft = GameObject.Find("VR_Avator/LeftHandController");
            VRHandRight = GameObject.Find("VR_Avator/RightHandController");
        }

        if (targetObj == null || VRHandLeft == null) return;

        bool operated = false;
        if (manipulate.GetState(SteamVR_Input_Sources.LeftHand))    // Rot
        {
            GrabObject();
            targetObj.transform.rotation = targetObj.transform.rotation;
            operated = true;
        }

        if (manipulate.GetState(SteamVR_Input_Sources.RightHand))   // Translate
        {
            Vector3 offsetPos = VRHandRight.transform.position - VRhandtPosPre;
            targetObj.transform.position += offsetPos;
            operated = true;
        }

        if (!manipulate.GetState(SteamVR_Input_Sources.LeftHand))   //Rot Release
        {
            ReleaseObject();
        }

        if (operated && myController.syncSplitPosList[myController.syncSplitPosList.Count-1].valid)     // 操作了并且实时更新
        {
            myController.CmdUpdateDPCSplitPos(new DPCSplitPosture()
            {
                index = myController.syncSplitPosList.Count - 1,
                valid = true,
                position = targetObj.transform.position,
                rotation = targetObj.transform.rotation,
            });
        }

        VRhandtPosPre = VRHandRight.transform.position;
    }

    public void RegisterObj(GameObject o) => targetObj = o;

    public void UnRegisterObj() {
        
        myController.CmdUpdateDPCSplitPos(new DPCSplitPosture()     // 释放时一定更新
        {
            index = myController.syncSplitPosList.Count - 1,
            valid = true,
            position = targetObj.transform.position,
            rotation = targetObj.transform.rotation,
        });

        grabObj.transform.rotation = new Quaternion();  // 为下一个物体的旋转做准备，暂时不知道是否需要
        targetObj = null;
    }

    void GrabObject()
    {
        if (!VRHandLeft.GetComponent<FixedJoint>().connectedBody)
            VRHandLeft.GetComponent<FixedJoint>().connectedBody = grabObj.GetComponent<Rigidbody>();
    }

    void ReleaseObject()
    {
        VRHandLeft.GetComponent<FixedJoint>().connectedBody = null;
    }
}
