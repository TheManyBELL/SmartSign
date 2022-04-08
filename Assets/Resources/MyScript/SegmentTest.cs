using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SegmentTest : MonoBehaviour
{

    public Mirror_MyController mirrorMyController;
    public Vector3 startPoint_test;
    public Vector3 endPoint_test;
    // Start is called before the first frame update
    void Start()
    {
        //mirrorMyController = GetComponent<Mirror_MyController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnGUI()
    {
        if (GlobleInfo.ClientMode.Equals(CameraMode.AR)) { return; } // 只有 VR端执行
        GUILayout.BeginArea(new Rect(Screen.width / 2 - 50, Screen.height / 2 - 50, 100, 999));
        if (GUILayout.Button("发送线段"))
        {
            Debug.Log("点击了还没发送");
            SegmentInfo testSegment = new SegmentInfo()
            {
                startPoint = startPoint_test,
                endPoint = endPoint_test
            };
            mirrorMyController.CmdUpdateSegmentInfo(testSegment);
            Debug.Log("发送了");
        }
        GUILayout.EndArea();
    }
}
