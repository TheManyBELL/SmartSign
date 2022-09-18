using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGlobalUtils : MonoBehaviour
{
    public Camera depthCamera;
    public int unvisible_count;     // 用于多少个点被遮挡认为物体不可见
    private TestDepthDPC GetDepthScript;

    // 辅助碰撞
    public GameObject assistColliderSpherePrefab;
    private GameObject assistColliderSphere;
    // 根据顶点生成物体
    public GameObject splitPrefab; 

    void Awake()
    {
        depthCamera = GameObject.Find("DepthCamera").GetComponent<Camera>();
        GetDepthScript = GameObject.Find("DepthCamera").GetComponent<TestDepthDPC>();

        assistColliderSphere = Instantiate(assistColliderSpherePrefab);
        assistColliderSphere.layer = LayerMask.NameToLayer("DepthCameraUnivisible");
        assistColliderSphere.SetActive(false);
    }

    private void Start()
    {
    }

    private void Update()
    {
    }

    public float GetDepth(int x, int y) => GetDepthScript.GetDepth(x, y);

    public Color GetColor(int x, int y) => GetDepthScript.GetColor(x, y);

    public Vector3 MScreenToWorldPointDepth(Vector3 p)
    {
        p.z *= depthCamera.farClipPlane;
        return depthCamera.ScreenToWorldPoint(p);
    }

    public Vector3 MWorldToScreenPointDepth(Vector3 p)
    {
        Vector3 screenP = depthCamera.WorldToScreenPoint(p);
        screenP.z = screenP.z / depthCamera.farClipPlane;
        return screenP;
    }

    public bool GetPointVisibility(Vector3 p)
    {
        Vector3 screenP = MWorldToScreenPointDepth(p);
        if (screenP.x < 0 || screenP.x > Screen.width || screenP.y < 0 || screenP.y > Screen.height)
            return true;
        float minDepth = GetDepthScript.GetDepth((int)screenP.x, (int)screenP.y);

        return minDepth > screenP.z;
    }

    public bool GameObjectVisible(GameObject t)
    {
        Bounds tAABB;
        if (t.GetComponentsInChildren<Transform>(true).Length > 1)
        {
            tAABB = t.transform.GetChild(0).GetComponent<MeshRenderer>().bounds;
        }
        else
        {
            tAABB = t.GetComponent<MeshRenderer>().bounds;
        }
        // var tAABB = t.GetComponent<MeshRenderer>().bounds;
        float x = tAABB.extents.x, y = tAABB.extents.y, z = tAABB.extents.z;
        float scale = 0.9f;
        Vector3[] vAABB = new Vector3[]{
            tAABB.center + scale * new Vector3( x,  y,  z),
            tAABB.center + scale * new Vector3( x,  y, -z),
            tAABB.center + scale * new Vector3( x, -y,  z),
            tAABB.center + scale * new Vector3( x, -y, -z),
            tAABB.center + scale * new Vector3(-x,  y,  z),
            tAABB.center + scale * new Vector3(-x,  y, -z),
            tAABB.center + scale * new Vector3(-x, -y,  z),
            tAABB.center + scale * new Vector3(-x, -y, -z)
        };
        int unvisible = 0;
        /*foreach (var v in vAABB)
        {
            if (!GetPointVisibility(v))
            {
                return false;
            }
        }
        return true;*/
        /*foreach (var v in vAABB)
        {
            if (GetPointVisibility(v))
            {
                return true;
            }
        }
        return false;*/
        foreach (var v in vAABB)
        {
            if (!GetPointVisibility(v))
            {
                ++unvisible;
            }
        }
        return (unvisible <= unvisible_count);
    }

    public Vector3 GetCollisionPoint()
    {
        Ray ray = depthCamera.ScreenPointToRay(Input.mousePosition);
        return GetCollisionPoint(ray);
    }

    public Vector3 GetCollisionPoint(Ray ray)
    {
        assistColliderSphere.SetActive(true);

        //TODO
        int MAXSTEP = 1000, stepCount = 0;
        float step = 0.01f;
        assistColliderSphere.transform.position = ray.origin;
        while (GameObjectVisible(assistColliderSphere))
        {
            assistColliderSphere.transform.position += step * ray.direction;
            stepCount++;
            if (stepCount > MAXSTEP) break;
        }

        assistColliderSphere.SetActive(false);

        return (assistColliderSphere.transform.position - 2 * step * ray.direction);
    }

    public GameObject CreateNewLine(string objName)
    {
        GameObject lineObj = new GameObject(objName);
        lineObj.transform.SetParent(this.transform);
        LineRenderer curveRender = lineObj.AddComponent<LineRenderer>();
        
        lineObj.layer = LayerMask.NameToLayer("DepthCameraUnivisible"); ;

        curveRender.startWidth = 0.002f;
        curveRender.endWidth = 0.002f;

        return lineObj;
    }

    public void CreateNewObjUsingVertices(ref List<Vector3> vertices, ref List<Color> colors)
    {

        /*GameObject split_target = Instantiate(splitPrefab);
        split_target.name = "SplitTarget";

        Vector3[] vertices2 = { new Vector3(0, 0, 0), new Vector3(0, 2, 0), new Vector3(2, 2, 0), new Vector3(2, 0, 0) };
        Color[] colors2 = { new Color(1, 0, 0), new Color(0, 1, 0), new Color(0, 0, 1), new Color(1, 0, 0) };

        Mesh m = new Mesh();
        m.SetVertices(vertices2);
        m.SetColors(colors2);
        m.SetIndices(new int[] { 0, 1, 2, 2, 3, 0 }, MeshTopology.Triangles, 0);
        m.RecalculateNormals();
        m.RecalculateBounds();
        m.RecalculateTangents();
        split_target.GetComponent<MeshFilter>().mesh = m;*/

        /*GameObject split_target = Instantiate(splitPrefab);
        split_target.name = "SplitTarget";

        Vector3[] vertices2 = { new Vector3(0, 0, 0), new Vector3(0, 2, 0), new Vector3(2, 2, 0), new Vector3(2, 0, 0) };
        Color[] colors2 = { new Color(0.5f, 0, 0, 1), new Color(0, 0.5f, 0, 1), new Color(0, 0, 0.5f, 1), new Color(0.8f, 0, 0, 1) };
        Vector2[] uv2 = { new Vector3(0, 0), new Vector3(0, 1), new Vector3(1, 0), new Vector3(0, 0) };

        Mesh m = new Mesh();
        m.SetVertices(vertices2);
        m.SetColors(colors2);
        m.SetIndices(new int[] { 0, 1, 2, 3 }, MeshTopology.Points, 0);
        split_target.GetComponent<MeshFilter>().mesh = m;*/

        Debug.Log(vertices.Count);

        GameObject split_target = Instantiate(splitPrefab);
        split_target.name = "SplitTarget";

        var indices = new int[vertices.Count];
        for (int i = 0; i < vertices.Count; i++)
            indices[i] = i;

        Mesh m = new Mesh();
        m.SetVertices(vertices);
        m.SetColors(colors);
        m.SetIndices(indices, MeshTopology.Points, 0, false);
        split_target.GetComponent<MeshFilter>().mesh = m;
    }
}
