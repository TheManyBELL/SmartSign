using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class TestUntilDieDYVR : MonoBehaviour
{
    private Vector3 p1, p2;
    public Button LineButton;
    private enum State
    {
        Inactive = 0, SelectPosition, SelectRotation, SelectP1, SelectP2
    };
    private State nowState = 0;
    private int currentLineIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        LineButton = GameObject.Find("TestObj/Canvas/Line").GetComponent<Button>();
        LineButton.onClick.AddListener(ActivateLine);
    }

    // Update is called once per frame
    void Update()
    {
        if (GlobleInfo.ClientMode.Equals(CameraMode.AR))
        {
            return;
        }

        if (nowState == State.Inactive)
        {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        
        // line 
        if (nowState == State.SelectP1)
        {
            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    p1 = hitInfo.point + 0.01f * hitInfo.normal;
                    nowState = State.SelectP2;
                    Debug.Log("p1 " + p1);
                }

            }
        }
        else if (nowState == State.SelectP2)
        {
            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    p2 = hitInfo.point + 0.01f * hitInfo.normal;
                    /*GameObject.Find("TestObj").GetComponent<RayDisocclusion>().segments.Add(new SegmentInfo()
                    {
                        startPoint = p1,
                        endPoint = p2
                    });*/
                    GameObject.Find("SmartSignA(Clone)").GetComponent<MirrorControllerA>().syncArrowList.Add(new DPCArrow()
                    {
                        index = currentLineIndex++,
                        startPoint = p1,
                        endPoint = p2,
                        curvePointList = new List<Vector3[]>()
                    }); ;
                    nowState = State.Inactive;
                    Debug.Log("p2 " + p2);
                }
            }
        }
    }

    private void ActivateLine()
    {
        Debug.Log("line");
        nowState = State.SelectP1;
    }
}
