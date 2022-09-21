using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.Extras;

public class AllPlacementVRA : MonoBehaviour
{

    private MirrorControllerA myController;

    private SymbolMode currentSymbolMode = SymbolMode.ARROW; // default mode is arrow

    public SteamVR_Action_Boolean switchSymbolMode;
    public SteamVR_Action_Boolean confirmSelection;
    public SteamVR_Action_Boolean deleteLastSymbol;

    private List<Vector3> currentPointList = new List<Vector3>();

    private GameObject rightHand;

    public GameObject rotateSymbolPrefab;
    public GameObject pressSymbolPrefab;
    private GameObject rotateSymbol;
    private GameObject pressSymbol;

    private GlobalUtilsVR globalUtils; // VR工具类，用于深度碰撞

    public GameObject assistPlaceSpherePrefab;
    private GameObject assistPlaceSphere;

    // 辅助线段选点
    public GameObject drawpointprefab;
    public List<GameObject> drawpointList;
    // 辅助分割选点
    private List<GameObject> splitPointVisble;
    private List<Vector3> splitPoints;
    private List<GameObject> splitObjects;

    private enum SymbolPRState
    {
        Inactive = 0, SelectPosition, SelectRotation
    }
    private SymbolPRState nowPRState = SymbolPRState.Inactive;

    private enum SplitState
    {
        SelectSplitpoint = 0, ManipulateSplitObj
    }
    private SplitState nowSplitState = SplitState.SelectSplitpoint;

    private void Awake()
    {
        globalUtils = GetComponent<GlobalUtilsVR>();
    }

    // Start is called before the first frame update
    void Start()
    {
        myController = GetComponentInParent<MirrorControllerA>();

        rightHand = GameObject.Find("[CameraRig]/Controller (right)");

        assistPlaceSphere = Instantiate(assistPlaceSpherePrefab);
        assistPlaceSphere.layer = LayerMask.NameToLayer("AssitRotateSphere"); ;
        assistPlaceSphere.SetActive(false);

        rotateSymbol = Instantiate(rotateSymbolPrefab);
        rotateSymbol.SetActive(false);

        pressSymbol = Instantiate(pressSymbolPrefab);
        pressSymbol.SetActive(false);

        drawpointList = new List<GameObject>();
        splitPointVisble = new List<GameObject>();
        splitPoints = new List<Vector3>();
        splitObjects = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (switchSymbolMode.GetStateDown(SteamVR_Input_Sources.RightHand))
        {
            SwitchSymbolMode();
            Debug.Log("switch symbol mode: " + currentSymbolMode);
        }
        if (currentSymbolMode.Equals(SymbolMode.ARROW))
        {
            if (confirmSelection.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                Debug.Log("press the select button");
                AddArrowPoint();
            }
            if (deleteLastSymbol.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                Debug.Log("press the delete button");
                DeleteLastArrow();
            }
        }
        else if (currentSymbolMode.Equals(SymbolMode.SPLIT))
        {
            if (confirmSelection.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                if (nowSplitState == SplitState.SelectSplitpoint) AddSplit();
                else
                {
                    globalUtils.RestManipulateObj();
                    nowSplitState = SplitState.SelectSplitpoint;
                }
            }
            if (deleteLastSymbol.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                DeleteLastSplit();
            }
        }
        else if (currentSymbolMode.Equals(SymbolMode.PRESS))
        {
            // AddPress();
        }
        else if (currentSymbolMode.Equals(SymbolMode.ROTATE))
        {
            // AddRotation();
        }
    }

    /// <summary>
    /// clear environment, and then switch mode
    /// </summary>
    private void SwitchSymbolMode()
    {
        // clear environment
        if (currentSymbolMode.Equals(SymbolMode.ARROW))
        {
            currentPointList.Clear();
            for (int i = 0; i < drawpointList.Count; i++)
            {
                Destroy(drawpointList[i]);
            }
            drawpointList.Clear();
        }
        nowPRState = SymbolPRState.Inactive;

        // switch mode
        int n_symbol = System.Enum.GetNames(typeof(SymbolMode)).Length; // get symbol numbers
        currentSymbolMode = (SymbolMode)(((int)currentSymbolMode + 1) % n_symbol);
    }

    private void AddArrowPoint()
    {
        Vector3 newPoint = globalUtils.GetCollisionPoint();
        Debug.Log("select point is:" + newPoint.ToString());

        int currentPointNumber = currentPointList.Count;
        if (currentPointNumber < 2)
        {
            GameObject pointobj = Instantiate(drawpointprefab);
            pointobj.transform.position = newPoint;
            pointobj.layer = LayerMask.NameToLayer("DepthCameraUnivisible");
            drawpointList.Add(pointobj);
            Debug.Log("current point number is:" + currentPointNumber + ", add new point");
            currentPointList.Add(newPoint);

        }
        if (currentPointNumber == 2)
        {
            Debug.Log("current point number is:" + currentPointNumber + ", update segment");
            myController.CmdAddDPCArrow(new DPCArrow()
            {
                index = myController.syncArrowList.Count,
                startPoint = currentPointList[0],
                endPoint = currentPointList[1],
                curvePointList = new List<Vector3[]>(),
            });
            // 清空临时变量
            currentPointList.Clear();
            for (int i = 0; i < drawpointList.Count; i++)
            {
                Destroy(drawpointList[i]);
            }
            drawpointList.Clear();
        }
    }

    private void DeleteLastArrow()
    {
        myController.CmdDeleteDPCArrow();
    }

    private void AddSplit()
    {
        
        Vector3 p = globalUtils.GetCollisionPoint();
        splitPoints.Add(p);

        GameObject t = Instantiate(drawpointprefab);
        t.transform.position = p;
        splitPointVisble.Add(t);

        float dis = float.MaxValue;
        if (splitPoints.Count > 3)
        {
            dis = Vector3.Distance(splitPoints[0], splitPoints[splitPoints.Count - 1]);
        }

        if (dis < 0.5)
        {
            splitPoints.RemoveAt(splitPoints.Count - 1);
            splitPoints.Add(splitPoints[0]);

            Vector3 center = new Vector3();
            List<List<Vector3>> vertices = new List<List<Vector3>>();
            List<List<Color>> color = new List<List<Color>>();
            
            GameObject fa = GetComponent<SplitVRA>().SplitCPU(splitPoints, ref center, ref vertices, ref color);
            splitObjects.Add(fa);

            myController.CmdAddDPCSplitMesh(new DPCSplitMesh() { 
                index = myController.syncSplitMeshList.Count,
                center = center,
                color = color,
                vertices = vertices,
            });

            myController.CmdAddDPCSplitPos(new DPCSplitPosture() {
                index = myController.syncSplitPosList.Count,
                valid = false,
                position = center,
                rotation = new Quaternion(),
            });
            Debug.Assert(myController.syncSplitMeshList.Count == myController.syncSplitPosList.Count);
            globalUtils.SetManipulateObj(fa);
            nowSplitState = SplitState.ManipulateSplitObj;

            splitPoints.Clear();
            foreach (GameObject g in splitPointVisble) Destroy(g);
            splitPointVisble.Clear();
        }
    }

    private void DeleteLastSplit()
    {
        GameObject father = splitObjects[splitObjects.Count-1];

        int j = 0;
        while (j < father.transform.childCount)
        {
            Destroy(father.transform.GetChild(j++).gameObject);
        }

        Destroy(father);
        splitObjects.RemoveAt(splitObjects.Count - 1);

        myController.CmdDeleteDPCSplitMesh();
        myController.CmdDeleteDPCSplitPos();
        Debug.Assert(myController.syncSplitMeshList.Count == myController.syncSplitPosList.Count);

        globalUtils.RestManipulateObj();
    }

    private void AddRotation()
    {
        if (nowPRState == SymbolPRState.Inactive)
        {
            nowPRState = SymbolPRState.SelectPosition;
        }

        Ray ray = new Ray(rightHand.transform.position, rightHand.transform.forward);
        RaycastHit hitInfo;
        int assitSphereLayer = LayerMask.NameToLayer("AssitRotateSphere");
        int onlyCastAssitSphere = 1 << (assitSphereLayer);

        // fisrt select assist sphere position
        if (nowPRState == SymbolPRState.SelectPosition)
        {
            if (confirmSelection.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                Debug.Log("Rotation symbol, select assist sphere position");
                assistPlaceSphere.SetActive(true);
                assistPlaceSphere.transform.position = globalUtils.GetCollisionPoint();
                assistPlaceSphere.GetComponent<MeshRenderer>().enabled = true;

                nowPRState = SymbolPRState.SelectRotation;
                rotateSymbol.SetActive(true);
            }
        }

        // second select symbol rotation on surface, and confirm
        else if (nowPRState.Equals(SymbolPRState.SelectRotation))
        {
            Debug.Log("state is select rotation");
            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, onlyCastAssitSphere))
            {
                Debug.Log("ray hit");
                rotateSymbol.transform.position = hitInfo.point;
                rotateSymbol.transform.forward = hitInfo.normal;
                if (confirmSelection.GetStateDown(SteamVR_Input_Sources.RightHand))
                {
                    Debug.Log("Rotation symbol, select symbol position");
                    assistPlaceSphere.SetActive(false);
                    nowPRState = SymbolPRState.Inactive;
                    myController.CmdAddDPCRotation(new DPCSymbol()
                    {
                        index = myController.syncRotationList.Count,
                        up = hitInfo.normal,
                        position = rotateSymbol.transform.position,
                        up_new = new Vector3(),
                        position_new = new Vector3()
                    }) ;
                    rotateSymbol.SetActive(false);
                }

            }
        }

    }

    private void AddPress()
    {
        if (nowPRState == SymbolPRState.Inactive)
        {
            nowPRState = SymbolPRState.SelectPosition;
        }

        Ray ray = new Ray(rightHand.transform.position, rightHand.transform.forward);
        RaycastHit hitInfo;
        int assitSphereLayer = LayerMask.NameToLayer("AssitRotateSphere");
        int onlyCastAssitSphere = 1 << (assitSphereLayer);

        if (nowPRState.Equals(SymbolPRState.SelectPosition))
        {
            if (confirmSelection.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                Debug.Log("Press symbol, select assist sphere position");
                assistPlaceSphere.SetActive(true);
                assistPlaceSphere.transform.position = globalUtils.GetCollisionPoint();
                assistPlaceSphere.GetComponent<MeshRenderer>().enabled = true;
                nowPRState = SymbolPRState.SelectRotation;
                pressSymbol.SetActive(true);
            }
        }

        else if (nowPRState.Equals(SymbolPRState.SelectRotation))
        {
            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, onlyCastAssitSphere))
            {

                pressSymbol.transform.position = hitInfo.point + 0.05f * hitInfo.normal;
                pressSymbol.transform.right = hitInfo.normal;
                if (confirmSelection.GetStateDown(SteamVR_Input_Sources.RightHand))
                {
                    Debug.Log("Press symbol, select symbol position");
                    assistPlaceSphere.SetActive(false);
                    nowPRState = SymbolPRState.Inactive;
                    myController.CmdAddDPCPress(new DPCSymbol()
                    {
                        index = myController.syncPressList.Count,
                        up = hitInfo.normal,
                        position = pressSymbol.transform.position,
                        up_new = new Vector3(),
                        position_new = new Vector3()
                    });
                    pressSymbol.SetActive(false);
                }
            }
        }

    }

    

}
