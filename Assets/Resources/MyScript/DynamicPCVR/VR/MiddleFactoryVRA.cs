using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.Extras;
using System.Threading;

public class MiddleFactoryVRA : MonoBehaviour
{
    private MirrorControllerA my_controller;
    private AsylineCalculate asy_calculate;
    public List<SmartCue> cue_list;
    public int synchronous_index;

    public enum Type { 同步 = 0, 异步 };
    public Type type = Type.同步;
    public bool debug_demo_mode;    // 用于异步debug demo，开启后异步标识不再逐个出现，用于de线起点终点调整的bug
    public bool debug_asynchronous_manual_switch;
    private bool delayed_update_next = false;    // 异步切换下一个cue时，删除当前和添加下一个不在同一帧，保证AR能正确更新
    private bool delayed_update_previous = false;

    public SteamVR_Action_Boolean next_cue;
    public SteamVR_Action_Boolean previous_cue;

    // dependent related
    public GameObject depend_sphere_prefab;

    private Exp exp;

    // Start is called before the first frame update
    void Start()
    {
        synchronous_index = -1;
        my_controller = GetComponentInParent<MirrorControllerA>();
        asy_calculate = GetComponent<AsylineCalculate>();
        exp = GetComponent<Exp>();
        cue_list = new List<SmartCue>();
    }

    // Update is called once per frame
    void Update()
    {
        if (type == Type.异步)
        {
            AsyTickTODOList();
        }     
    }

    void AsyTickTODOList()
    {
        if (delayed_update_next)
        {
            SynchronizeNextCueStep2();
        }

        if (delayed_update_previous)
        {
            SynchronizePreviousCueStep2();
        }

        // 判断是否做完，切换下一条直线
        if ((!debug_demo_mode && CurrentCueComplete()) || next_cue.GetStateDown(SteamVR_Input_Sources.LeftHand))
        {
            UpdatePointCloud();
            SynchronizeNextCueStep1();    
        }

        if (previous_cue.GetStateDown(SteamVR_Input_Sources.LeftHand))
        {
            UpdatePointCloud();
            SynchronizePreviousCueStep1();
        }
    }

    public void AddDepend(int index, Depend.DependType type)
    {
        LineCue line = (LineCue)cue_list[cue_list.Count - 1];
        line.depend.AddDepend(index, type);
    }

    public void RecordStartPointRay(Ray ray)
    {
        LineCue line = (LineCue)cue_list[cue_list.Count - 1];
        line.ray = ray;
    }

    public void AddLine(Vector3 start_point, Vector3 end_point)
    {
        if (type == Type.异步) cue_list.Add(new LineCue(start_point, end_point, depend_sphere_prefab));
        else cue_list.Add(new LineCue(start_point, end_point));

        SynchronizeCurrentCue();
    }

    public void AddAxes(GameObject initial_axes, GameObject final_axes)
    {
        cue_list.Add(new AxesCue(initial_axes, final_axes));
        SynchronizeCurrentCue();
    }

    public void AddSplitCut(Vector3 center, List<List<Vector3>> vertices, List<List<Color>> color, GameObject split_obj)
    {
        // Debug.LogWarningFormat("color size {0}, vertices size {1}", vertices.Count, color.Count);
        // Debug.LogWarningFormat("color 0 size {0}, vertices 0 size {1}", vertices[0].Count, color[0].Count);

        cue_list.Add(new CutpieceCue(center, vertices, color, split_obj));
        SynchronizeCurrentCue();
    }

    private void SynchronizeCurrentCue()
    {
        if (type == Type.同步)
        {
            cue_list[cue_list.Count - 1].Synchronize(my_controller);
        } 
        else if (synchronous_index == -1)
        {
            cue_list[cue_list.Count - 1].Synchronize(my_controller);
            synchronous_index = cue_list.Count - 1;
        }
    }

    public void SynchronizeUpdate()
    {
        cue_list[cue_list.Count - 1].UpdateSynchronize(my_controller);
    }

    public void RemoveSmartCue()
    {
        if (cue_list.Count <= 0) return;
        SmartCue cue = cue_list[cue_list.Count - 1];
        if (cue.is_synchronous) synchronous_index = -1;
        cue_list[cue_list.Count - 1].Remove(my_controller);
        cue_list.RemoveAt(cue_list.Count - 1);      // 删除以后目前不显示上一个
        // synchronous_index -= 1;
    }

    public void SynchronizeNextCueStep1()   // 停止同步这一个
    {
        if (!exp.asynchronous_ar_begin) // 只为记录AR开始
        {
            exp.asynchronous_ar_begin = true;
            exp.AsynchronousARBegin();
            return;
        }

        if (synchronous_index >= cue_list.Count || synchronous_index == -1) return;
        cue_list[synchronous_index].StopSynchronize(my_controller, false);
        delayed_update_next = true;

        AdjustCue();
    }

    public void SynchronizeNextCueStep2() // 同步下一个
    {
        synchronous_index += 1;     
        if (synchronous_index < cue_list.Count)
        {
            cue_list[synchronous_index].Synchronize(my_controller);
        }
        else
        {
            synchronous_index = -1;
        }
        delayed_update_next = false;
        exp.VRBeginAREnd(); // 记录AR结束
    }

    public void SynchronizePreviousCueStep1()
    {
        if (synchronous_index <= 0) return;
        cue_list[synchronous_index].StopSynchronize(my_controller, true);     // 停止同步这一个
        delayed_update_previous = true;
    }

    public void SynchronizePreviousCueStep2()
    {
        synchronous_index -= 1;     // 同步上一个
        if (synchronous_index >= 0)
        {
            cue_list[synchronous_index].Synchronize(my_controller);
        }
        delayed_update_previous = false;
    }


    bool CurrentCueComplete()
    {
        return false;
    }


    void AdjustCue()
    {
        if (synchronous_index < 0 || synchronous_index >= cue_list.Count) return;
        for (int i = synchronous_index + 1; i < cue_list.Count; ++i)
        {
            if (cue_list[i].type != SmartCue.CueType.Line) continue;

            LineCue line = (LineCue)cue_list[i];
            if (!line.depend.is_depend) continue;

            foreach (var d in line.depend.depend_list)
            {
                if (d.Key != synchronous_index) continue;
                if (d.Value == Depend.DependType.start_start) {
                    Debug.LogWarning("start_start");
                    line.SetStartPoint(asy_calculate.AdjustStartPointDependStart(line.ray));
                }
                else if (d.Value == Depend.DependType.start_end)
                {
                    Debug.LogWarning("start_end");
                    line.SetStartPoint(asy_calculate.AdjustStartPointDependEnd(line.GetStartPoint(), cue_list[synchronous_index]));
                }
                else if (d.Value == Depend.DependType.end_start)
                {
                    Debug.LogWarning("end_start");
                    line.SetEndPoint(asy_calculate.AdjustEndPointDependStart(line.GetEndPoint()));
                }
                else if (d.Value == Depend.DependType.end_end)
                {
                    Debug.LogWarning("end_end");
                    line.SetEndPoint(asy_calculate.AdjustEndPointDependEnd(line.GetEndPoint(), cue_list[synchronous_index]));
                }
            }
            line.UpdateSynchronize(my_controller);
        }
    }

    void DetectDependency()
    {
        
    }

    void ChangeVisibleState()
    {


    }

    void UpdatePointCloud()
    {
        if (GameObject.Find("PointCloud(Clone)/TCPserver2/PointCloud_0"))
        {
            GameObject.Find("PointCloud(Clone)/TCPserver2/PointCloud_0").GetComponent<DisplayPointCloud>().isRenderFrame = true;
        }
    }
}
