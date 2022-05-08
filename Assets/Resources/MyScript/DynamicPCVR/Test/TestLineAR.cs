using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLineAR : MonoBehaviour
{
    private TestMirror mirrorController; // ÍøÂçÖÐ¿Ø
    private List<GameObject> lines;
    private int line_index;

    public Material straightLineMaterial;
    public float straightLineThickness = 0.004f;

    public bool origin_line = false;

    // Start is called before the first frame update
    void Start()
    {
        mirrorController = GetComponent<TestMirror>();
        lines = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        line_index = 0;
        for (int i = 0; i < mirrorController.syncArrowList.Count; ++i)
        {
            DPCArrow current_line = mirrorController.syncArrowList[i];
            DrawLine(ref current_line);
        }
        ClearLine();
    }

    void DrawLine(ref DPCArrow currend_line)
    {
        if (origin_line)
        {
            for (int i = 0; i < 3; ++i)
            {
                if (line_index >= lines.Count)
                {
                    lines.Add(CreateNewLine("line" + line_index.ToString()));
                }

                if (i == 2)
                {
                    lines[line_index].GetComponent<LineRenderer>().positionCount = 2;
                    lines[line_index].GetComponent<LineRenderer>().SetPosition(0, currend_line.startPoint);
                    lines[line_index].GetComponent<LineRenderer>().SetPosition(1, currend_line.endPoint);
                }
                else
                {
                    lines[line_index].GetComponent<LineRenderer>().positionCount = currend_line.curvePointList[i].Length;
                    lines[line_index].GetComponent<LineRenderer>().SetPositions(currend_line.curvePointList[i]);
                }
                ++line_index;
            }
        }
        else
        {
            foreach (var t in currend_line.curvePointList)
            {
                if (line_index >= lines.Count)
                {
                    lines.Add(CreateNewLine("line" + line_index.ToString()));
                }

                lines[line_index].GetComponent<LineRenderer>().positionCount = t.Length;
                lines[line_index].GetComponent<LineRenderer>().SetPositions(t);
                ++line_index;
            }
        }

    }

    void ClearLine()
    {
        while (line_index < lines.Count)
        {
            lines[line_index++].GetComponent<LineRenderer>().positionCount = 0;
        }
    }


    private GameObject CreateNewLine(string objName)
    {
        GameObject lineObj = new GameObject(objName);
        lineObj.transform.SetParent(this.transform);
        LineRenderer curveRender = lineObj.AddComponent<LineRenderer>();
        curveRender.material = straightLineMaterial;
        lineObj.layer = LayerMask.NameToLayer("DepthCameraUnivisible"); ;


        curveRender.startWidth = straightLineThickness;
        curveRender.endWidth = straightLineThickness;
        return lineObj;
    }
}
