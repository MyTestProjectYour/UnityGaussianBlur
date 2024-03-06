using UnityEngine;
using UnityEngine.UI;

public class UIBgBlur : MonoBehaviour
{
    // ��˹ģ����������
    [Range(0, 4)]
    public int Iterations = 3;
    // ÿ�ε�����������ƫ�Ƶ��ٶ�
    [Range(0.2f, 3.0f)]
    public float BlurSpread = 0.6f;
    // ���������� 
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
    /// ���������ͼƬ
    /// </summary> 
    private void DrawCameraBlur(Camera camera)
    {

        if (camera == null)
            return;
        //���������Ⱦ���ں���ָ�
        var cameraPrevT = camera.targetTexture;
        camera.targetTexture = _srcTexture;
        camera.Render();
        // ��ԭ�����ȾĿ��
        camera.targetTexture = cameraPrevT;
        OnRender(_srcTexture, _destTexture);
        // ���õ�ǰ�����Ⱦ����
        RenderTexture.active = _destTexture;
        //����һ��Texture2D
        if (_texture2D == null)
            _texture2D = new Texture2D(_destTexture.width, _destTexture.height, TextureFormat.RGB24, false);
        var texRect = new Rect(0, 0, _destTexture.width, _destTexture.height);
        _texture2D.ReadPixels(texRect, 0, 0);
        _texture2D.Apply();
        // ���
        RenderTexture.active = null;
        // ��ֵ��RawImage
        _blurImage.texture = _texture2D;
    }

    /// <summary>
    /// ��Shader��Ⱦ��˹ģ��
    /// </summary> 
    private void OnRender(RenderTexture src, RenderTexture dest)
    {
        if (_material != null)
        {
            // ��������������
            int rtW = src.width / DownSample;
            // ������������߶�
            int rtH = src.height / DownSample;
            RenderTexture buffer0 = RenderTexture.GetTemporary(rtW, rtH, 0);
            // �˲�ģʽ����Ϊ˫����
            buffer0.filterMode = FilterMode.Bilinear;
            Graphics.Blit(src, buffer0);
            for (int i = 0; i < Iterations; i++)
            {
                // ����ģ���ߴ�(���������ƫ����)
                _material.SetFloat("_BlurSize", 1.0f + i * BlurSpread);
                RenderTexture buffer1 = RenderTexture.GetTemporary(rtW, rtH, 0);
                // ��Ⱦ��ֱ��Pass
                Graphics.Blit(buffer0, buffer1, _material, 0);
                RenderTexture.ReleaseTemporary(buffer0);
                buffer0 = buffer1;
                buffer1 = RenderTexture.GetTemporary(rtW, rtH, 0);
                // ��Ⱦˮƽ��Pass
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