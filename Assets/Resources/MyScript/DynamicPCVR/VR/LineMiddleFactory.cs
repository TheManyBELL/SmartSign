using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct MiddleLine
{
    public int index;
    public Vector3 start_point;
    public Vector3 end_point;
    public bool finish;
    public List<int> depent_line;

    public MiddleLine(int index, Vector3 start_point, Vector3 end_point)
    {
        this.index = index;
        this.start_point = start_point;
        this.end_point = end_point;
        this.finish = false;
        this.depent_line = new List<int>();
    }
};


public class LineMiddleFactory : MonoBehaviour
{
    private MirrorControllerA myController;
    // private static int current_line_count = 0;
    private int current_ar_line_index = -1;
    private List<MiddleLine> line_list;

    public enum LineExpType { 同步 = 0, 异步 };
    public LineExpType current_exp_type = LineExpType.同步;

    // Start is called before the first frame update
    void Start()
    {
        line_list = new List<MiddleLine>();
        myController = GetComponentInParent<MirrorControllerA>();
    }

    // Update is called once per frame
    void Update()
    {
        // 异步
        if (CompleteLine()) SwitchNextLine();
    }

    void AddNewLine(Vector3 start_point, Vector3 end_point)
    {
        line_list.Add(new MiddleLine(line_list.Count, start_point, end_point));
        if (current_exp_type == LineExpType.同步) SwitchNextLine();   // 同步和异步不优化是一致的，我画线以后就直接同步到网络，删除则靠专家
        else DetectDependence();
    }

    /*
     * 仅用于异步
     * 检测当前ar端线是否完成
     */
    bool CompleteLine()
    {
        return true;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    void AddToNetwork()
    {
        myController.CmdAddDPCArrow(new DPCArrow()
        {
            index = myController.syncArrowList.Count,
            startPoint = line_list[current_ar_line_index].start_point,
            endPoint = line_list[current_ar_line_index].end_point,
            curvePointList = new List<Vector3[]>(),
            originPointList = new List<Vector3[]>(),
        });
    }


    void SwitchNextLine()
    {
        current_ar_line_index++;
        AddToNetwork();
    }

    void DetectDependence()
    {

    }
}
