using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalUtils : MonoBehaviour
{
    public Camera depthCamera;
    private DepthDPC GetDepthScript;

    void Awake()
    {
        
    }

    private void Update()
    {
        if (depthCamera) { return; }
        if (GameObject.Find("DepthCameraAR(Clone)"))
        {
            depthCamera = GameObject.Find("DepthCameraAR(Clone)").GetComponent<Camera>();
            GetDepthScript = GameObject.Find("DepthCameraAR(Clone)").GetComponent<DepthDPC>();
            if (depthCamera)
            {
                Debug.Log("[Global Utils]: Depth Camera found");
            }
        }
        else
        {
            //depthCamera = Camera.main;
            //GetDepthScript = Camera.main.GetComponent<DepthDPC>();
            Debug.Log("[Global Utils]: Depth Camera not found");
        }
    }

    public float GetDepth(int x, int y) => GetDepthScript.GetDepth(x, y);

    public Vector3 MScreenToWorldPointDepth(Vector3 p)
    {
        p.z *= depthCamera.farClipPlane;
        return depthCamera.ScreenToWorldPoint(p);
    }

    public Vector3 MWorldToScreenPointDepth(Vector3 p)
    {
        Vector3 screenP = depthCamera.WorldToScreenPoint(p);
        screenP.z = screenP.z / depthCamera.farClipPlane;
        return screenP;
    }

    public bool GetPointVisibility(Vector3 p)
    {
        Vector3 screenP = MWorldToScreenPointDepth(p);
        if (screenP.x < 0 || screenP.x > Screen.width || screenP.y < 0 || screenP.y > Screen.height)
            return true;
        float minDepth = GetDepthScript.GetDepth((int)screenP.x, (int)screenP.y);

        return minDepth > screenP.z;
    }

    public bool GameObjectVisible(GameObject t)
    {
        Bounds tAABB;
        if (t.GetComponentsInChildren<Transform>(true).Length > 1)
        {
            tAABB = t.transform.GetChild(0).GetComponent<MeshRenderer>().bounds;
        }
        else
        {
            tAABB = t.GetComponent<MeshRenderer>().bounds;
        }
        // var tAABB = t.GetComponent<MeshRenderer>().bounds;
        float x = tAABB.extents.x, y = tAABB.extents.y, z = tAABB.extents.z;
        float scale = 0.9f;
        Vector3[] vAABB = new Vector3[]{
            tAABB.center + scale * new Vector3( x,  y,  z),
            tAABB.center + scale * new Vector3( x,  y, -z),
            tAABB.center + scale * new Vector3( x, -y,  z),
            tAABB.center + scale * new Vector3( x, -y, -z),
            tAABB.center + scale * new Vector3(-x,  y,  z),
            tAABB.center + scale * new Vector3(-x,  y, -z),
            tAABB.center + scale * new Vector3(-x, -y,  z),
            tAABB.center + scale * new Vector3(-x, -y, -z)
        };
        foreach (var v in vAABB)
        {
            if (!GetPointVisibility(v))
            {
                return false;
            }
        }
        return true;
    }
}
