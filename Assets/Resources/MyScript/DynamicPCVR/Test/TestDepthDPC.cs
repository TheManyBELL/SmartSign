using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TestDepthDPC : MonoBehaviour
{
    // Start is called before the first frame update
    private Camera m_Camera;
    public RenderTexture depthTexture;
    public Texture2D depthTextureRead;
    public Material Mat;

    // 另一种方法
    public RenderTexture colorRT;
    public RenderTexture depthRT;
    public RenderTexture test;

    // 
    public bool origin = false;


    private void Awake()
    {

    }

    void Start()
    {
        //GameObject.Find("Diagnostics").SetActive(false);
        //if (GlobleInfo.ClientMode.Equals(CameraMode.VR)) { return; }
        m_Camera = gameObject.GetComponent<Camera>();
        m_Camera.depthTextureMode = DepthTextureMode.Depth;
        depthTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.RFloat);

        // point cloud depth
        colorRT = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.Default);
        depthRT = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.Depth);


        test = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.RFloat);
        // depth to read
        depthTextureRead = new Texture2D(Screen.width, Screen.height, TextureFormat.RFloat, true);

    }


    void OnPostRender()
    {
        if (!origin) return;

        RenderTexture source = m_Camera.activeTexture;
        Graphics.Blit(source, depthTexture, Mat);

        RenderTexture currentActiveRT = RenderTexture.active;
        RenderTexture.active = depthTexture;
        depthTextureRead.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        RenderTexture.active = currentActiveRT;
    }

    // 另一种方法
    private void OnPreRender()
    {
        if (origin) return;

        var depthBufferSize = new Vector2Int(depthRT.width, depthRT.height);
        var targetSize = new Vector2Int(colorRT.width, colorRT.height);
        if (targetSize != depthBufferSize)
        {
            Debug.Log($"Target {colorRT} has a buffer size of {targetSize}, which mismatches depth buffer size of {depthBufferSize}.");
        }
        m_Camera.SetTargetBuffers(colorRT.colorBuffer, depthRT.depthBuffer);
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (origin) return;


        Graphics.Blit(depthRT, test);

        RenderTexture currentActiveRT = RenderTexture.active;
        RenderTexture.active = test;
        depthTextureRead.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        RenderTexture.active = currentActiveRT;
    }

    public float GetDepth(int x, int y)
    {
        float d = depthTextureRead.GetPixel(x, y).r;
        // Debug.Log(d);
        if (!origin)
        {
            float zc0 = 1.0f - m_Camera.farClipPlane / m_Camera.nearClipPlane;
            float zc1 = m_Camera.farClipPlane / m_Camera.nearClipPlane;
            d = 1.0f / (zc0 * (1.0f - d) + zc1);
        }

        return d;
    }

    void Update()
    {
        // Debug.Log("depth");
        this.transform.position = Camera.main.transform.position;
        this.transform.rotation = Camera.main.transform.rotation;
    }
}