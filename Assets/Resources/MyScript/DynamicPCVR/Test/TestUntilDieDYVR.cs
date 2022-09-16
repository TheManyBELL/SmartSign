using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class TestUntilDieDYVR : MonoBehaviour
{
    private Vector3 p1, p2;
    public Button LineButton;
    private enum State
    {
        Inactive = 0, SelectPosition, SelectRotation, SelectP1, SelectP2
    };
    private State nowState = 0;
    private int currentLineIndex = 0;

    public GameObject assistColliderSpherePrefab;
    private GameObject assistColliderSphere;

    public Camera depthCamera;
    private DepthDPC GetDepthScript;

    // Start is called before the first frame update
    void Start()
    {
        LineButton = GameObject.Find("TestObj/Canvas/Line").GetComponent<Button>();
        LineButton.onClick.AddListener(ActivateLine);

        assistColliderSphere = Instantiate(assistColliderSpherePrefab);
        assistColliderSphere.layer = LayerMask.NameToLayer("DepthCameraUnivisible"); ;
        assistColliderSphere.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (GlobleInfo.ClientMode.Equals(CameraMode.AR))
        {
            return;
        }

        if (!depthCamera)
        {
            depthCamera = GameObject.Find("SmartSignA(Clone)/VR/DepthCameraVR").GetComponent<Camera>();
            GetDepthScript = GameObject.Find("SmartSignA(Clone)/VR/DepthCameraVR").GetComponent<DepthDPC>();
        }

        if (nowState == State.Inactive)
        {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        
        // line 
        if (nowState == State.SelectP1)
        {
            // if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    // p1 = hitInfo.point + 0.01f * hitInfo.normal;
                    assistColliderSphere.SetActive(true);
                    p1 = GetCollisionPoint(ray);
                    nowState = State.SelectP2;
                    Debug.Log("p1 " + p1);
                }

            }
        }
        else if (nowState == State.SelectP2)
        {
            // if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    p2 = GetCollisionPoint(ray);
                    assistColliderSphere.SetActive(false);
                    /*GameObject.Find("TestObj").GetComponent<RayDisocclusion>().segments.Add(new SegmentInfo()
                    {
                        startPoint = p1,
                        endPoint = p2
                    });*/
                    GameObject.Find("SmartSignA(Clone)").GetComponent<MirrorControllerA>().CmdAddDPCArrow(new DPCArrow()
                    {
                        index = currentLineIndex++,
                        startPoint = p1,
                        endPoint = p2,
                        curvePointList = new List<Vector3[]>()
                    });
                    nowState = State.Inactive;
                    Debug.Log("p2 " + p2);
                }
            }
        }
    }

    private Vector3 GetCollisionPoint(Ray ray)
    {
        //TODO
        int MAXSTEP = 1000, stepCount = 0;
        float step = 0.01f;
        assistColliderSphere.transform.position = ray.origin;
        while (GameObjectVisible(assistColliderSphere))
        {
            assistColliderSphere.transform.position += step * ray.direction;
            stepCount++;
            if (stepCount > MAXSTEP) break;
        }

        return (assistColliderSphere.transform.position - 2 * step * ray.direction);
    }

    private void ActivateLine()
    {
        Debug.Log("line");
        nowState = State.SelectP1;
    }

    private float GetDepth(int x, int y) => GetDepthScript.GetDepth(x, y);

    private Vector3 MScreenToWorldPointDepth(Vector3 p)
    {
        p.z *= depthCamera.farClipPlane;
        return depthCamera.ScreenToWorldPoint(p);
    }

    private Vector3 MWorldToScreenPointDepth(Vector3 p)
    {
        Vector3 screenP = depthCamera.WorldToScreenPoint(p);
        screenP.z = screenP.z / depthCamera.farClipPlane;
        return screenP;
    }

    private bool GetPointVisibility(Vector3 p)
    {
        Vector3 screenP = MWorldToScreenPointDepth(p);
        if (screenP.x < 0 || screenP.x > Screen.width || screenP.y < 0 || screenP.y > Screen.height)
            return true;
        float minDepth = GetDepthScript.GetDepth((int)screenP.x, (int)screenP.y);

        return minDepth > screenP.z;
    }

    private bool GameObjectVisible(GameObject t)
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
