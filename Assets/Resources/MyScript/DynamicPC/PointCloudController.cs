using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCloudController : MonoBehaviour
{

    public string hostIP_ar;
    public string hostIP_vr;
    public string hostIP;

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

        gameObject.transform.position = new Vector3(-1.51f, 0.84f, 2.07f);
        gameObject.transform.eulerAngles = new Vector3(58f, 40.69f, 88.6f);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
