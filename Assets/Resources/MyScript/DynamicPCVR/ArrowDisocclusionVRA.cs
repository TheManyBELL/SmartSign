using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class ArrowDisocclusionVRA : MonoBehaviour
{
    private MirrorControllerA mirrorController;

    private void Start()
    {
        mirrorController = GetComponentInParent<MirrorControllerA>();

    }

    private void Update()
    {
        for (int i = 0;i<mirrorController.syncArrowList.Count;++i)
        {
            DPCArrow curArrow = mirrorController.syncArrowList[i];
            arrowDisocclusion(ref curArrow);
            mirrorController.CmdUpdateDPCArrow(curArrow);
        }
    }

    private void arrowDisocclusion(ref DPCArrow oldArrow)
    {
        // De occlusion calculation is performed here

    }
}
