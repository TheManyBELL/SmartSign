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
    public Vector3 new_start;
    public Vector3 new_end;

    public enum Type { ͬ�� = 0, �첽 };
    public Type type = Type.ͬ��;
    public bool debug_demo_mode;    // �����첽debug demo���������첽��ʶ����������֣�����de������յ������bug
    public bool debug_asynchronous_manual_switch;
    private bool delayed_update_next = false;    // �첽�л���һ��cueʱ��ɾ����ǰ�������һ������ͬһ֡����֤AR����ȷ����
    private bool delayed_update_previous = false;

    public SteamVR_Action_Boolean next_cue;
    public SteamVR_Action_Boolean previous_cue;

    // dependent related
    public GameObject depend_sphere_prefab;
    public bool auto_generate_line = true;

    private Exp exp;

    public Vector3 picture_start, picture_end;
    public bool draw_picture_line;
    public bool picture;
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
        if (type == Type.�첽)
        {
            AsyTickTODOList();
        }


        // ��ͼר��  cue_list[synchronous_index].type.Equals(CueType.Line)
        if (synchronous_index >= 0 && synchronous_index < cue_list.Count && cue_list[synchronous_index].type.Equals(CueType.Line) && picture)
        {
            LineCue line = (LineCue)cue_list[synchronous_index];
            line.p1 = picture_start / 10;
            line.p2 = picture_end / 10;
            line.UpdateSynchronize(my_controller);
        }

        if (draw_picture_line)
        {
            draw_picture_line = false;
            AddLine(picture_start / 10, picture_end / 10);
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

        // �ж��Ƿ����꣬�л���һ��ֱ��
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
        if (type == Type.�첽) cue_list.Add(new LineCue(start_point, end_point, depend_sphere_prefab));
        else cue_list.Add(new LineCue(start_point, end_point));
        // else cue_list.Add(new SmartCue());

        if (auto_generate_line && cue_list.Count > 1 && cue_list[cue_list.Count - 2].type != CueType.Line)
        {
            SynchronizePair();
        } 
        else
        {
            SynchronizeCurrentCue();
        }
        
    }

    public void AddAxes(GameObject initial_axes, GameObject final_axes)
    {
        cue_list.Add(new AxesCue(initial_axes, final_axes));
        if (!auto_generate_line) 
        {
            SynchronizeCurrentCue();
        } 
/*        if (auto_generate_line)
        {
            cue_list.Add(new LineCue(initial_axes.transform.position, final_axes.transform.position));
            SynchronizeCurrentCue(true);
        }*/
    }

    public void AddSplitCut(Vector3 center, List<List<Vector3>> vertices, List<List<Color>> color, GameObject split_obj)
    {
        // Debug.LogWarningFormat("color size {0}, vertices size {1}", vertices.Count, color.Count);
        // Debug.LogWarningFormat("color 0 size {0}, vertices 0 size {1}", vertices[0].Count, color[0].Count);

        cue_list.Add(new CutpieceCue(center, vertices, color, split_obj));
        if (!auto_generate_line)
        {
            SynchronizeCurrentCue();
        }
    }

    private void SynchronizePair()
    {
        if (type == Type.ͬ�� || synchronous_index == -1)
        {
            cue_list[cue_list.Count - 1].Synchronize(my_controller);
            cue_list[cue_list.Count - 2].Synchronize(my_controller);
            synchronous_index = cue_list.Count - 1;
        }
    }

    private void SynchronizeCurrentCue(bool auto_gen = false)
    {
        if (type == Type.ͬ�� || synchronous_index == -1 || auto_gen)
        {
            cue_list[cue_list.Count - 1].Synchronize(my_controller);
            synchronous_index = cue_list.Count - 1;

            // ��ͼר��
            if (cue_list[synchronous_index].type == CueType.Line)
            {
                LineCue line = (LineCue)cue_list[synchronous_index];
                picture_start = line.p1 * 10;
                picture_end = line.p2 * 10;
            }
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
        cue_list.RemoveAt(cue_list.Count - 1);      // ɾ���Ժ�Ŀǰ����ʾ��һ��
        // synchronous_index -= 1;
    }

    public void SynchronizeNextCueStep1()   // ֹͣͬ����һ��
    {
        if (!exp.asynchronous_ar_begin) // ֻΪ��¼AR��ʼ
        {
            exp.asynchronous_ar_begin = true;
            exp.AsynchronousARBegin();
            return;
        }

        if (synchronous_index >= cue_list.Count || synchronous_index == -1) return;
        cue_list[synchronous_index].StopSynchronize(my_controller, false);

        // new 
        if (synchronous_index >= 1 && cue_list[synchronous_index].type == CueType.Line &&
            cue_list[synchronous_index - 1].type != CueType.Line && auto_generate_line)
        {
            cue_list[synchronous_index - 1].StopSynchronize(my_controller, false);
        }

        delayed_update_next = true;

        AdjustNewCue();
    }

    public void SynchronizeNextCueStep2() // ͬ����һ��
    {
        synchronous_index += 1;     
        if (synchronous_index < cue_list.Count)
        {
            cue_list[synchronous_index].Synchronize(my_controller);

            // ��ͼר��
            /*if (cue_list[synchronous_index].type == SmartCue.CueType.Line)
            {
                LineCue line = (LineCue)cue_list[synchronous_index];
                picture_start = line.p1;
                picture_end = line.p2;
            }*/

            // new 
            if (cue_list[synchronous_index].type != CueType.Line && auto_generate_line && synchronous_index + 1 < cue_list.Count)
            {
                synchronous_index += 1;
                cue_list[synchronous_index].Synchronize(my_controller);
            }
        }
        else
        {
            synchronous_index = -1;
        }
        delayed_update_next = false;
        exp.VRBeginAREnd(); // ��¼AR����
    }

    public void SynchronizePreviousCueStep1()
    {
        if (synchronous_index <= 0) return;
        cue_list[synchronous_index].StopSynchronize(my_controller, true);     // ֹͣͬ����һ��

        if (synchronous_index >= 1 && cue_list[synchronous_index].type == CueType.Line &&
            cue_list[synchronous_index - 1].type != CueType.Line && auto_generate_line)
        {
            cue_list[synchronous_index - 1].StopSynchronize(my_controller, false);
        }

        delayed_update_previous = true;
    }

    public void SynchronizePreviousCueStep2()
    {
        synchronous_index -= 1;     // ͬ����һ��
        if (synchronous_index >= 0)
        {
            cue_list[synchronous_index].Synchronize(my_controller);

            // new 
            if (cue_list[synchronous_index].type != CueType.Line && auto_generate_line && synchronous_index - 1 >= 0)
            {
                synchronous_index -= 1;
                cue_list[synchronous_index].Synchronize(my_controller);
            }
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
            if (cue_list[i].type != CueType.Line) continue;

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

    void AdjustNewCue()
    {
        if (synchronous_index < 0 || synchronous_index >= cue_list.Count) return;
        for (int i = synchronous_index + 1; i < cue_list.Count; ++i)
        {
            Debug.LogWarning("cnm");
            if (cue_list[i].type != CueType.Line) continue;
            Debug.LogWarning("ctm");

            LineCue line = (LineCue)cue_list[i];
            if (!line.depend.is_depend) continue;

            foreach (var d in line.depend.depend_list)
            {
                if (d.Key != synchronous_index) continue;
                if (d.Value == Depend.DependType.start_start)
                {
                    Debug.LogWarning("start_start");
                    // line.SetStartPoint(asy_calculate.AdjustStartPointDependStart(line.ray));
                }
                else if (d.Value == Depend.DependType.start_end)
                {
                    Debug.LogWarning("start_end");
                    line.SetStartPoint(new_start);
                }
                else if (d.Value == Depend.DependType.end_start)
                {
                    Debug.LogWarning("end_start");
                    // line.SetEndPoint(asy_calculate.AdjustEndPointDependStart(line.GetEndPoint()));
                }
                else if (d.Value == Depend.DependType.end_end)
                {
                    Debug.LogWarning("end_end");
                    line.SetEndPoint(new_end);
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
