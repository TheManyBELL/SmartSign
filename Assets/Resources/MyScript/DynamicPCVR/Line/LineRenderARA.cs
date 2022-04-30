using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRenderARA : MonoBehaviour
{
    private MirrorControllerA mirrorController; // ÍøÂçÖÐ¿Ø
    private List<GameObject> lines;
    private int line_index;

    public Material straightLineMaterial;
    public float straightLineThickness = 0.01f;

    // Start is called before the first frame update
    void Start()
    {
        mirrorController = GetComponentInParent<MirrorControllerA>();
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
        foreach (var t in currend_line.curvePointList)
        {
            if (line_index >= lines.Count)
            {
                lines.Add(CreateNewLine("line" + line_index.ToString()));
            }
            Debug.Log(t.Length);
            lines[line_index].GetComponent<LineRenderer>().positionCount = t.Length;
            lines[line_index].GetComponent<LineRenderer>().SetPositions(t);
            ++line_index;
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

        curveRender.startWidth = straightLineThickness;
        curveRender.endWidth = straightLineThickness;
        return lineObj;
    }
}
