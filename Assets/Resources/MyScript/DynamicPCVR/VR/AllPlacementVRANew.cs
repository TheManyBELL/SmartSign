using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.Extras;

public class AllPlacementVRANew : MonoBehaviour
{

    private MirrorControllerA myController;
    private Exp myExp;
    private GlobalUtilsVR globalUtils; // VR工具类，用于深度碰撞
    private MiddleFactoryVRA middleFactory;

    public SymbolMode currentSymbolMode = SymbolMode.ARROW; // default mode is arrow

    public SteamVR_Action_Boolean switchSymbolMode;
    public SteamVR_Action_Boolean confirmSelection;
    public SteamVR_Action_Boolean deleteLastSymbol;
    public SteamVR_Action_Boolean confirmDependency;
    private GameObject rightHand;

    public GameObject visiblePointprefab;
    // 辅助线段选点
    private List<Vector3> linePoints;
    private List<GameObject> linePointsVisble;
    private List<KeyValuePair<int, Depend.DependType>> lineDepend;
    private Ray ray;
    // 辅助分割选点
    private List<GameObject> splitPointsVisble;
    private List<Vector3> splitPoints;
    private List<List<Vector3>> vertices;
    private List<List<Color>> color;
    private Vector3 center;
    private GameObject splitObject;
    // 辅助轴
    public GameObject AxesPrefab;
    private GameObject initialAxesObject;
    private GameObject FinalAxesObject;
    // 提示信息

    public bool autoGenerateLine;

    private enum SymbolPRState
    {
        Inactive = 0, SelectPosition, SelectRotation
    }
    private SymbolPRState nowPRState = SymbolPRState.Inactive;

    private enum SplitState
    {
        SelectSplitpoint = 0, SelectPosition, ManipulateSplitObj
    }
    private SplitState nowSplitState = SplitState.SelectSplitpoint;

    private enum AxesState
    {
        SelectAxesPosition = 0, ManipulateAxes, SelectAnotherAxesPosition, ManipulateAnotherAxes
    }
    private AxesState nowAxesState = AxesState.SelectAxesPosition;

    public Vector3 oralEnd;

    // Start is called before the first frame update
    void Start()
    {
        myController = GetComponentInParent<MirrorControllerA>();
        myExp = GetComponent<Exp>();
        globalUtils = GetComponent<GlobalUtilsVR>();
        middleFactory = GetComponent<MiddleFactoryVRA>();

        rightHand = GameObject.Find("[CameraRig]/Controller (right)");

        linePoints = new List<Vector3>();
        linePointsVisble = new List<GameObject>();
        lineDepend = new List<KeyValuePair<int, Depend.DependType>>();
        splitPointsVisble = new List<GameObject>();
        splitPoints = new List<Vector3>();
        vertices = new List<List<Vector3>>();
        color = new List<List<Color>>();
    }

    // Update is called once per frame
    void Update()
    {
        if (switchSymbolMode.GetStateDown(SteamVR_Input_Sources.LeftHand))      // VR端开始画标识, AR端结束, 左手扳机键
        {
            myExp.VRBeginAREnd();

            if (GameObject.Find("PointCloud(Clone)/TCPserver2/PointCloud_0"))
            {
                GameObject.Find("PointCloud(Clone)/TCPserver2/PointCloud_0").GetComponent<DisplayPointCloud>().isRenderFrame = true;
            }
        }

        if (!myExp.GetVRExpState()) return;

        if (switchSymbolMode.GetStateDown(SteamVR_Input_Sources.RightHand))     // 切换 画线<->分割物体 右手扳机键
        {
            // Debug.Log("switch symbol mode: " + currentSymbolMode);
            SwitchSymbolMode();
        }

        /*if (myExp.exp_type == Exp.ExpType.CG)
        {
            currentSymbolMode = SymbolMode.Axes;
        }
        if (myExp.exp_type == Exp.ExpType.EG1)
        {
            currentSymbolMode = SymbolMode.SPLIT;
        }
        if (myExp.exp_type == Exp.ExpType.EG2)
        {
            currentSymbolMode = SymbolMode.Oral;
        }
*/
        if (currentSymbolMode.Equals(SymbolMode.ARROW))
        {
            if (confirmSelection.GetStateDown(SteamVR_Input_Sources.RightHand))     // 右手A键
            {
                Debug.Log("press the select button");
                AddArrowPoint();
            }
            
            if (deleteLastSymbol.GetStateDown(SteamVR_Input_Sources.RightHand))     // 右手B键
            {
                Debug.Log("press the delete button");
                DeleteLastArrow();
            }

            if (confirmDependency.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                AddDependency();
            }
        }
        else if (currentSymbolMode.Equals(SymbolMode.SPLIT))
        {
            if (confirmSelection.GetStateDown(SteamVR_Input_Sources.RightHand))     // 右手A键
            {
                if (nowSplitState == SplitState.SelectSplitpoint)   // 选点
                {
                    AddSplitPoint();
                }
                else if (nowSplitState == SplitState.SelectPosition)  // 选初始位置
                {
                    SelectSplitPosition();
                }
                else   // 停止操作并发送给AR端
                {
                    ConfirmSyncSplit();
                }
            }
            if (confirmSelection.GetStateDown(SteamVR_Input_Sources.LeftHand))  // 选点结束 左手A键
            {
                if (splitPoints.Count >= 3) ConfirmSplit();
            }
            if (deleteLastSymbol.GetStateDown(SteamVR_Input_Sources.RightHand)) // 删除上一个split的物体  右手B键
            {
                DeleteLastSplit();
            }
        }
        else if (currentSymbolMode.Equals(SymbolMode.Axes))
        {
            if (confirmSelection.GetStateDown(SteamVR_Input_Sources.RightHand))     // 右手A键
            {
                if (nowAxesState == AxesState.SelectAxesPosition)   // 选点
                {
                    AddAxes();
                }
                else if (nowAxesState == AxesState.ManipulateAxes)
                {
                    globalUtils.RestManipulateObj();
                    nowAxesState = AxesState.SelectAnotherAxesPosition;
                }
                else if (nowAxesState == AxesState.SelectAnotherAxesPosition)
                {
                    AddAnotherAxes();
                }
                else
                {
                    ConfirmSyncAxes();
                }
            }
            if (deleteLastSymbol.GetStateDown(SteamVR_Input_Sources.RightHand)) // 删除上一个Axes  右手B键
            {
                DeleteAxes();
            }
        }
        else if (currentSymbolMode.Equals(SymbolMode.Oral))
        {
            if (confirmSelection.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                // myExp.RecordObjInitRot();
                myExp.RecordObjEndRot(oralEnd);
                // if (middleFactory.type == MiddleFactoryVRA.Type.同步) 
                myExp.VREndARBegin();
            }
        }
    }

    /// <summary>
    /// clear environment, and then switch mode
    /// </summary>
    private void SwitchSymbolMode()
    {
        // 操作过程中切换模式 clear environment
        if (currentSymbolMode.Equals(SymbolMode.ARROW))
        {
            linePoints.Clear();
            for (int i = 0; i < linePointsVisble.Count; i++)
            {
                Destroy(linePointsVisble[i]);
            }
            linePointsVisble.Clear();
        }

        if (currentSymbolMode.Equals(SymbolMode.SPLIT))
        {
            splitPoints.Clear();
            for (int i = 0; i < splitPointsVisble.Count; i++)
            {
                DestroyGameObject(splitPointsVisble[i]);
            }
            splitPointsVisble.Clear();
        }

        nowPRState = SymbolPRState.Inactive;
        nowSplitState = SplitState.SelectSplitpoint;
        nowAxesState = AxesState.SelectAxesPosition;

        // switch mode
        int n_symbol = System.Enum.GetNames(typeof(SymbolMode)).Length; // get symbol numbers
        currentSymbolMode = (SymbolMode)(((int)currentSymbolMode + 1) % n_symbol);

        if (myExp.condition == Exp.Condition.CG2 && currentSymbolMode.Equals(SymbolMode.SPLIT))
        {
            currentSymbolMode = SymbolMode.Axes;
        }
        if (myExp.condition == Exp.Condition.EG && currentSymbolMode.Equals(SymbolMode.Axes))
        {
            currentSymbolMode = SymbolMode.SPLIT;
        }
    }

    private void AddDependency()
    {
        if (linePoints.Count == 0) return;
        Vector3 collision = globalUtils.GetCollisionPoint();

        for (int i = 0; i < middleFactory.cue_list.Count; ++i)
        {
            var cue = middleFactory.cue_list[i];
            if (!cue.IsLine()) continue;
            LineCue line = (LineCue)cue;

            float distance = Vector3.Distance(collision, line.GetStartPoint());
            if (distance < middleFactory.depend_sphere_prefab.transform.localScale.x)
            {
                Depend.DependType d = (linePoints.Count == 1) ? 
                    Depend.DependType.start_start :
                    Depend.DependType.end_start;
                lineDepend.Add(new KeyValuePair<int, Depend.DependType>(i, d));

                Debug.LogWarning("add dependence success 1");
                return;
            }

            distance = Vector3.Distance(collision, line.GetEndPoint());
            if (distance < middleFactory.depend_sphere_prefab.transform.localScale.x)
            {
                Depend.DependType d = (linePoints.Count == 1) ?
                    Depend.DependType.start_end :
                    Depend.DependType.end_end;
                lineDepend.Add(new KeyValuePair<int, Depend.DependType>(i, d));
            }
        }
        Debug.LogWarning("add dependence success 2");
    }

    private void AddArrowPoint()
    {
        Vector3 newPoint = globalUtils.GetCollisionPoint();
        // Debug.Log("select point is:" + newPoint.ToString());

        int currentPointNumber = linePoints.Count;
        if (currentPointNumber < 2)
        {
            if (currentPointNumber == 0) ray = new Ray(rightHand.transform.position, rightHand.transform.forward);
            GameObject pointobj = Instantiate(visiblePointprefab);
            pointobj.transform.position = newPoint;
            pointobj.layer = LayerMask.NameToLayer("DepthCameraUnivisible");
            linePointsVisble.Add(pointobj);
            // Debug.Log("current point number is:" + currentPointNumber + ", add new point");
            linePoints.Add(newPoint);
        }
        else if (currentPointNumber == 2)
        {
            // if (middleFactory.type == MiddleFactoryVRA.Type.同步) 
            myExp.VREndARBegin();
            middleFactory.AddLine(linePoints[0], linePoints[1]);
            middleFactory.RecordStartPointRay(ray);
            foreach (var d in lineDepend)
            {
                middleFactory.AddDepend(d.Key, d.Value);
            }

            // 清空临时变量
            linePoints.Clear();
            for (int i = 0; i < linePointsVisble.Count; i++)
            {
                Destroy(linePointsVisble[i]);
            }
            linePointsVisble.Clear();
            lineDepend.Clear();
        }
    }

    private void DeleteLastArrow()
    {
        if (linePoints.Count > 0)
        {
            linePoints.Clear();
            Destroy(linePointsVisble[0]);
            linePointsVisble.Clear();
            lineDepend.Clear();
        } 
        else
        {
            middleFactory.RemoveSmartCue();
        }
    }

    // ======================================= Split ========================================
    private void AddSplitPoint()
    {
        Vector3 p = globalUtils.GetCollisionPoint();
        splitPoints.Add(p);

        GameObject t = Instantiate(visiblePointprefab);
        t.transform.position = p;
        splitPointsVisble.Add(t);
    }

    private void ConfirmSplit()
    {
        splitPoints.Add(splitPoints[0]);
        splitObject = GetComponent<SplitVRA>().SplitCPU(splitPoints, ref center, ref vertices, ref color);

        myExp.RecordObjInitRot(splitObject.transform.eulerAngles);

        splitPoints.Clear();
        foreach (GameObject g in splitPointsVisble) Destroy(g);
        splitPointsVisble.Clear();

        nowSplitState = SplitState.SelectPosition;
    }

    private void SelectSplitPosition()
    {
        Vector3 p = globalUtils.GetCollisionPoint();

        splitObject.transform.position = p;
        globalUtils.SetManipulateObj(splitObject);

        nowSplitState = SplitState.ManipulateSplitObj;
    }

    private void DeleteLastSplit()
    {
        // SelectSplitpoint = 0, SelectPosition, ManipulateSplitObj
        SplitState preState = nowSplitState;
        nowSplitState = SplitState.SelectSplitpoint;

        if (preState == SplitState.SelectSplitpoint && splitPoints.Count == 0)
        {
            middleFactory.RemoveSmartCue();
            return;
        }

        /*  private List<GameObject> splitPointsVisble;
            private List<Vector3> splitPoints;
            private List<List<Vector3>> vertices;
            private List<List<Color>> color;
            private Vector3 center;
            private GameObject splitObject;*/
        splitPoints.Clear();
        foreach (GameObject g in splitPointsVisble) Destroy(g);
        splitPointsVisble.Clear();
        vertices.Clear();
        color.Clear();
        DestroyGameObject(splitObject);
        globalUtils.RestManipulateObj();
    }

    private void ConfirmSyncSplit()
    {
        myExp.RecordObjEndRot(splitObject.transform.eulerAngles);
        // if (middleFactory.type == MiddleFactoryVRA.Type.同步) 
        myExp.VREndARBegin();

        middleFactory.AddSplitCut(center, vertices, color, splitObject);
        vertices.Clear();
        color.Clear();
        /*        if (autoGenerateLine)
                {
                    myController.CmdAddDPCArrow(new DPCArrow()
                    {
                        index = myController.syncArrowList.Count,
                        startPoint = myController.syncSplitMeshList[i].center,
                        endPoint = splitObject[i].transform.position,
                        curvePointList = new List<Vector3[]>(),
                        originPointList = new List<Vector3[]>(),
                    });
                }*/

        globalUtils.RestManipulateObj();
        nowSplitState = SplitState.SelectSplitpoint;
    }

    // ======================================= Axes ========================================
    private void AddAxes()
    {
        Vector3 p = globalUtils.GetCollisionPoint();
        GameObject Axes = Instantiate(AxesPrefab);
        Axes.transform.position = p;
        initialAxesObject = Axes;
        FinalAxesObject = null;

        globalUtils.SetManipulateObj(Axes);
        nowAxesState = AxesState.ManipulateAxes;
    }

    private void AddAnotherAxes()
    {
        Vector3 p = globalUtils.GetCollisionPoint();
        GameObject Axes = Instantiate(AxesPrefab);
        Axes.transform.position = p;
        FinalAxesObject = Axes;

        globalUtils.SetManipulateObj(Axes);
        nowAxesState = AxesState.ManipulateAnotherAxes;
    }

    private void ConfirmSyncAxes()
    {
        myExp.RecordObjEndRot(FinalAxesObject.transform.eulerAngles);
        // if (middleFactory.type == MiddleFactoryVRA.Type.同步) 
        myExp.VREndARBegin();

        middleFactory.AddAxes(initialAxesObject, FinalAxesObject);

        /*if (autoGenerateLine)
        {
            myController.CmdAddDPCArrow(new DPCArrow()
            {
                index = myController.syncArrowList.Count,
                startPoint = initialAxesObject.transform.position,
                endPoint = FinalAxesObject.transform.position,
                curvePointList = new List<Vector3[]>(),
                originPointList = new List<Vector3[]>(),
            });
        }*/

        globalUtils.RestManipulateObj();
        nowAxesState = AxesState.SelectAxesPosition;
    }

    private void DeleteAxes()
    {
        AxesState preState = nowAxesState;
        nowAxesState = AxesState.SelectAxesPosition;
        // SelectAxesPosition = 0, ManipulateAxes, SelectAnotherAxesPosition, ManipulateAnotherAxes
        if (preState == AxesState.SelectAxesPosition)
        {
            middleFactory.RemoveSmartCue();
            return;
        }

        // if (nowAxesState == AxesState.SelectAxesPosition)

        globalUtils.RestManipulateObj();
        DestroyGameObject(initialAxesObject);
        DestroyGameObject(FinalAxesObject);

        /*int lineIndex = myController.syncAxesList[myController.syncAxesList.Count - 1].correspondingLineIndex;
        if (lineIndex != -1) myController.CmdDeleteDPCArrow(lineIndex);
        myController.CmdDeleteDPCAxes();*/
    }

    private void DestroyGameObject(GameObject t)
    {
        if (!t) return;

        int j = 0;
        while (j < t.transform.childCount)
        {
            Destroy(t.transform.GetChild(j++).gameObject);
        }
        Destroy(t);
    }

}
