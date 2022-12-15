using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.Extras;


public class MiddleFactoryVRA : MonoBehaviour
{
    private MirrorControllerA my_controller;
    private AsylineCalculate asy_calculate;
    public List<SmartCue> cue_list;
    public int synchronous_index;


    public enum Type { ͬ�� = 0, �첽 };
    public Type type = Type.ͬ��;
    public bool debug_demo_mode;    // �����첽debug demo���������첽��ʶ����������֣�����de������յ������bug
    public bool debug_asynchronous_manual_switch;

    public SteamVR_Action_Boolean next_cue;
    public SteamVR_Action_Boolean previous_cue;

    // dependent related
    public float dependency_threshold;
    public GameObject threshold_sphere_prefab;
    public bool visible_dependency_threshold;
    private bool last_visible_state = true;

    // Start is called before the first frame update
    void Start()
    {
        my_controller = GetComponentInParent<MirrorControllerA>();
        asy_calculate = GetComponent<AsylineCalculate>();
        cue_list = new List<SmartCue>();
    }

    // Update is called once per frame
    void Update()
    {
        if (type == Type.�첽)
        {
            AsyTickTODOList();
        }     
    }

    void AsyTickTODOList()
    {
        // ���ӻ�����
        if (last_visible_state != visible_dependency_threshold)
        {
            ChangeVisibleState();
            last_visible_state = visible_dependency_threshold;
        }

        // �ж��Ƿ����꣬�л���һ��ֱ��
        if ((!debug_demo_mode && CurrentCueComplete()) || next_cue.GetStateDown(SteamVR_Input_Sources.LeftHand))
        {
            SynchronizeNextCue();
            AdjustCue();      
        }

        if (debug_asynchronous_manual_switch || previous_cue.GetStateDown(SteamVR_Input_Sources.LeftHand))
        {
            SynchronizePreviousCue();
            debug_asynchronous_manual_switch = false;
        }
    }

    public void AddLine(Vector3 start_point, Vector3 end_point)
    {
        cue_list.Add(new LineCue(start_point, end_point));
        SynchronizeCurrentCue();
    }

    public void AddAxes(GameObject initial_axes, GameObject final_axes)
    {
        cue_list.Add(new AxesCue(initial_axes, final_axes));
        SynchronizeCurrentCue();
    }

    public void AddSplitCut(Vector3 center, List<List<Vector3>> vertices, List<List<Color>> color, GameObject split_obj)
    {
        cue_list.Add(new CutpieceCue(center, vertices, color, split_obj));
        SynchronizeCurrentCue();
    }

    private void SynchronizeCurrentCue()
    {
        if (type == Type.ͬ��)
        {
            cue_list[cue_list.Count - 1].Synchronize(my_controller);
        }
    }

    public void SynchronizeUpdate()
    {
        cue_list[cue_list.Count - 1].UpdateSynchronize(my_controller);
    }

    public void RemoveSmartCue()
    {
        if (cue_list.Count <= 0) return;
        cue_list[cue_list.Count - 1].Remove(my_controller);
        // cue_list.RemoveAt(cue_list.Count - 1);      // ɾ���Ժ�Ŀǰ����ʾ��һ��
        synchronous_index -= 1;
    }

    public void SynchronizeNextCue()
    {
        cue_list[synchronous_index].StopSynchronize(my_controller);     // ֹͣͬ����һ��
        synchronous_index += 1;     // ͬ����һ��
        if (synchronous_index < cue_list.Count)
        {
            cue_list[synchronous_index].Synchronize(my_controller);
        }
    }

    public void SynchronizePreviousCue()
    {
        cue_list[synchronous_index].StopSynchronize(my_controller);     // ֹͣͬ����һ��
        synchronous_index -= 1;     // ͬ����һ��
        if (synchronous_index >= 0)
        {
            cue_list[synchronous_index].Synchronize(my_controller);
        }
    }


    bool CurrentCueComplete()
    {
        return false;
    }


    void AdjustCue()
    {
        if (synchronous_index < 0 || synchronous_index >= cue_list.Count) return;
    }

    void DetectDependency()
    {
        
    }

    void ChangeVisibleState()
    {


    }
}
