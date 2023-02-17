using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMirror : MonoBehaviour
{
    public List<DPCArrow> syncArrowList;
    public List<DPCSymbol> syncRotationList;
    public List<DPCSymbol> syncPressList;

    // Start is called before the first frame update
    void Start()
    {
        syncArrowList = new List<DPCArrow>();
        syncRotationList = new List<DPCSymbol>();
        syncPressList = new List<DPCSymbol>();
}

    // Update is called once per frame
    void Update()
    {
        
    }   

    public void CmdUpdateDPCArrow(DPCArrow newArrow)
    {
        syncArrowList[newArrow.index] = newArrow;
        Debug.Log("[server] arrow " + newArrow.index + " updated");
    }

    public void CmdAddDPCArrow(DPCArrow newArrow)
    {
        syncArrowList.Add(newArrow);

        Debug.Log("[server] arrow added:" + syncArrowList.Count);

    }

    public void CmdAddDPCRotation(DPCSymbol newRotation)
    {
        syncRotationList.Add(newRotation);

        Debug.Log("[server] arrow added:" + syncRotationList.Count);
    }
    public void CmdUpdateDPCRotation(DPCSymbol newRotation)
    {
        syncRotationList[newRotation.index] = newRotation;
        Debug.Log("[server] rotation " + newRotation.index + " updated");
    }

    public void CmdAddDPCPress(DPCSymbol newPress)
    {
        syncPressList.Add(newPress);
        Debug.Log("server: arrow added:" + syncPressList.Count);

    }

    public void CmdUpdateDPCPress(DPCSymbol newPress)
    {
        syncPressList[newPress.index] = newPress;
        Debug.Log("[server] press " + newPress.index + " updated");
    }
}
