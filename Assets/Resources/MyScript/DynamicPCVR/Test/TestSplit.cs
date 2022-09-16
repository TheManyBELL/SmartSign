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

    void SplitCPU(List<Vector3> points)
    {
        List<Vector3> plane_points = new List<Vector3>();
        foreach (Vector3 p in points)
        {
            plane_points.Add(globalUtils.MWorldToScreenPointDepth(p));
        }

    }

    void SplitGPU(List<Vector3> points)
    {

    }
}
