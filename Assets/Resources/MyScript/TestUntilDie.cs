using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class TestUntilDie : MonoBehaviour
{
    public Material lineMaterial;
    public float lineThickness = 0.01f;
    private GameObject visibleRay;

    private List<GameObject> symbolList;

    public GameObject RotateSymbol;
    public GameObject PressSymbol;
    private GameObject symbol;
    private GameObject copySymbol;
    public GameObject AssitRotateSphere;

    private TestGlobalUtils globalUtils;

    private int currentLineIndex = 0;

    private enum State
    {
        Inactive = 0, SelectPosition, SelectRotation, SelectP1, SelectP2
    };
    private State nowState = 0;

    public Button PlaceRotButton, PlacePressButton, LineButton;

    // new 
    public DepthDPC GetDepthScript;
    private Camera depthCamera;
    private List<Vector3> symbolOriginPos;
    private int symbolLayer;
    private bool press;
    public GameObject sphere;
    public bool raise = true;

    private Vector3 p1, p2;

    public GameObject test;
    public bool draw;

    public GameObject assistColliderSpherePrefab;
    public GameObject assistColliderSphere;

    // Start is called before the first frame update
    void Start()
    {
        // visibleRay = CreateNewLine("CastRay");
        symbolList = new List<GameObject>();
        AssitRotateSphere.SetActive(false);

        PlaceRotButton = GameObject.Find("TestObj/Canvas/Rotate").GetComponent<Button>();
        PlaceRotButton.onClick.AddListener(ActivateRotPlacement);

        PlacePressButton = GameObject.Find("TestObj/Canvas/Press").GetComponent<Button>();
        PlacePressButton.onClick.AddListener(ActivatePressPlacement);

        LineButton = GameObject.Find("TestObj/Canvas/Line").GetComponent<Button>();
        LineButton.onClick.AddListener(ActivateLine);

        if (GameObject.Find("DepthCamera"))
        {
            depthCamera = GameObject.Find("DepthCamera").GetComponent<Camera>();
            GetDepthScript = GameObject.Find("DepthCamera").GetComponent<DepthDPC>();
        }

        symbolOriginPos = new List<Vector3>();
        symbolLayer = LayerMask.NameToLayer("Symbol");

        draw = false;

        assistColliderSphere = Instantiate(assistColliderSpherePrefab);
        assistColliderSphere.layer = LayerMask.NameToLayer("DepthCameraUnivisible"); ;
        assistColliderSphere.SetActive(false);

        globalUtils = GameObject.Find("Script").GetComponent<TestGlobalUtils>();
    }

    // Update is called once per frame
    void Update()
    {
        /*if (draw)
        {
            GameObject.Find("TestObj").GetComponent<RayDisocclusion>().segments.Add(new SegmentInfo()
            {
                startPoint = new Vector3(-2.7f, 0.2f, 3.0f),
                endPoint = new Vector3(-1.7f, 0.6f, 3.2f)
            });
            draw = false;
        }*/

        Debug.Log("test");
        for (int i = 0; i < symbolList.Count; i++)
        {
            symbolList[i].transform.position = symbolOriginPos[i];
            Debug.Log(symbolList[i].transform.position.ToString("f4"));
        }
        for (int i = 0; i < symbolList.Count; i++)
        {
            if (raise)
                RaiseSymbol(symbolList[i]);
        }

        if (nowState == State.Inactive)
        {
            return;
        }

        Ray ray = GameObject.Find("DepthCamera").GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        int assitSphereLayer = LayerMask.NameToLayer("AssitRotateSphere");
        int onlyCastAssitSphere = 1 << (assitSphereLayer);
        int ignoreAssotSphere = ~onlyCastAssitSphere;

        if (nowState == State.SelectPosition)
        {

            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, ignoreAssotSphere))
            {

                AssitRotateSphere.transform.position = hitInfo.point;
                AssitRotateSphere.GetComponent<MeshRenderer>().enabled = true;
                if (Input.GetMouseButtonDown(0))
                {
                    nowState = State.SelectRotation;
                    copySymbol = GameObject.Instantiate(symbol);
                }
            }
        }

        else if (nowState == State.SelectRotation)
        {

            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, onlyCastAssitSphere))
            {
                copySymbol.transform.position = hitInfo.point;
                if (!press)
                {
                    copySymbol.transform.forward = hitInfo.normal;
                }
                else
                {
                    copySymbol.transform.position += 0.05f * hitInfo.normal;
                    copySymbol.transform.right = hitInfo.normal;
                }
                if (Input.GetMouseButtonDown(0))
                {
                    symbolList.Add(copySymbol);
                    nowState = State.Inactive;
                    AssitRotateSphere.SetActive(false);
                    // new 
                    symbolOriginPos.Add(copySymbol.transform.position);
                }
            }
        }
        // line 
        else if (nowState == State.SelectP1)
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
            if (true)
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
                    GameObject.Find("Script").GetComponent<TestMirror>().syncArrowList.Add(new DPCArrow()
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
        while (globalUtils.GameObjectVisible(assistColliderSphere))
        {
            assistColliderSphere.transform.position += step * ray.direction;
            stepCount++;
            if (stepCount > MAXSTEP) break;
        }

        return (assistColliderSphere.transform.position - 2 * step * ray.direction);
    }

    private void ActivateRotPlacement()
    {
        Debug.Log("rot");
        nowState = State.SelectPosition;
        AssitRotateSphere.SetActive(true);
        symbol = RotateSymbol;
        press = false;
    }

    private void ActivatePressPlacement()
    {
        Debug.Log("press");
        nowState = State.SelectPosition;
        AssitRotateSphere.SetActive(true);
        symbol = PressSymbol;
        press = true;
    }

    private void ActivateLine()
    {
        Debug.Log("line");
        nowState = State.SelectP1;
    }

    private GameObject CreateNewLine(string objName)
    {
        GameObject lineObj = new GameObject(objName);
        lineObj.transform.SetParent(this.transform);
        LineRenderer lineRender = lineObj.AddComponent<LineRenderer>();
        lineRender.material = lineMaterial;

        lineRender.startWidth = lineThickness;
        lineRender.endWidth = lineThickness;
        return lineObj;
    }

    // new 
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
        float minDepth = GetDepthScript.depthTextureRead.GetPixel((int)screenP.x, (int)screenP.y).r;
        return minDepth > screenP.z;
    }

    private void GetBoundsScreenBorder(ref GameObject t, ref Vector2 rightTop, ref Vector2 leftBottom)
    {
        var tAABB = t.GetComponent<MeshRenderer>().bounds;
        float x = tAABB.extents.x,
            y = tAABB.extents.y,
            z = tAABB.extents.z;
        Vector3[] vAABB = new Vector3[]{
            tAABB.center + new Vector3( x,  y,  z),
            tAABB.center + new Vector3( x,  y, -z),
            tAABB.center + new Vector3( x, -y,  z),
            tAABB.center + new Vector3( x, -y, -z),
            tAABB.center + new Vector3(-x,  y,  z),
            tAABB.center + new Vector3(-x,  y, -z),
            tAABB.center + new Vector3(-x, -y,  z),
            tAABB.center + new Vector3(-x, -y, -z)
        };
        

        rightTop = depthCamera.WorldToScreenPoint(vAABB[0]);
        leftBottom = depthCamera.WorldToScreenPoint(vAABB[0]);
        foreach (var v in vAABB)
        {
            Vector2 screenV = depthCamera.WorldToScreenPoint(v);
            rightTop.x = Math.Max(rightTop.x, screenV.x);
            rightTop.y = Math.Max(rightTop.y, screenV.y);
            leftBottom.x = Math.Min(leftBottom.x, screenV.x);
            leftBottom.y = Math.Min(leftBottom.y, screenV.y);
        }

        rightTop.x = Math.Max(rightTop.x, 0);
        rightTop.y = Math.Max(rightTop.y, 0);
        rightTop.x = Math.Min(rightTop.x, Screen.width);
        rightTop.y = Math.Min(rightTop.y, Screen.height);

        leftBottom.x = Math.Max(leftBottom.x, 0);
        leftBottom.y = Math.Max(leftBottom.y, 0);
        leftBottom.x = Math.Min(leftBottom.x, Screen.width);
        leftBottom.y = Math.Min(leftBottom.y, Screen.height);
    }

    private bool GameObjectVisible(GameObject t)
    {
        Bounds tAABB;
        var child = t.transform.GetChild(0).gameObject;
        if (child != null)
        {
            tAABB = child.GetComponent<MeshRenderer>().bounds;
        }
        else
        {
            tAABB = t.GetComponent<MeshRenderer>().bounds;
        }
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

    private void RaiseSymbol(GameObject t)
    {
        float step = 0.1f;

        Debug.Log(GameObjectVisible(t));
        while (!GameObjectVisible(t))
        {
            t.transform.position += step * depthCamera.transform.up;
            // step *= 2;
            // Debug.Log(step);
        }
    }
}
