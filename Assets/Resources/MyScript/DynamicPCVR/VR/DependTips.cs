using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.Extras;

public class DependTips : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject g;
    private MeshRenderer render;
    private bool hit_depend_sphere;

    public SteamVR_LaserPointer laser_script;

    void Start()
    {
        g = gameObject;
        render = g.GetComponent<MeshRenderer>();
        laser_script = GameObject.Find("[CameraRig]/Controller (right)").GetComponent<SteamVR_LaserPointer>();
        hit_depend_sphere = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (hit_depend_sphere)
        {
            if (Vector3.Distance(g.transform.position, laser_script.endPoint) > g.transform.localScale.x)
            {
                hit_depend_sphere = false;
                render.material.color = Color.yellow;
            }
            return;
        }

        if (laser_script.isHit)
        {
            if (Vector3.Distance(g.transform.position, laser_script.endPoint) < g.transform.localScale.x)
            {
                hit_depend_sphere = true;
                render.material.color = Color.green;
            }
        }

    }
}
