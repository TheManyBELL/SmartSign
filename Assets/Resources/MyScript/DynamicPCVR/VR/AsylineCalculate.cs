using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsylineCalculate : MonoBehaviour
{
    private GlobalUtils globalUtils;
    private GlobalUtilsVR globalUtilsVR;

    // Start is called before the first frame update
    void Start()
    {
        globalUtils = GameObject.Find("PointCloud(Clone)/TopViewCamera").GetComponent<GlobalUtils>();
        globalUtilsVR = GetComponent<GlobalUtilsVR>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool CalculateLineComplete(Vector3 end_point)
    {
        Vector3 top_view_2d_corrdinate = globalUtils.MWorldToScreenPointDepth(end_point);
        float min_depth = globalUtils.GetDepth((int)top_view_2d_corrdinate.x, (int)top_view_2d_corrdinate.y);
        Debug.LogFormat("calculte line complete {0}", (min_depth - top_view_2d_corrdinate.z).ToString("f4"));
        return (min_depth < top_view_2d_corrdinate.z - 0.00005f);
    }

    // start-start 高遮挡-重新打射线
    public Vector3 AdjustStartPointDependStart(Ray ray)
    {
        return globalUtilsVR.GetCollisionPoint(ray);
    }

    // start-end 连续移动-向上移动/起点移到终点
    public Vector3 AdjustStartPointDependEnd(Vector3 start_point, SmartCue depend)
    {
        /*LineCue cue = (LineCue)depend;
        Vector3 translation = cue.GetEndPoint() - cue.GetStartPoint();
        start_point += translation;*/

        // return start_point;
        Vector3 top_view_2d_corrdinate = globalUtils.MWorldToScreenPointDepth(start_point);
        float min_depth = globalUtils.GetDepth((int)top_view_2d_corrdinate.x, (int)top_view_2d_corrdinate.y);
        top_view_2d_corrdinate.z = min_depth - 0.00001f;
        return globalUtils.MScreenToWorldPointDepth(top_view_2d_corrdinate);
    }

    // end-start 移到物体移走后的位置-向下移动
    public Vector3 AdjustEndPointDependStart(Vector3 end_point)
    {
        Vector3 top_view_2d_corrdinate = globalUtils.MWorldToScreenPointDepth(end_point);
        float min_depth = globalUtils.GetDepth((int)top_view_2d_corrdinate.x, (int)top_view_2d_corrdinate.y);
        top_view_2d_corrdinate.z = min_depth - 0.00001f;
        return globalUtils.MScreenToWorldPointDepth(top_view_2d_corrdinate);
    }

    // end-end 移到另一个物体移动后的上面-向上移动/终点移到终点
    public Vector3 AdjustEndPointDependEnd(Vector3 end_point, SmartCue depend)
    {
        /*LineCue cue = (LineCue)depend;
        Vector3 translation = cue.GetEndPoint() - cue.GetStartPoint();
        end_point += translation;*/

        // return end_point;
        Vector3 top_view_2d_corrdinate = globalUtils.MWorldToScreenPointDepth(end_point);
        float min_depth = globalUtils.GetDepth((int)top_view_2d_corrdinate.x, (int)top_view_2d_corrdinate.y);
        top_view_2d_corrdinate.z = min_depth - 0.00001f;
        return globalUtils.MScreenToWorldPointDepth(top_view_2d_corrdinate);
    }
}
