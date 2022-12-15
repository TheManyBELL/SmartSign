using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class SmartCue : MonoBehaviour
{
    public enum CueType { Line = 0, Axes, Split };
    public CueType type;
    public bool is_synchronous;
    public bool is_valid;
    public int sync_list_index;  // cmp sync list

    public SmartCue()
    {
        is_synchronous = false;
        is_valid = true;
    }

    public static void DestroyGameObject(GameObject t)
    {
        if (!t) return;

        int j = 0;
        while (j < t.transform.childCount)
        {
            Destroy(t.transform.GetChild(j++).gameObject);
        }
        Destroy(t);
    }

    public virtual void Synchronize(MirrorControllerA my_controller) {
        Debug.LogWarning("Synchronize to AR");

        is_synchronous = true;
    }

    public virtual void StopSynchronize(MirrorControllerA my_controller)
    {
        Debug.LogWarning("Stop Synchronize");

        is_synchronous = false;
    }

    public virtual void UpdateSynchronize(MirrorControllerA my_controller)
    {
        Debug.LogWarning("Update Synchronize");
    }

    public virtual void Remove(MirrorControllerA my_controller) {
        Debug.LogWarning("Remove");

        is_valid = false;
    }


}

public class LineCue : SmartCue
{
    private Vector3 p1, p2;
    
    public LineCue(Vector3 p1, Vector3 p2)
    {
        this.p1 = p1;
        this.p2 = p2;
        type = CueType.Line;
    }

    public override void Synchronize(MirrorControllerA my_controller)
    {
        base.Synchronize(my_controller);

        my_controller.CmdAddDPCArrow(new DPCArrow()
        {
            index = my_controller.syncArrowList.Count,
            startPoint = p1,
            endPoint = p2,
            curvePointList = new List<Vector3[]>(),
            originPointList = new List<Vector3[]>(),
        });

        sync_list_index = my_controller.syncArrowList.Count;
    }

    public override void StopSynchronize(MirrorControllerA my_controller)
    {
        base.StopSynchronize(my_controller);

        my_controller.CmdDeleteDPCArrow();
    }

    public override void UpdateSynchronize(MirrorControllerA my_controller)
    {
        base.UpdateSynchronize(my_controller);

        if (!is_synchronous) return;

        my_controller.CmdUpdateDPCArrow(new DPCArrow
        {
            index = sync_list_index,
            startPoint = p1,
            endPoint = p2,
            curvePointList = my_controller.syncArrowList[sync_list_index].curvePointList,
            originPointList = my_controller.syncArrowList[sync_list_index].originPointList,
            startPointVisibility = my_controller.syncArrowList[sync_list_index].startPointVisibility,
        });
    }

    public override void Remove(MirrorControllerA my_controller)
    {
        base.Remove(my_controller);

        if (is_synchronous) StopSynchronize(my_controller);
    }

    public void SetStartPoint(Vector3 p1)
    {
        this.p1 = p1;
    }

    public void SetEndPoint(Vector3 p2)
    {
        this.p2 = p2;
    }

    public Vector3 GetStartPoint()
    {
        return p1;
    }

    public Vector3 GetEndPoint()
    {
        return p2;
    }
}

public class AxesCue : SmartCue
{
    private GameObject initial_axes;
    private GameObject final_axes;

    public AxesCue(GameObject initial_axes, GameObject final_axes)
    {
        this.initial_axes = initial_axes;
        this.final_axes = final_axes;
        type = CueType.Axes;
    }

    public override void Synchronize(MirrorControllerA my_controller)
    {
        base.Synchronize(my_controller);

        my_controller.CmdAddDPCAxes(new DPCAxes()
        {
            index = my_controller.syncAxesList.Count,
            init_position = initial_axes.transform.position,
            init_rotation = initial_axes.transform.rotation,
            end_position = initial_axes.transform.position,
            end_rotation = initial_axes.transform.rotation,
            // correspondingLineIndex = autoGenerateLine ? my_controller.syncArrowList.Count : -1,
            correspondingLineIndex = -1
        });

        sync_list_index = my_controller.syncAxesList.Count;
    }

    public override void UpdateSynchronize(MirrorControllerA my_controller)
    {
        base.UpdateSynchronize(my_controller);

        if (!is_synchronous) return;
        my_controller.CmdUpdateDPCAxes(new DPCAxes()
        {
            index = sync_list_index,
            init_position = initial_axes.transform.position,
            init_rotation = initial_axes.transform.rotation,
            end_position = initial_axes.transform.position,
            end_rotation = initial_axes.transform.rotation,
            correspondingLineIndex = -1
        });
    }

    public override void StopSynchronize(MirrorControllerA my_controller)
    {
        base.StopSynchronize(my_controller);

        my_controller.CmdDeleteDPCAxes();
    }

    public override void Remove(MirrorControllerA my_controller)
    {
        base.Remove(my_controller);

        DestroyGameObject(initial_axes);
        DestroyGameObject(final_axes);

        if (is_synchronous) StopSynchronize(my_controller);
    }
}

public class CutpieceCue : SmartCue
{
    private Vector3 center;
    private List<List<Vector3>> vertices;
    private List<List<Color>> color;

    private GameObject split_obj;

    public CutpieceCue(Vector3 center, List<List<Vector3>> vertices, List<List<Color>> color, GameObject split_obj)
    {
        this.center = center;
        this.vertices = vertices;
        this.color = color;
        this.split_obj = split_obj;
        type = CueType.Split;
    }

    public override void Synchronize(MirrorControllerA my_controller)
    {
        base.Synchronize(my_controller);

        my_controller.CmdAddDPCSplitMesh(new DPCSplitMesh()
        {
            index = my_controller.syncSplitMeshList.Count,
            center = center,
            color = color,
            vertices = vertices,
        });

        my_controller.CmdAddDPCSplitPos(new DPCSplitPosture()   
        {
            index = my_controller.syncSplitPosList.Count,
            valid = true,
            position = split_obj.transform.position,
            rotation = split_obj.transform.rotation,
            // correspondingLineIndex = autoGenerateLine ? myController.syncArrowList.Count : -1,
            correspondingLineIndex = -1
        });

        sync_list_index = my_controller.syncSplitMeshList.Count;
    }

    public override void UpdateSynchronize(MirrorControllerA my_controller)
    {
        base.UpdateSynchronize(my_controller);

        if (!is_synchronous) return;

        my_controller.CmdUpdateDPCSplitPos(new DPCSplitPosture()     
        {
            index = sync_list_index,
            valid = true,
            position = split_obj.transform.position,
            rotation = split_obj.transform.rotation,
            // correspondingLineIndex = autoGenerateLine ? myController.syncArrowList.Count : -1,
            correspondingLineIndex = -1
        });
    }

    public override void StopSynchronize(MirrorControllerA my_controller)
    {
        base.StopSynchronize(my_controller);

        my_controller.CmdDeleteDPCSplitMesh();
        my_controller.CmdDeleteDPCSplitPos();
    }

    public override void Remove(MirrorControllerA my_controller)
    {
        base.Remove(my_controller);
        DestroyGameObject(split_obj);

        if (is_synchronous) StopSynchronize(my_controller);
    }
}