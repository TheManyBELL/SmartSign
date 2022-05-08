using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;


public class LineDisocclusionVRA : MonoBehaviour
{
    private MirrorControllerA mirrorController;
    private Vector3 p1, p2;
    private DPCArrow current_line;

    public int VisibilitySampleCount = 20, BezierSampleCount = 50, CatmullRomSampleCount = 50;
    public bool StarightLineCompleteConvered;
    private const float DirectChangeDepthVal = 0.0002f;
    private int[,] Cnk;
    public bool[] lineVisibility;

    private List<Vector3[]> curve_list;

    // filter
    public bool filter = true;
    public int filter_frame_count = 20;
    private int current_line_index;
    private List<visibiltyFilter[]> pastLineVisibility;

    private GlobalUtils globalUtils;

    private void Start()
    {
        mirrorController = GetComponentInParent<MirrorControllerA>();
        globalUtils = GetComponent<GlobalUtils>();

        InitCnk(VisibilitySampleCount);
        lineVisibility = new bool[VisibilitySampleCount];
        curve_list = new List<Vector3[]>();
        pastLineVisibility = new List<visibiltyFilter[]>();
    }

    private void Update()
    {
        // filter
        while (pastLineVisibility.Count > mirrorController.syncArrowList.Count)
        {
            pastLineVisibility.RemoveAt(pastLineVisibility.Count - 1);
        }
        while (pastLineVisibility.Count < mirrorController.syncArrowList.Count)
        {
            visibiltyFilter[] tmp = new visibiltyFilter[VisibilitySampleCount];
            for (int i = 0; i < VisibilitySampleCount; ++i)
            {
                tmp[i] = new visibiltyFilter()
                {
                    true_count = 0,
                    false_count = 0,
                    past_val = new Queue<bool>()
                };
            }
            pastLineVisibility.Add(tmp);
        }

        for (int i = 0; i < mirrorController.syncArrowList.Count;++i)
        {
            current_line = mirrorController.syncArrowList[i];
            // current_line.curvePointList.Clear();

            p1 = current_line.startPoint;
            p2 = current_line.endPoint;
            curve_list.Clear();
            arrowDisocclusion();

            current_line.curvePointList.Clear();
            current_line.curvePointList.AddRange(curve_list);
            mirrorController.CmdUpdateDPCArrow(current_line);

        }
    }

    private void arrowDisocclusion()
    {
        // De occlusion calculation is performed here
        RaisestraightLineLogo();
        DrawArrow();
        AdjustPointOrder();
        DrawVisiblestraightLine();
        detourToEndpoint();
        detourToUnvisiblePoint();
    }

    private void InitCnk(int maxn = 30)
    {
        Cnk = new int[maxn, maxn];
        for (int i = 0; i < maxn; ++i)
        {
            for (int j = 0; j < maxn; ++j)
            {
                if (j > i) Cnk[i, j] = 0;
                else if (j == i) Cnk[i, j] = 1;
                else if (j == 0) Cnk[i, j] = 1;
                else Cnk[i, j] = Cnk[i - 1, j] + Cnk[i - 1, j - 1];
            }
        }
    }

    private bool inScreenRange(Vector3 v) =>
        0 <= v.x && v.x < Screen.width && 0 <= v.y && v.y < Screen.height && 0 <= v.z && v.z <= 1;

    private Vector3 scaleToVec(int i) =>
        p1 + (float)i / (VisibilitySampleCount - 1) * (p2 - p1);

    private void DrawArrow()
    {
        if (!globalUtils.GetPointVisibility(p2))
        {
            return;
        }
        Vector3 screenP1 = globalUtils.MWorldToScreenPointDepth(p1);
        Vector3 screenP2 = globalUtils.MWorldToScreenPointDepth(p2);
        Vector2 dir = (screenP1 - screenP2).normalized;
        Vector2 verticalDir = new Vector2(-dir.y, dir.x);

        int length = 10;
        Vector3 screenArrowP1 = screenP2 + length * new Vector3(verticalDir.x, verticalDir.y) 
            + length * new Vector3(dir.x, dir.y);
        Vector3 screenArrowP2 = screenP2 - length * new Vector3(verticalDir.x, verticalDir.y) 
            + length * new Vector3(dir.x, dir.y);

        Vector3 arrowP1 = globalUtils.MScreenToWorldPointDepth(screenArrowP1);
        Vector3 arrowP2 = globalUtils.MScreenToWorldPointDepth(screenArrowP2);

        curve_list.Add(new Vector3[] { arrowP1, p2 });
        curve_list.Add(new Vector3[] { arrowP2, p2 });
    }

    private void AdjustPointOrder()
    {
        if (Vector3.Dot((p2 - p1), globalUtils.depthCamera.transform.right) < 0)
        {
            Vector3 t = p1;
            p1 = p2;
            p2 = t;
        }
    }

    private bool GetSamplePointsVisibility()
    {
        bool completeConvered = true, completeOutofView = true;
        for (int i = 0; i < VisibilitySampleCount; ++i)
        {
            Vector3 p = scaleToVec(i);
            Vector3 screenP = globalUtils.MWorldToScreenPointDepth(p);

            if (!inScreenRange(screenP))
            {
                lineVisibility[i] = true;
                continue;
            }

            completeOutofView = false;
            float minDepth = globalUtils.GetDepth((int)screenP.x, (int)screenP.y);
            // float testVisibleThreshold = 0.00001f; ;
            lineVisibility[i] = (minDepth >= screenP.z);

            //filter
            pastLineVisibility[current_line_index][i].past_val.Enqueue(lineVisibility[i]);
            pastLineVisibility[current_line_index][i].true_count += lineVisibility[i] ? 1 : 0;
            pastLineVisibility[current_line_index][i].false_count += lineVisibility[i] ? 0 : 1;
            if (pastLineVisibility[current_line_index][i].past_val.Count > filter_frame_count)
            {
                bool val = pastLineVisibility[current_line_index][i].past_val.Dequeue();
                pastLineVisibility[current_line_index][i].true_count -= val ? 1 : 0;
                pastLineVisibility[current_line_index][i].false_count -= val ? 0 : 1;

                if (filter && lineVisibility[i] && pastLineVisibility[current_line_index][i].true_count != filter_frame_count)
                {
                    lineVisibility[i] = pastLineVisibility[current_line_index][i].last_frame_val;
                }

                if (filter && !lineVisibility[i] && pastLineVisibility[current_line_index][i].false_count != filter_frame_count)
                {
                    lineVisibility[i] = pastLineVisibility[current_line_index][i].last_frame_val;
                }
                pastLineVisibility[current_line_index][i].last_frame_val = lineVisibility[i];
            }

            if (completeConvered)
            {
                completeConvered = !lineVisibility[i];
            }
        }
        if (completeOutofView)
        {
            completeConvered = false;
        }

        return completeConvered;
    }

    private void RaisestraightLineLogo()
    {
        float step = 0.1f;

        while (GetSamplePointsVisibility())
        {
            p1 += step * globalUtils.depthCamera.transform.up;
            p2 += step * globalUtils.depthCamera.transform.up;
            step *= 2;
        }
    }

    private void DrawVisiblestraightLine()
    {
        int i = 0;
        while (i < VisibilitySampleCount)
        {
            if (lineVisibility[i])
            {
                Vector3 tp = scaleToVec(i);
                while (i < VisibilitySampleCount && lineVisibility[i]) { i++; }
                curve_list.Add(new Vector3[] { tp, scaleToVec(i - 1) });
            }
            i++;
        }
    }

    private void ChangePointDepth(ref Vector3 p, float changeDepth = DirectChangeDepthVal)
    {
        Vector3 screenP = globalUtils.MWorldToScreenPointDepth(p);
        float minDepth = globalUtils.GetDepth((int)screenP.x, (int)screenP.y);

        if (minDepth < screenP.z)
        {
            screenP.z = minDepth - changeDepth;
            p = globalUtils.MScreenToWorldPointDepth(screenP);
        }
    }

    // =========================================================== detour to endpoint ============================================================
    private void detourToEndpoint()
    {
        Vector3 screenP1 = globalUtils.MWorldToScreenPointDepth(p1);
        Vector3 screenP2 = globalUtils.MWorldToScreenPointDepth(p2);
        Vector2 screenP12 = screenP2 - screenP1;
        screenP12.Normalize();

        if (!lineVisibility[0])
        {
            int i = 0;
            while (i < VisibilitySampleCount - 1 && !lineVisibility[++i]) { }
            Vector3 visibleP = scaleToVec(i);

            // Vector3 edgeP1 = disturbanceEndpoint(p1, screenP12 * -1.0f);
            Vector3 edgeP1 = disturbanceEndpoint(p1, new Vector2(-1.0f, 0.0f));
            Vector3 extraP = p1 + (visibleP - edgeP1);
            var catmullRomP = new List<Vector3> { extraP, p1, edgeP1, visibleP, extraP };
            DrawCatmullRomCurve(catmullRomP);
        }

        if (!lineVisibility[VisibilitySampleCount - 1])
        {
            int i = VisibilitySampleCount - 1;
            while (i > 0 && !lineVisibility[--i]) { }
            Vector3 visibleP = scaleToVec(i);

            // Vector3 edgeP2 = disturbanceEndpoint(p2, screenP12);
            Vector3 edgeP2 = disturbanceEndpoint(p2, new Vector2(1.0f, 0.0f));
            Vector3 extraP = p2 + (visibleP - edgeP2);
            var catmullRomP = new List<Vector3> { extraP, p2, edgeP2, visibleP, extraP };

            DrawCatmullRomCurve(catmullRomP);
        }
    }

    private Vector3 disturbanceEndpoint(Vector3 p, Vector2 direction)
    {
        Vector3 faceP = globalUtils.MWorldToScreenPointDepth(p);
        float lastMinDepth = globalUtils.GetDepth((int)faceP.x, (int)faceP.y);
        // faceP.z = lastMinDepth;

        float threshold = 0.0003f;
        int MAXSTEP = 0;
        while (inScreenRange(faceP + new Vector3(direction.x, direction.y)))
        {
            faceP += new Vector3(direction.x, direction.y);     // sets z = 0
            float minDepth = globalUtils.GetDepth((int)faceP.x, (int)faceP.y);
            if (Math.Abs(minDepth - lastMinDepth) > threshold)
            {
                break;
            }
            faceP.z = Math.Min(faceP.z, minDepth);
            lastMinDepth = minDepth;

            ++MAXSTEP;
            if (MAXSTEP > 50)
            {
                break;
            }
        }

        float dis = Vector3.Distance(globalUtils.MScreenToWorldPointDepth(faceP), p);
        faceP -= new Vector3(0.0f, 0.0f, 0.0002f);
        faceP += dis * 30.0f * new Vector3(direction.x, direction.y);
        for (int i = 0; i < 10; i++)
        {
            faceP -= dis * 10.0f * new Vector3(0.0f, 1.0f, 0.0f);
            if (!inScreenRange(faceP))
            {
                break;
            }
            float minDepth = globalUtils.GetDepth((int)faceP.x, (int)faceP.y);
            if (minDepth < faceP.z)
            {
                break;
            }
        }
        faceP += dis * 10.0f * new Vector3(0.0f, 1.0f, 0.0f);

        faceP.x = Math.Max(faceP.x, 0.0f);
        faceP.x = Math.Min(faceP.x, (float)Screen.width);
        faceP.y = Math.Max(faceP.y, 0.0f);
        faceP.y = Math.Min(faceP.y, (float)Screen.height);

        p = globalUtils.MScreenToWorldPointDepth(faceP);
        return p;
    }

    private void DrawCatmullRomCurve(List<Vector3> controlPoints)
    {
        float factor = 0.8f;
        int index = 0;
        int pointCount = (controlPoints.Count - 3) * (CatmullRomSampleCount + 1);
        Vector3[] curvePoints = new Vector3[pointCount];

        for (int i = 0; i + 3 < controlPoints.Count; ++i)
        {
            Vector3 p0 = controlPoints[i], p1 = controlPoints[i + 1],
                p2 = controlPoints[i + 2], p3 = controlPoints[i + 3];

            float t = 0, step = 1.0f / CatmullRomSampleCount;
            while (t <= 1 + 0.0001f)
            {
                Vector3 c0 = p1;
                Vector3 c1 = (p2 - p0) * factor;
                Vector3 c2 = (p2 - p1) * 3.0f - (p3 - p1) * factor - (p2 - p0) * 2.0f * factor;
                Vector3 c3 = (p2 - p1) * -2.0f + (p3 - p1) * factor + (p2 - p0) * factor;

                Vector3 curvePoint = c3 * t * t * t + c2 * t * t + c1 * t + c0;

                if (i == 1)
                {
                    ChangePointDepth(ref curvePoint, 0.0001f);
                }
                curvePoints[index++] = curvePoint;
                t += step;
            }
        }

        curve_list.Add(curvePoints);
    }

    // ====================================================== detour to UnvisiblePoint ============================================================
    private void detourToUnvisiblePoint()
    {
        int l = 0, r = VisibilitySampleCount - 1;
        while (!lineVisibility[l]) { l++; }
        while (!lineVisibility[r]) { r--; }

        List<Vector3> controlPoints = new List<Vector3>();
        bool meetl = false;
        for (int i = l; i <= r; ++i)
        {
            Vector3 p = scaleToVec(i);

            // new 
            // right visible 放前面 因为这个点可能同时是起点和终点
            if (meetl && i >= 1 && !lineVisibility[i - 1] && lineVisibility[i])
            {
                controlPoints.Add(p);
                disturbanceControlPoints(ref controlPoints);  // 绕一下
                DrawBezierCurve(ref controlPoints);
                controlPoints.Clear();
                meetl = false;
            }
            // left visible
            if (i + 1 < VisibilitySampleCount && lineVisibility[i] && !lineVisibility[i + 1])
            {
                controlPoints.Add(p);
                meetl = true;
            }
            // invisible
            if (meetl && !lineVisibility[i])
            {
                ChangePointDepth(ref p);
                controlPoints.Add(p);
            }
        }

    }

    void disturbanceControlPoints(ref List<Vector3> controlPoints)
    {
        int countP = controlPoints.Count;
        Vector3 lookAt = globalUtils.depthCamera.transform.forward;
        Vector3 line = controlPoints[countP - 1] - controlPoints[0];
        Vector3 offset = Vector3.Distance(controlPoints[0], controlPoints[countP - 1]) * Vector3.Cross(line, lookAt).normalized;  
        // std::cout << offset.x << " " << offset.y << " " << offset.z << ";" << std::endl;

        for (int i = 1, j = countP - 2; i <= j; i++, j--)
        {
            float coef = (float)i / countP * 0.3f;
            controlPoints[i] += coef * offset;
            if (i != j)
            {
                controlPoints[j] += coef * offset;
            }
        }
    }

    private void DrawBezierCurve(ref List<Vector3> controlPoints)
    {
        int n = controlPoints.Count - 1;

        double t = 0, dt = 1.0 / BezierSampleCount;
        Vector3[] curvePoints = new Vector3[BezierSampleCount + 1];
        for (int j = 0; j <= BezierSampleCount; ++j)
        {
            Vector3 np = new Vector3(0.0f, 0.0f, 0.0f);
            for (int i = 0; i <= n; ++i)
            {
                float coef = (float)Math.Pow(t, i) * (float)Math.Pow(1 - t, n - i) * Cnk[n, i];
                np += coef * controlPoints[i];
            }
            ChangePointDepth(ref np, 0.0001f);
            curvePoints[j] = np;
            t += dt;
        }
        curve_list.Add(curvePoints);
    }
}
