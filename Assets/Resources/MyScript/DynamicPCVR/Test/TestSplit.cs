using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestSplit : MonoBehaviour
{
    private TestGlobalUtils globalUtils;

    // Start is called before the first frame update
    void Start()
    {
        globalUtils = GameObject.Find("Script").GetComponent<TestGlobalUtils>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SplitCPU(List<Vector3> points)
    {
        float xmin = float.MaxValue, xmax = float.MinValue,
            ymin = float.MaxValue, ymax = float.MinValue;

        List<Vector3> plane_points = new List<Vector3>();
        foreach (Vector3 p in points)
        {
            Vector3 plane_p = globalUtils.MWorldToScreenPointDepth(p);
            plane_points.Add(plane_p);
            xmin = Mathf.Max(Mathf.Min(xmin, plane_p.x), 0.0f);
            xmax = Mathf.Min(Mathf.Max(xmax, plane_p.x), Screen.width);
            ymin = Mathf.Max(Mathf.Min(ymin, plane_p.y), 0.0f);
            ymax = Mathf.Min(Mathf.Max(ymax, plane_p.y), Screen.height);
        }

        Debug.Log(xmin + " " + xmax + " " + ymin + " " + ymax + " ");

        List<Vector3> vertices = new List<Vector3>();
        List<Color> color = new List<Color>();

        float del = (float)50 / 255;
        for (int i = (int)xmin; i < xmax; ++i)
        {
            for (int j = (int)ymin; j < ymax; ++j)
            {
                for (int k = 0; k < 1000; ++k) { }

                if (pointInPolygon(new Vector2(i, j), plane_points)) {

                    float d = globalUtils.GetDepth(i, j);
                    if (d == 1.0f) continue;
                    // i dont know why
                    Color c = globalUtils.GetColor(i, j) - new Color(del, del, del, 0);
                    
                    Vector3 plane_p = new Vector3(i, j, d);
                    Vector3 world_p = globalUtils.MScreenToWorldPointDepth(plane_p);

                    vertices.Add(world_p);
                    color.Add(c);
                }
            }
        }

        globalUtils.CreateNewObjUsingVertices(ref vertices, ref color);
    }

    void SplitGPU(List<Vector3> points)
    {

    }

    bool IntersectHorizontalRight(Vector2 start_p, Vector3 line_p1, Vector3 line_p2)
    {
        float x1 = line_p1.x, y1 = line_p1.y,
           x2 = line_p2.x, y2 = line_p2.y,
           x = start_p.x, y = start_p.y;

        if (Mathf.Abs(x1 - x2) < 0.0001) return ((y1 < y && y < y2) || (y2 < y && y < y1)) && (x <= x1 || x <= x2);
        if (Mathf.Abs(y1 - y2) < 0.0001) return false;

        // x = ky + b
        float k = (x2 - x1) / (y2 - y1);
        float b = (x1 * y2 - x2 * y1) / (y2 - y1);
        float intersect_x = k * y + b;
        return ((x1 < intersect_x && intersect_x < x2) || (x2 < intersect_x && intersect_x < x1)) && (x < intersect_x);
    }

    bool IntersectHorizontalRight(Vector2 start_p, float k, float b, Vector3 line_p1, Vector3 line_p2)
    {
        float x1 = line_p1.x, y1 = line_p1.y,
           x2 = line_p2.x, y2 = line_p2.y,
           x = start_p.x, y = start_p.y;

        if (Mathf.Abs(x1 - x2) < 0.0001) return ((y1 < y && y < y2) || (y2 < y && y < y1)) && (x <= x1 || x <= x2);
        if (Mathf.Abs(y1 - y2) < 0.0001) return false;

        float intersect_x = k * y + b;
        return ((x1 < intersect_x && intersect_x < x2) || (x2 < intersect_x && intersect_x < x1)) && (x < intersect_x);
    }

    void Calculkb(Vector3 line_p1, Vector3 line_p2, ref float k, ref float b)
    {
        float x1 = line_p1.x, y1 = line_p1.y,
           x2 = line_p2.x, y2 = line_p2.y;

        if (Mathf.Abs(y1 - y2) > 0.0001)
        {
            k = (x2 - x1) / (y2 - y1);
            b = (x1 * y2 - x2 * y1) / (y2 - y1);
        } 
    }

    bool pointInPolygon(Vector2 p, List<Vector3> polygon_points)
    {
        List<float> k = new List<float>();
        List<float> b = new List<float>();
        for (int i = 0; i < polygon_points.Count - 1; ++i)
        {
            float tk = 0, tb = 0;
            Calculkb(polygon_points[i], polygon_points[i + 1], ref tk, ref tb);
            k.Add(tk); b.Add(tb);
        }

        int intersetion_count = 0;
        for (int i = 0; i < polygon_points.Count - 1; ++i)
        {
            Vector3 line_p1 = polygon_points[i],
                line_p2 = polygon_points[i + 1];

            if (IntersectHorizontalRight(p, k[i], b[i], line_p1, line_p2)) 
                intersetion_count++;
        }
        return (intersetion_count % 2 == 1);
    }
}
