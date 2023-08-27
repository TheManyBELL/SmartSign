using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCloudController : MonoBehaviour
{

    public string hostIP_ar;
    public string hostIP_vr;
    public string hostIP;

    public GameObject server2_point0;
    public GameObject server2_point1;
    public GameObject server4_point1;

    public Vector3 server2_pointcloud0_pos;
    public Vector3 server2_pointcloud0_rot;
    public Vector3 server2_pointcloud1_pos;
    public Vector3 server2_pointcloud1_rot;
    public Vector3 server4_pointcloud1_pos;

    public GameObject Stand;
    public GameObject HPC;
    public GameObject Fan;
    public GameObject pppp;
    public GameObject LPT;

    // Start is called before the first frame update
    void Start()
    {
        if (GlobleInfo.ClientMode.Equals(CameraMode.VR))
        {
            hostIP = hostIP_vr;
        }
        else
        {
            hostIP = hostIP_ar;
        }

        gameObject.transform.position = new Vector3(0.369f, 1.011f, 2.223f);
        gameObject.transform.eulerAngles = new Vector3(56.864f, 85.834f, 86.211f);


        server2_pointcloud0_pos = new Vector3(-0.009f, -0.006f, -0.01f);
        server2_pointcloud0_rot = new Vector3(0.947f, 0.222f, -1.116f);

        // server2_pointcloud1_pos = new Vector3(-0.043f, -0.014f, 0.06f);
        // server2_pointcloud1_rot = new Vector3(-0.372f, -0.678f, -2.3f);

        server2_pointcloud1_pos = new Vector3(-0.024f, -0.014f, 0.029f);
        server2_pointcloud1_rot = new Vector3(-0.257f, -0.258f, -2.214f);

        server4_pointcloud1_pos = new Vector3(-0.05f, -0.005f, 0.02f);

        GameObject.Find("[CameraRig]").transform.position = new Vector3(-0.545f, -0.421f, 1.737f);
        GameObject.Find("[CameraRig]").transform.eulerAngles = new Vector3(0, -48.943f, 0);

        Stand.transform.localPosition = new Vector3(-1.3635f, -0.3196f, 0.564f);
        Stand.transform.localEulerAngles = new Vector3(44.117f, -123.793f, 89.578f);
        Stand.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        HPC.transform.localPosition = new Vector3(-1.462f, -1.276f, 0.503f);
        HPC.transform.localEulerAngles = new Vector3(7.7f, -37.33f, -51f);
        HPC.transform.localScale = new Vector3(1.0f, 1.1f, 1.04f);

        Fan.transform.localPosition = new Vector3(-1.622f, -0.889f, 0.453f);
        Fan.transform.localEulerAngles = new Vector3(56.924f, 45.59f, 43.14f);
        Fan.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        pppp.transform.localPosition = new Vector3(-1.4437f, -1.1579f, 0.5042f);
        pppp.transform.localEulerAngles = new Vector3(-92.551f, 95.294f, -35.12f);
        // pppp.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        LPT.transform.localPosition = new Vector3(-1.238f, -1.026f, 0.625f);
        LPT.transform.localEulerAngles = new Vector3(-252.517f, -133.826f, 166.038f);
    }

    // Update is called once per frame
    void Update()
    {
        if (!server2_point0)
        {
            server2_point0 = GameObject.Find("PointCloud(Clone)/TCPserver2/PointCloud_0");
            if (server2_point0)
            {
                server2_point0.transform.localPosition = server2_pointcloud0_pos;
                server2_point0.transform.localEulerAngles = server2_pointcloud0_rot;
            }
        }

        if (!server2_point1)
        {
            server2_point1 = GameObject.Find("PointCloud(Clone)/TCPserver2/PointCloud_1");
            if (server2_point0)
            {
                server2_point1.transform.localPosition = server2_pointcloud1_pos;
                server2_point1.transform.localEulerAngles = server2_pointcloud1_rot;
            }
        }

        if (!server4_point1)
        {
            server4_point1 = GameObject.Find("PointCloud(Clone)/TCPserver4/PointCloud_1");
            if (server4_point1)
            {
                server4_point1.transform.localPosition = server4_pointcloud1_pos;
            }
        }
    }
}
