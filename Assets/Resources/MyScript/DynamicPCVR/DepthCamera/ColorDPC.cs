using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ColorDPC : MonoBehaviour
{
    // Start is called before the first frame update
    private Camera m_Camera;
    public RenderTexture depthTexture;
    public Material Mat;

    // 另一种方法
    public RenderTexture colorRT;
    public RenderTexture depthRT;

    public Texture2D colorTextureRead;

    // 使用哪种
    public bool origin = false;


    private void Awake()
    {

    }

    void Start()
    {
        m_Camera = gameObject.GetComponent<Camera>();
        m_Camera.depthTextureMode = DepthTextureMode.Depth;

        // point cloud depth
        colorRT = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.Default);
        depthRT = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.Depth);
        colorTextureRead = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBAFloat, true);
    }


    // 另一种方法
    private void OnPreRender()
    {
        m_Camera.SetTargetBuffers(colorRT.colorBuffer, depthRT.depthBuffer);
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        RenderTexture currentActiveRT = RenderTexture.active;
        RenderTexture.active = colorRT;
        colorTextureRead.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        RenderTexture.active = currentActiveRT;
    }

    public Color GetColor(int x, int y)
    {
        Color c = colorTextureRead.GetPixel(x, y);
        return c;
    }

    void Update()
    {
    }
}