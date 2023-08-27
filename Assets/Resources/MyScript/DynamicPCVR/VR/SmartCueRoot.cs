using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Depend
{
    public enum DependType { start_start = 0, start_end, end_start, end_end };

    public bool is_depend;
    public List<KeyValuePair<int, DependType>> depend_list;

    public Depend(bool is_depend)
    {
        this.is_depend = is_depend;
        this.depend_list = new List<KeyValuePair<int, DependType>>();
    }

    public void AddDepend(int index, DependType type)
    {
        is_depend = true;
        depend_list.Add(new KeyValuePair<int, DependType>(index, type));
    }
};

public enum CueType { None = 0, Line, Axes, Split };


public class SmartCue
{
    public CueType type;
    public bool is_synchronous;
    public bool is_valid;   // render in VR
    public int sync_list_index;  // cmp sync list

    public SmartCue()
    {
        is_synchronous = false;
        is_valid = true;

        Debug.LogWarning("base");
    }

    public bool IsLine()
    {
        return type == CueType.Line;
    }

    public static void DestroyGameObject(GameObject t)
    {
        if (!t) return;

        int j = 0;
        while (j < t.transform.childCount)
        {
            GameObject.Destroy(t.transform.GetChild(j++).gameObject);
        }
        GameObject.Destroy(t);
    }

    public virtual void Synchronize(MirrorControllerA my_controller) {
        Debug.LogWarning("Synchronize to AR");

        is_valid = true;
        is_synchronous = true;
    }

    public virtual void StopSynchronize(MirrorControllerA my_controller, bool is_valid)
    {
        Debug.LogWarning("Stop Synchronize");

        is_synchronous = false;
        this.is_valid = is_valid;
    }

    public virtual void UpdateSynchronize(MirrorControllerA my_controller)
    {
        Debug.LogWarning("Update Synchronize");
    }

    public virtual void Remove(MirrorControllerA my_controller) {
        Debug.LogWarning("Remove");

    }


}

public class LineCue : SmartCue
{
    public Vector3 p1, p2;
    public Depend depend;
    public Ray ray;

    private GameObject start_depend_sphere;
    private GameObject end_depend_sphere;

    public LineCue(Vector3 p1, Vector3 p2, GameObject prefab = null) : base()
    {
        this.p1 = p1;
        this.p2 = p2;
        type = CueType.Line;
        depend = new Depend(false);
        if (prefab) start_depend_sphere = GameObject.Instantiate(prefab);
        if (prefab) end_depend_sphere = GameObject.Instantiate(prefab);
        if (prefab) start_depend_sphere.transform.position = p1;
        if (prefab) end_depend_sphere.transform.position = p2;
    }

    public override void Synchronize(MirrorControllerA my_controller)
    {
        base.Synchronize(my_controller);

        sync_list_index = my_controller.syncArrowList.Count;

        my_controller.CmdAddDPCArrow(new DPCArrow()
        {
            index = my_controller.syncArrowList.Count,
            startPoint = p1,
            endPoint = p2,
            curvePointList = new List<Vector3[]>(),
            originPointList = new List<Vector3[]>(),
        });

        if (start_depend_sphere) start_depend_sphere.SetActive(true);
        if (end_depend_sphere) end_depend_sphere.SetActive(true);
    }

    public override void StopSynchronize(MirrorControllerA my_controller, bool is_valid)
    {
        base.StopSynchronize(my_controller, is_valid);

        my_controller.CmdDeleteDPCArrow();

        if (start_depend_sphere) start_depend_sphere.SetActive(false);
        if (end_depend_sphere) end_depend_sphere.SetActive(false);
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

        if (start_depend_sphere) GameObject.Destroy(start_depend_sphere);
        if (end_depend_sphere) GameObject.Destroy(end_depend_sphere);
        if (is_synchronous) StopSynchronize(my_controller, false);
    }

    public void SetStartPoint(Vector3 p1)
    {
        this.p1 = p1;
        if (start_depend_sphere) start_depend_sphere.transform.position = p1;
    }

    public void SetEndPoint(Vector3 p2)
    {
        this.p2 = p2;
        if (end_depend_sphere) end_depend_sphere.transform.position = p2;
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
            end_position = final_axes.transform.position,
            end_rotation = final_axes.transform.rotation,
            // correspondingLineIndex = autoGenerateLine ? my_controller.syncArrowList.Count : -1,
            correspondingLineIndex = -1
        });

        initial_axes.SetActive(true);
        final_axes.SetActive(true);

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
            end_position = final_axes.transform.position,
            end_rotation = final_axes.transform.rotation,
            correspondingLineIndex = -1
        });
    }

    public override void StopSynchronize(MirrorControllerA my_controller, bool is_valid)
    {
        base.StopSynchronize(my_controller, is_valid);

        initial_axes.SetActive(is_valid);
        final_axes.SetActive(is_valid);
        my_controller.CmdDeleteDPCAxes();
    }

    public override void Remove(MirrorControllerA my_controller)
    {
        base.Remove(my_controller);

        DestroyGameObject(initial_axes);
        DestroyGameObject(final_axes);

        if (is_synchronous) StopSynchronize(my_controller, false);
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
        this.vertices = new List<List<Vector3>>(vertices);
        this.color = new List<List<Color>>(color);
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

        split_obj.SetActive(true);
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

    public override void StopSynchronize(MirrorControllerA my_controller, bool is_valid)
    {
        base.StopSynchronize(my_controller, is_valid);

        split_obj.SetActive(is_valid);
        my_controller.CmdDeleteDPCSplitMesh();
        my_controller.CmdDeleteDPCSplitPos();
    }

    public override void Remove(MirrorControllerA my_controller)
    {
        if (is_synchronous) StopSynchronize(my_controller, false);

        base.Remove(my_controller);
        DestroyGameObject(split_obj);
    }

    public void DebugLog()
    {
        

    }
}