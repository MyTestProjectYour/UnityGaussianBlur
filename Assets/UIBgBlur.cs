using UnityEngine;
using UnityEngine.UI;

public class UIBgBlur : MonoBehaviour
{
    // 高斯模糊迭代次数
    [Range(0, 4)]
    public int Iterations = 3;
    // 每次迭代纹理坐标偏移的速度
    [Range(0.2f, 3.0f)]
    public float BlurSpread = 0.6f;
    // 降采样比率 
    [Range(1, 8)]
    public int DownSample = 2;
    private RawImage _blurImage;
    private RenderTexture _srcTexture, _destTexture;
    private Texture2D _texture2D;
    private Material _material;
    private void Awake()
    {
        _blurImage = transform.Find("RawImage").GetComponent<RawImage>();
        _material = _blurImage.material;
        _blurImage.material = null;

        _srcTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
        _destTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
    }

    public void SetSortingOrder(int order)
    {
        Canvas blurCanvas = GetComponent<Canvas>();
        blurCanvas.overrideSorting = true;
        blurCanvas.sortingOrder = order;
    }

    public void SetEnanble(bool isOK)
    {
        Camera camera = Camera.main;
        _blurImage.gameObject.SetActive(isOK && camera != null);
        if (isOK)
            DrawCameraBlur(camera);
    }

    /// <summary>
    /// 绘制相机的图片
    /// </summary> 
    private void DrawCameraBlur(Camera camera)
    {

        if (camera == null)
            return;
        //保存相机渲染用于后面恢复
        var cameraPrevT = camera.targetTexture;
        camera.targetTexture = _srcTexture;
        camera.Render();
        // 还原相机渲染目标
        camera.targetTexture = cameraPrevT;
        OnRender(_srcTexture, _destTexture);
        // 设置当前活动的渲染纹理
        RenderTexture.active = _destTexture;
        //创建一个Texture2D
        if (_texture2D == null)
            _texture2D = new Texture2D(_destTexture.width, _destTexture.height, TextureFormat.RGB24, false);
        var texRect = new Rect(0, 0, _destTexture.width, _destTexture.height);
        _texture2D.ReadPixels(texRect, 0, 0);
        _texture2D.Apply();
        // 清空
        RenderTexture.active = null;
        // 赋值给RawImage
        _blurImage.texture = _texture2D;
    }

    /// <summary>
    /// 用Shader渲染高斯模糊
    /// </summary> 
    private void OnRender(RenderTexture src, RenderTexture dest)
    {
        if (_material != null)
        {
            // 降采样的纹理宽度
            int rtW = src.width / DownSample;
            // 降采样的纹理高度
            int rtH = src.height / DownSample;
            RenderTexture buffer0 = RenderTexture.GetTemporary(rtW, rtH, 0);
            // 滤波模式设置为双线性
            buffer0.filterMode = FilterMode.Bilinear;
            Graphics.Blit(src, buffer0);
            for (int i = 0; i < Iterations; i++)
            {
                // 设置模糊尺寸(纹理坐标的偏移量)
                _material.SetFloat("_BlurSize", 1.0f + i * BlurSpread);
                RenderTexture buffer1 = RenderTexture.GetTemporary(rtW, rtH, 0);
                // 渲染垂直的Pass
                Graphics.Blit(buffer0, buffer1, _material, 0);
                RenderTexture.ReleaseTemporary(buffer0);
                buffer0 = buffer1;
                buffer1 = RenderTexture.GetTemporary(rtW, rtH, 0);
                // 渲染水平的Pass
                Graphics.Blit(buffer0, buffer1, _material, 1);
                RenderTexture.ReleaseTemporary(buffer0);
                buffer0 = buffer1;
            }
            Graphics.Blit(buffer0, dest);
            RenderTexture.ReleaseTemporary(buffer0);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }
}