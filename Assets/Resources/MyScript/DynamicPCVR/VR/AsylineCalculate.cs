using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsylineCalculate : MonoBehaviour
{
    private GlobalUtils globalUtils;

    // Start is called before the first frame update
    void Start()
    {
        globalUtils = GameObject.Find("PointCloudVR/TopViewDepthCamera").GetComponent<GlobalUtils>();
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

    public Vector3 AdjustStartPointDependStart(Vector3 start_point)
    {
        Vector3 top_view_2d_corrdinate = globalUtils.MWorldToScreenPointDepth(start_point);
        float min_depth = globalUtils.GetDepth((int)top_view_2d_corrdinate.x, (int)top_view_2d_corrdinate.y);
        top_view_2d_corrdinate.z = min_depth - 0.00001f;
        return globalUtils.MScreenToWorldPointDepth(top_view_2d_corrdinate);
    }

    public Vector3 AdjustStartPointDependEnd(Vector3 start_point)
    {
        Vector3 top_view_2d_corrdinate = globalUtils.MWorldToScreenPointDepth(start_point);
        float min_depth = globalUtils.GetDepth((int)top_view_2d_corrdinate.x, (int)top_view_2d_corrdinate.y);
        top_view_2d_corrdinate.z = min_depth - 0.00001f;
        return globalUtils.MScreenToWorldPointDepth(top_view_2d_corrdinate);
    }

    public Vector3 AdjustEndPointDependStart(Vector3 end_point)
    {
        Vector3 top_view_2d_corrdinate = globalUtils.MWorldToScreenPointDepth(end_point);
        float min_depth = globalUtils.GetDepth((int)top_view_2d_corrdinate.x, (int)top_view_2d_corrdinate.y);
        top_view_2d_corrdinate.z = min_depth - 0.00001f;
        return globalUtils.MScreenToWorldPointDepth(top_view_2d_corrdinate);
    }

    public Vector3 AdjustEndPointDependEnd(Vector3 end_point)
    {
        Vector3 top_view_2d_corrdinate = globalUtils.MWorldToScreenPointDepth(end_point);
        float min_depth = globalUtils.GetDepth((int)top_view_2d_corrdinate.x, (int)top_view_2d_corrdinate.y);
        top_view_2d_corrdinate.z = min_depth - 0.00001f;
        return globalUtils.MScreenToWorldPointDepth(top_view_2d_corrdinate);
    }
}
