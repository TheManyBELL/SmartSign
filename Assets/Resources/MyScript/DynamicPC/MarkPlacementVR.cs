using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.Extras;

public class MarkPlacementVR : MonoBehaviour
{

    private MirrorController myController;
    private SymbolMode currentSymbolMode = SymbolMode.ARROW; // default mode is arrow

    public SteamVR_Action_Boolean switchSymbolMode;
    public SteamVR_Action_Boolean confirmSelection;
    public SteamVR_Action_Boolean deleteLastSymbol;

    private List<Vector3> currentPointList = new List<Vector3>();

    private GameObject rightHand;
    
    public GameObject rotateSymbolPrefab;
    public GameObject pressSymbolPrefab;
    private GameObject copySymbol;

    private GlobalUtils globalUtils;

    public GameObject assistPlaceSpherePrefab;
    private GameObject assistPlaceSphere;
    public GameObject assistColliderSpherePrefab;
    private GameObject assistColliderSphere;

    private enum SymbolPRState
    {
        Inactive = 0, SelectPosition, SelectRotation
    }
    private SymbolPRState nowPRState = SymbolPRState.Inactive;

    private void Awake()
    {
        globalUtils.GetComponentInParent<GlobalUtils>();
    }

    // Start is called before the first frame update
    void Start()
    {
        myController = GetComponentInParent<MirrorController>();

        rightHand = GameObject.Find("[CameraRig]/Controller (right)");
        
        assistPlaceSphere = Instantiate(assistPlaceSpherePrefab);
        assistPlaceSphere.layer = LayerMask.NameToLayer("AssitRotateSphere"); ;
        assistPlaceSphere.SetActive(false);

        assistColliderSphere = Instantiate(assistColliderSpherePrefab);
        assistColliderSphere.layer = LayerMask.NameToLayer("VRCameraUnvisible"); ;
        assistColliderSphere.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (switchSymbolMode.GetStateDown(SteamVR_Input_Sources.RightHand))
        {
            SwitchSymbolMode();
        }
        if (currentSymbolMode.Equals(SymbolMode.ARROW))
        {
            if (confirmSelection.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                AddArrowPoint();
            }
            if (deleteLastSymbol.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                DeleteLastArrow();
            }
        }
        else if (currentSymbolMode.Equals(SymbolMode.PRESS))
        {
            AddPress();
        }
        else if (currentSymbolMode.Equals(SymbolMode.ROTATE))
        {
            AddRotation();
        }
    }

    /// <summary>
    /// clear environment, and then switch mode
    /// </summary>
    private void SwitchSymbolMode() {
        // clear environment
        if (currentSymbolMode.Equals(SymbolMode.ARROW))
        {
            currentPointList.Clear();
        }

        // switch mode
        int n_symbol = System.Enum.GetNames(typeof(SymbolMode)).Length; // get symbol numbers
        currentSymbolMode = (SymbolMode)(((int)currentSymbolMode+1)% n_symbol);

        
    }

    private void AddArrowPoint() {
        Vector3 newPoint = GetCollisionPoint();
        int currentPointNumber = currentPointList.Count;
        if (currentPointNumber < 2)
        {
            currentPointList.Add(newPoint);
        }
        if (currentPointNumber == 2)
        {
            myController.CmdUpdateSegmentInfo(new SegmentInfo()
            {
                startPoint = currentPointList[0],
                endPoint = currentPointList[1]
            });
            currentPointList.Clear();
        }
    }

    private void DeleteLastArrow() {

        Debug.Log("VR客户端发起删除线段请求");
        myController.CmdDeleteSegmentInfo();
    }

    private Vector3 GetCollisionPoint()
    {
        //TODO
        int MAXSTEP = 200, stepCount = 0;
        float step = 0.01f;
        assistColliderSphere.transform.position = rightHand.transform.position;
        while (globalUtils.GameObjectVisible(assistColliderSphere))
        {
            assistColliderSphere.transform.position += step * rightHand.transform.forward;
            stepCount++;
            if (stepCount > MAXSTEP) break;
        }

        return (assistColliderSphere.transform.position - step * rightHand.transform.forward);
    }

    private void AddRotation()
    {
        nowPRState = SymbolPRState.SelectPosition;

        Ray ray = new Ray(rightHand.transform.position, rightHand.transform.forward);
        RaycastHit hitInfo;
        int assitSphereLayer = LayerMask.NameToLayer("AssitRotateSphere");
        int onlyCastAssitSphere = 1 << (assitSphereLayer);

        // fisrt select assist sphere position
        if (nowPRState.Equals(SymbolPRState.SelectPosition))
        {
            if (confirmSelection.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                assistPlaceSphere.transform.position = GetCollisionPoint();
                assistPlaceSphere.GetComponent<MeshRenderer>().enabled = true;

                nowPRState = SymbolPRState.SelectRotation;
            }
        }

        // second select symbol rotation on surface, and confirm
        if (nowPRState.Equals(SymbolPRState.SelectRotation))
        {
            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, onlyCastAssitSphere))
            {
                copySymbol.transform.position = hitInfo.point;
                copySymbol.transform.forward = hitInfo.normal;
                if (confirmSelection.GetStateDown(SteamVR_Input_Sources.RightHand))
                {
                    assistPlaceSphere.SetActive(false);
                    nowPRState = SymbolPRState.Inactive;
                    myController.CmdUpdateRotationInfo(new SymbolInfo()
                    {
                        up = hitInfo.normal,
                        position = copySymbol.transform.position
                    });
                }
            }
        }

    }

    private void AddPress()
    {
        nowPRState = SymbolPRState.SelectPosition;

        Ray ray = new Ray(rightHand.transform.position, rightHand.transform.forward);
        RaycastHit hitInfo;
        int assitSphereLayer = LayerMask.NameToLayer("AssitRotateSphere");
        int onlyCastAssitSphere = 1 << (assitSphereLayer);

        if (nowPRState.Equals(SymbolPRState.SelectPosition))
        {
            if (confirmSelection.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                assistPlaceSphere.transform.position = GetCollisionPoint();
                assistPlaceSphere.GetComponent<MeshRenderer>().enabled = true;
                nowPRState = SymbolPRState.SelectRotation;
            }
        }
            
        if (nowPRState.Equals(SymbolPRState.SelectRotation))
        {
            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, onlyCastAssitSphere))
            {
                copySymbol.transform.position = hitInfo.point + 0.05f * hitInfo.normal;
                copySymbol.transform.right = hitInfo.normal;
                if (confirmSelection.GetStateDown(SteamVR_Input_Sources.RightHand))
                {
                    assistPlaceSphere.SetActive(false);
                    nowPRState = SymbolPRState.Inactive;
                    myController.CmdUpdatePressInfo(new SymbolInfo()
                    {
                        up = hitInfo.normal,
                        position = copySymbol.transform.position
                    });
                }
            }
        }

    }



}
