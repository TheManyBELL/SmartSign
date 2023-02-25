using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

public class Exp : MonoBehaviour
{
    // 积木场景需要记录的数据
    public float walking_dis;
    public int is_wrong;
    public GameObject ar_camera;
    private DateTime task_start_time, task_end_time;
    private Vector3 last_ar_pos;
    // 装配场景需要记录的数据
    public List<GameObject> dot_objs;
    private string dots_init_rot, dots_end_rot;
    private Vector3 init_rotation;
    private Vector3 end_rotation;
    private DateTime vr_start_time, ar_start_time;
    // 同步异步需要记录的数据
    private DateTime mode_vr_start_time, mode_ar_start_time;
    public bool asynchronous_ar_begin = false;

    // 实验者姓名，
    public String exper_name;
    public enum Condition { CG1 = 0, CG2, EG };
    public Condition condition = Condition.CG1;
    public enum Task { BLOCK = 0, ASSEMBLY, MODE };     // BLOCK积木平移 ASSEMBLY装配旋转 MODE俩场景同步或异步
    public Task task = Task.BLOCK;

    private bool initial_exp_start = true;     // 实验是否是初次启动
    private bool vrExpStart;    // Vr端操作是否开始  ;
    private MiddleFactoryVRA middle;

    // Start is called before the first frame update
    void Start()
    {
        walking_dis = 0;
        middle = gameObject.GetComponent<MiddleFactoryVRA>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!ar_camera)
        {
            // ar_camera = GameObject.Find("DepthCameraAR(Clone)");
            ar_camera = Camera.main.gameObject;
        }

        if (!vrExpStart && ar_camera && !initial_exp_start)
        {
            walking_dis += Vector2.Distance(new Vector2(ar_camera.transform.position.x, ar_camera.transform.position.z), new Vector2(last_ar_pos.x, last_ar_pos.z));
        }
        last_ar_pos = ar_camera.transform.position;
    }


    private void writeFile(string s)
    {
        string dir = "Data/";
        
        string exp_dir = "Block/";
        if (task == Task.ASSEMBLY) exp_dir = "Assembly/";
        else if (task == Task.MODE) exp_dir = "Mode/";

        string ExpType_dir = "CG1/";
        if (condition == Condition.CG2) ExpType_dir = "CG2/";
        if (condition == Condition.EG) ExpType_dir = "EG/";

        if (task == Task.MODE) { 
            if (middle.type == MiddleFactoryVRA.Type.同步)
            {
                ExpType_dir = "CG1/";
            }
            else
            {
                ExpType_dir = "EG/";
            }
        } 

        string file_dir = dir + exp_dir + ExpType_dir + exper_name + ".csv";

        StreamWriter wf = File.AppendText(file_dir);

        wf.WriteLine(s);
        wf.Flush();
        wf.Close();

        Debug.Log("write success");
    }

    public void VRBeginAREnd()
    {
        vrExpStart = true;
        vr_start_time = DateTime.Now;

        if (initial_exp_start)
        {
            initial_exp_start = false;
            task_start_time = DateTime.Now;
            mode_vr_start_time = DateTime.Now;
            return;
        }

        if (task == Task.ASSEMBLY)
        {
            dots_end_rot = "";
            foreach (var o in dot_objs) dots_end_rot += o.transform.eulerAngles.ToString("f4");
            DateTime ar_end_time = DateTime.Now;
            TimeSpan ar_ope_time = ar_end_time.Subtract(ar_start_time).Duration();
            writeFile("ar:" + "," + ar_ope_time.TotalMilliseconds.ToString() + "," + dots_init_rot + "," + dots_end_rot);
        }
        else if (task == Task.BLOCK)
        {
            task_end_time = DateTime.Now;
            TimeSpan task_total_time = task_end_time.Subtract(task_start_time).Duration();
            writeFile("task complete time:" + "," + task_total_time.TotalMilliseconds.ToString() + "," + walking_dis.ToString("f4"));
        }
        else if (middle.type == MiddleFactoryVRA.Type.异步 && task == Task.MODE && asynchronous_ar_begin)
        {
            TimeSpan ar_total_time = DateTime.Now.Subtract(mode_ar_start_time).Duration();
            writeFile("ar complete time:" + "," + ar_total_time.TotalMilliseconds.ToString());
        }
        else if (middle.type == MiddleFactoryVRA.Type.同步 && task == Task.MODE)
        {
            DateTime ar_end_time = DateTime.Now;
            TimeSpan ar_ope_time = ar_end_time.Subtract(ar_start_time).Duration();
            writeFile("ar ope time:" + "," + ar_ope_time.TotalMilliseconds.ToString());
        }
    }

    public void VREndARBegin()
    {
        vrExpStart = false;
        ar_start_time = DateTime.Now;

        if (task == Task.ASSEMBLY)
        {       
            dots_init_rot = "";
            foreach (var o in dot_objs) dots_init_rot += o.transform.eulerAngles.ToString("f4");

            DateTime vr_end_time = DateTime.Now;
            TimeSpan vr_ope_time = vr_end_time.Subtract(vr_start_time).Duration();
            writeFile("vr:" + "," + vr_ope_time.TotalMilliseconds.ToString() + "," + init_rotation.ToString("f4") + "," + end_rotation.ToString("f4"));
        }
        else if (task == Task.BLOCK)
        {
            last_ar_pos = ar_camera.transform.position;
        }
        else if (middle.type == MiddleFactoryVRA.Type.异步 && task == Task.MODE)
        {
            TimeSpan vr_total_time = DateTime.Now.Subtract(mode_vr_start_time).Duration();
            writeFile("vr complete time:" + "," + vr_total_time.TotalMilliseconds.ToString());
        }
        else if (middle.type == MiddleFactoryVRA.Type.同步 && task == Task.MODE)
        {
            DateTime vr_end_time = DateTime.Now;
            TimeSpan vr_ope_time = vr_end_time.Subtract(vr_start_time).Duration();
            writeFile("vr ope time:" + "," + vr_ope_time.TotalMilliseconds.ToString());
        }
    }

    public void AsynchronousARBegin()
    {
        mode_ar_start_time = DateTime.Now;
    }

    public void RecordObjInitRot(Vector3 init_rot)
    {
        init_rotation = init_rot;
    }

    public void RecordObjEndRot(Vector3 end_rot)
    {
        end_rotation = end_rot;
    }

    public bool GetVRExpState() => vrExpStart;
}
