using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.Extras;

public struct MiddleLine
{
    public int index;
    public Vector3 start_point;
    public Vector3 end_point;

    public bool start_depent_start;
    public bool start_depent_end;
    public bool end_depent_start;
    public bool end_depent_end;
    public Vector3 end_depend_end_dir;

    public bool finish;

    public GameObject start_sphere;
    public GameObject end_sphere;
    /*
    public List<int> start_depent_start;
    public List<int> start_depent_end;
    public List<int> end_depent_start;
    public List<int> end_depent_end;
    */

    public MiddleLine(int index, Vector3 start_point, Vector3 end_point, GameObject prefab)
    {
        this.index = index;
        this.start_point = start_point;
        this.end_point = end_point;

        this.start_depent_start = false;
        this.start_depent_end = false;
        this.end_depent_start = false;
        this.end_depent_end = false;
        this.end_depend_end_dir = new Vector3();

        this.finish = false;
 
        this.start_sphere = GameObject.Instantiate(prefab);
        this.end_sphere = GameObject.Instantiate(prefab);
        start_sphere.transform.position = start_point;
        end_sphere.transform.position = end_point;
        start_sphere.layer = LayerMask.NameToLayer("DepthCameraUnivisible");
        end_sphere.layer = LayerMask.NameToLayer("DepthCameraUnivisible");
 
        
        /*this.start_depent_start = new List<int>();
        this.start_depent_end = new List<int>();
        this.end_depent_start = new List<int>();
        this.end_depent_end = new List<int>();*/
    }

    // public void SetFinish(bool finish) => this.finish = finish;

    public void SetSphereState(bool state)
    {
        // Debug.Log("??");
        start_sphere.SetActive(state);
        end_sphere.SetActive(state); 
    }

    public void DeleteThis()
    {
        GameObject.Destroy(start_sphere);
        GameObject.Destroy(end_sphere);
    }
};


public class LineMiddleFactory : MonoBehaviour
{
    private MirrorControllerA my_controller;
    private AsylineCalculate asy_calculate;
    public int current_ar_line_index = 0; // 仅用于异步
    public List<MiddleLine> line_list;

    public enum LineExpType { 同步 = 0, 异步 };
    public LineExpType current_exp_type = LineExpType.同步;
    public bool debug_demo_mode;    // 用于异步debug demo，开启后异步标识不再逐个出现，用于de线起点终点调整的bug
    public bool debug_asynchronous_manual_switch;

    // dependent related
    public float dependency_threshold;
    public GameObject threshold_sphere_prefab;
    public bool visible_dependency_threshold;
    private bool last_visible_state = true;


    public SteamVR_Action_Boolean next_line;
    public SteamVR_Action_Boolean previous_line;

    // Start is called before the first frame update
    void Start()
    {
        line_list = new List<MiddleLine>();
        my_controller = GetComponentInParent<MirrorControllerA>();
        asy_calculate = GetComponent<AsylineCalculate>();

    }

    // Update is called once per frame
    void Update()
    {
        // 同步直接返回
        if (current_exp_type == LineExpType.同步) return;
        AsyTickTODOList();
    }

    void AsyTickTODOList()
    {
        // 可视化更新
        if (last_visible_state != visible_dependency_threshold)
        {
            ChangeVisibleState();
            last_visible_state = visible_dependency_threshold;
        }

        // 判断是否做完，切换下一条直线
        if ((!debug_demo_mode && CurrentLineComplete()) || next_line.GetStateDown(SteamVR_Input_Sources.LeftHand))
        {
            CompleteLine(current_ar_line_index);
            AsynSwitchNextLine();
            AdjustEndpoints();      // 这条线已经切换了，so，adjust
        }
        
        if (previous_line.GetStateDown(SteamVR_Input_Sources.LeftHand) || debug_asynchronous_manual_switch)
        {
            GoBackToPreLine();
            debug_asynchronous_manual_switch = false;
        }
    }

    /*
     * line place 唯一接口
     */
    public void AddNewLine(Vector3 start_point, Vector3 end_point)
    {
        line_list.Add(new MiddleLine(line_list.Count, start_point, end_point, threshold_sphere_prefab));
        line_list[line_list.Count - 1].SetSphereState(visible_dependency_threshold && current_exp_type == LineExpType.异步);

        if (current_exp_type == LineExpType.同步)
        {
            SynSwitchNextLine();   // 删除则靠专家
            return;
        }

        // ar在等待vr新的线
        if (current_ar_line_index == line_list.Count - 1)
        {
            current_ar_line_index--;
            AsynSwitchNextLine();
        }

        DetectDependency();
    }

    public void DeleteLine(int index = -1)
    {
        if (current_exp_type == LineExpType.同步 || (current_exp_type == LineExpType.异步 && current_ar_line_index == line_list.Count-1))
        {
            my_controller.CmdDeleteDPCArrow();
        }
        
        int real_index = index == -1 ? (line_list.Count - 1) : index;
        line_list[real_index].DeleteThis();
        line_list.RemoveAt(real_index);
    }

    public void CompleteLine(int index = -1)
    {
        if (index >= line_list.Count) return;

        my_controller.CmdDeleteDPCArrow();
        int real_index = index == -1 ? (line_list.Count - 1) : index;
        line_list[real_index].SetSphereState(false);

        MiddleLine t = line_list[real_index];
        t.finish = true;
        line_list[real_index] = t;
    }

    /*
     * 仅用于异步
     * 检测当前ar端线是否完成
     */
    bool CurrentLineComplete()
    {
        if (current_ar_line_index < 0 || current_ar_line_index >= line_list.Count) return false;

        bool finish = asy_calculate.CalculateLineComplete(line_list[current_ar_line_index].end_point);
        return finish;
    }

    void GoBackToPreLine()
    {
        if (current_ar_line_index == 0) return;
        if (current_ar_line_index < line_list.Count)
        {
            MiddleLine cur = line_list[current_ar_line_index];
            cur.SetSphereState(visible_dependency_threshold);
            line_list[current_ar_line_index] = cur;

            my_controller.CmdDeleteDPCArrow();
        }

        current_ar_line_index--;

        MiddleLine prev = line_list[current_ar_line_index];
        prev.SetSphereState(visible_dependency_threshold);
        prev.finish = false;
        line_list[current_ar_line_index] = prev;

        my_controller.CmdAddDPCArrow(new DPCArrow()
        {
            index = my_controller.syncArrowList.Count,
            startPoint = line_list[current_ar_line_index].start_point,
            endPoint = line_list[current_ar_line_index].end_point,
            curvePointList = new List<Vector3[]>(),
            originPointList = new List<Vector3[]>(),
        });

    }

    void AdjustEndpoints()
    {
        if (current_ar_line_index < 0 || current_ar_line_index >= line_list.Count) return;

        MiddleLine target = line_list[current_ar_line_index];
        if (target.start_depent_start) target.start_point = asy_calculate.AdjustStartPointDependStart(target.start_point);
        if (target.start_depent_end) target.start_point = asy_calculate.AdjustStartPointDependEnd(target.start_point);
        if (target.end_depent_start) target.end_point = asy_calculate.AdjustEndPointDependStart(target.end_point);
        if (target.end_depent_end) target.end_point = asy_calculate.AdjustEndPointDependEnd(target.end_point);

        // line_list[current_ar_line_index] = target;

        my_controller.CmdUpdateDPCArrow(new DPCArrow()
        {
            index = my_controller.syncArrowList.Count - 1,
            startPoint = target.start_point,
            endPoint = target.end_point,
            curvePointList = new List<Vector3[]>(),
            originPointList = new List<Vector3[]>(),
        });
    }

    void AsynSwitchNextLine()
    {
        if (current_ar_line_index >= line_list.Count) return;
        current_ar_line_index++;
        if (current_ar_line_index >= line_list.Count) return;

        my_controller.CmdAddDPCArrow(new DPCArrow()
        {
            index = my_controller.syncArrowList.Count,
            startPoint = line_list[current_ar_line_index].start_point,
            endPoint = line_list[current_ar_line_index].end_point,
            curvePointList = new List<Vector3[]>(),
            originPointList = new List<Vector3[]>(),
        });
    }

    void SynSwitchNextLine()
    {
        my_controller.CmdAddDPCArrow(new DPCArrow()
        {
            index = my_controller.syncArrowList.Count,
            startPoint = line_list[line_list.Count-1].start_point,
            endPoint = line_list[line_list.Count-1].end_point,
            curvePointList = new List<Vector3[]>(),
            originPointList = new List<Vector3[]>(),
        });
    }

    void DetectDependency()
    {
        MiddleLine current_line = line_list[line_list.Count - 1];
        Vector3 start_point = current_line.start_point;
        Vector3 end_point = current_line.end_point;
        for (int i = 0; i < line_list.Count - 1; ++i)
        {
            MiddleLine t = line_list[i];
            if (t.finish) continue;
            current_line.start_depent_start |= Vector3.Distance(start_point, t.start_point) < dependency_threshold;
            current_line.start_depent_end |= Vector3.Distance(start_point, t.end_point) < dependency_threshold;
            current_line.end_depent_start |= Vector3.Distance(end_point, t.start_point) < dependency_threshold;
            current_line.end_depent_end |= Vector3.Distance(end_point, t.end_point) < dependency_threshold;
        }
        line_list[line_list.Count - 1] = current_line;
    }

    void ChangeVisibleState()
    {
        foreach (var line in line_list)
        {
            line.SetSphereState(visible_dependency_threshold && current_exp_type == LineExpType.异步);
        }
        
    }
}
