using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllRenderVRA : MonoBehaviour
{
    private MirrorControllerA mirrorController;
    // private LineMiddleFactory middleFactory;
    private MiddleFactoryVRA middleFactory;

    private List<GameObject> lineObjectList;
    private List<GameObject> rotationObjectList;
    private List<GameObject> pressObjectList;

    public Material segmentMaterial;
    public float segmentThickness = 0.01f;

    public GameObject RotateSymbol;
    public GameObject PressSymbol;

    // Start is called before the first frame update
    void Start()
    {
        mirrorController = GetComponentInParent<MirrorControllerA>();
        middleFactory = GetComponent<MiddleFactoryVRA>();
        lineObjectList = new List<GameObject>();
        rotationObjectList = new List<GameObject>();
        pressObjectList = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        RenderArrowMiddleFactory();
        // RenderRotation();
        // RenderPress();
    }

    /// <summary>
    /// render segmetn on VR client per frame
    /// </summary>
    private void RenderArrow()
    {
        // 1 arrow contained with 3 segment
        int n_curSegmentObj = lineObjectList.Count; // number of segment object
        int n_curArrow = n_curSegmentObj / 3; // number of segment
        int n_serverArrow = mirrorController.syncArrowList.Count; // number of latest segment list

        int delta = n_serverArrow - n_curArrow;
        // delete segment
        for (int i = 0; i > delta; --i)
        {
            for (int j = 0; j < 3; ++j)
            {
                GameObject tempObj = lineObjectList[lineObjectList.Count - 1];
                lineObjectList.RemoveAt(lineObjectList.Count - 1);
                Destroy(tempObj);
            }
        }
        // add new segment
        for (int i = 0; i < delta; ++i)
        {
            DPCArrow arrow = mirrorController.syncArrowList[n_curArrow + i];
            SegmentInfo segment = new SegmentInfo()
            {
                startPoint = arrow.startPoint,
                endPoint = arrow.endPoint
            };
            DrawSegment(segment);
            DrawArrow(segment);
        }
    }

    private void RenderArrowMiddleFactory()
    {
        int valid_line = 0;
        for (int i = 0; i < middleFactory.cue_list.Count; ++i)
        {
            if (middleFactory.cue_list[i].type == SmartCue.CueType.Line && middleFactory.cue_list[i].is_valid)
            {
                valid_line += 1;
            }
        }
        
        for (int i = 0; i < lineObjectList.Count; ++i)
        {
            lineObjectList[i].GetComponent<LineRenderer>().positionCount = 0;
        }

        for (int i = lineObjectList.Count; i < valid_line * 3; ++i)
        {
            AddNewEmptyLine();
        }

        int line_obj_indexs = 0;
        for (int i = 0; i < middleFactory.cue_list.Count; ++i)
        {
            
            // Debug.LogWarningFormat("size {0}, count {1}", middleFactory.cue_list.Count, i);
            SmartCue cue = middleFactory.cue_list[i];
            if (cue.type != SmartCue.CueType.Line || !cue.is_valid) continue;

            LineCue line = (LineCue)cue;
            SegmentInfo t = new SegmentInfo()
            {
                endPoint = line.GetEndPoint(),
                startPoint = line.GetStartPoint(),
            };

            DrawMiddleLine(t, 3 * line_obj_indexs);
            DrawMiddleArrow(t, 3 * line_obj_indexs);
            line_obj_indexs++;
        }
    }

    private void RenderRotation()
    {
        int n_curRotationObj = rotationObjectList.Count;
        int n_clientRotation = mirrorController.syncRotationList.Count;

        int delta = n_clientRotation - n_curRotationObj;
        // delete rotation 
        for (int i = 0; i > delta; --i)
        {
            GameObject tempObj = lineObjectList[n_clientRotation - 1 + i];
            rotationObjectList.Remove(tempObj);
            Destroy(tempObj);
        }
        // add new rotation
        for (int i = 0; i < delta; ++i)
        {
            DPCSymbol newRotation = mirrorController.syncRotationList[n_curRotationObj + i];

            GameObject tempObj = Instantiate(RotateSymbol);
            tempObj.transform.parent = transform;
            tempObj.transform.position = newRotation.position;
            tempObj.transform.forward = newRotation.up;
            rotationObjectList.Add(tempObj);
        }
    }

    private void RenderPress()
    {
        int n_curPressObj = pressObjectList.Count;
        int n_clientPress = mirrorController.syncPressList.Count;

        int delta = n_clientPress - n_curPressObj;
        // delete press 
        for (int i = 0; i > delta; --i)
        {
            GameObject tempObj = pressObjectList[n_clientPress - 1+i];
            pressObjectList.Remove(tempObj);
            Destroy(tempObj);
        }
        // add new press
        for (int i = 0; i < delta; ++i)
        {
            DPCSymbol newPress = mirrorController.syncPressList[n_curPressObj + i];
            GameObject tempObj = Instantiate(PressSymbol);
            tempObj.transform.parent = transform;
            tempObj.transform.position = newPress.position;
            tempObj.transform.right = newPress.up;
            pressObjectList.Add(tempObj);
        }
    }

    /// <summary>
    /// draw an segment
    /// </summary>
    /// <param name="segmentInfo"></param>
    private void DrawSegment(SegmentInfo segmentInfo)
    {
        GameObject segmentObj = new GameObject();
        segmentObj.transform.SetParent(this.transform);
        segmentObj.layer = LayerMask.NameToLayer("DepthCameraUnivisible");
        LineRenderer segmentRender = segmentObj.AddComponent<LineRenderer>();
        segmentRender.material = segmentMaterial;
        segmentRender.startWidth = segmentThickness;
        segmentRender.endWidth = segmentThickness;
        segmentRender.numCapVertices = 2;
        segmentRender.positionCount = 2;
        segmentRender.SetPosition(0, segmentInfo.startPoint);
        segmentRender.SetPosition(1, segmentInfo.endPoint);

        lineObjectList.Add(segmentObj);
    }

    /// <summary>
    /// draw two segment as an arrow
    /// </summary>
    /// <param name="segmentInfo"></param>
    private void DrawArrow(SegmentInfo segmentInfo)
    {
        Vector3 screenP1 = Camera.main.WorldToScreenPoint(segmentInfo.startPoint),
            screenP2 = Camera.main.WorldToScreenPoint(segmentInfo.endPoint);
        Vector2 dir = (screenP1 - screenP2).normalized;
        Vector2 verticalDir = new Vector2(-dir.y, dir.x);

        int length = 20;
        Vector3 screenArrowP1 = screenP2 + length * new Vector3(verticalDir.x, verticalDir.y) + length * new Vector3(dir.x, dir.y),
            screenArrowP2 = screenP2 - length * new Vector3(verticalDir.x, verticalDir.y) + length * new Vector3(dir.x, dir.y);

        Vector3 arrowP1 = Camera.main.ScreenToWorldPoint(screenArrowP1),
            arrowP2 = Camera.main.ScreenToWorldPoint(screenArrowP2);

        DrawSegment(new SegmentInfo()
        {
            startPoint = arrowP1,
            endPoint = segmentInfo.endPoint
        });

        DrawSegment(new SegmentInfo()
        {
            startPoint = arrowP2,
            endPoint = segmentInfo.endPoint
        });
    }

    private void AddNewEmptyLine()
    {
        GameObject segmentObj = new GameObject();
        segmentObj.transform.SetParent(this.transform);
        segmentObj.layer = LayerMask.NameToLayer("DepthCameraUnivisible");
        LineRenderer segmentRender = segmentObj.AddComponent<LineRenderer>();
        segmentRender.material = segmentMaterial;
        segmentRender.startWidth = segmentThickness;
        segmentRender.endWidth = segmentThickness;
        segmentRender.positionCount = 0;

        lineObjectList.Add(segmentObj);
    }

    private void DrawMiddleLine(SegmentInfo segmentInfo, int index)
    {
        lineObjectList[index].GetComponent<LineRenderer>().positionCount = 2;
        lineObjectList[index].GetComponent<LineRenderer>().SetPosition(0, segmentInfo.startPoint);
        lineObjectList[index].GetComponent<LineRenderer>().SetPosition(1, segmentInfo.endPoint);
    }

    private void DrawMiddleArrow(SegmentInfo segmentInfo, int index)
    {
        Vector3 screenP1 = Camera.main.WorldToScreenPoint(segmentInfo.startPoint),
            screenP2 = Camera.main.WorldToScreenPoint(segmentInfo.endPoint);
        Vector2 dir = (screenP1 - screenP2).normalized;
        Vector2 verticalDir = new Vector2(-dir.y, dir.x);

        int length = 20;
        Vector3 screenArrowP1 = screenP2 + length * new Vector3(verticalDir.x, verticalDir.y) + length * new Vector3(dir.x, dir.y),
            screenArrowP2 = screenP2 - length * new Vector3(verticalDir.x, verticalDir.y) + length * new Vector3(dir.x, dir.y);

        Vector3 arrowP1 = Camera.main.ScreenToWorldPoint(screenArrowP1),
            arrowP2 = Camera.main.ScreenToWorldPoint(screenArrowP2);

        DrawMiddleLine(new SegmentInfo()
        {
            startPoint = arrowP1,
            endPoint = segmentInfo.endPoint
        }, index + 1);

        DrawMiddleLine(new SegmentInfo()
        {
            startPoint = arrowP2,
            endPoint = segmentInfo.endPoint
        }, index + 2);
    }

    
}
