using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;

public class MapVisualizer : MonoBehaviour
{
    [SerializeField] private ComputeShader computeShader;
    [SerializeField] private DdeviceManager deviceManager;
    [SerializeField] private GPSManager gpsManager;

    [SerializeField] private RawImage displayImage;

    private RenderTexture workTexture;    // 計算・蓄積用
    private RenderTexture displayTexture; // 表示用
    private ComputeBuffer sampleBuffer;

    private int kernelMain;
    private int kernelClear;
    private int currentDeviceIndex = 0;
    private const float LAT_DEGREE_TO_METERS = 111319.9f;

    public float render_progress;
    public float appliedPower;
    public float appliedEnv;
    public float appliedZoom;
    public int appliedNum;

    void Start()
    {
        // テクスチャの初期化
        RectTransform rect = displayImage.rectTransform;
        int width = Mathf.RoundToInt(rect.rect.width);
        int height = Mathf.RoundToInt(rect.rect.height);

        // 2. そのサイズに合わせてテクスチャを作成
        workTexture = CreateRT(width, height);
        displayTexture = CreateRT(width, height);
        displayImage.texture = displayTexture;
                
        kernelMain = computeShader.FindKernel("CSMain");
        kernelClear = computeShader.FindKernel("CSClear");
    }

    RenderTexture CreateRT(int w, int h)
    {
        // フォーマットを適正化し、filterModeをBilinearにすることでドットの荒さを軽減
        RenderTexture rt = new RenderTexture(w, h, 0, RenderTextureFormat.ARGBHalf);
        rt.enableRandomWrite = true;
        rt.filterMode = FilterMode.Bilinear; // これで滑らかになります
        rt.wrapMode = TextureWrapMode.Clamp;
        rt.Create();
        return rt;
    }
    private void UpdateBuffer(int requiredCount, int stride)
    {
        // サイズが変わった場合、またはバッファが未作成の場合のみ作り直す
        if (sampleBuffer == null || sampleBuffer.count != requiredCount)
        {
            if (sampleBuffer != null) sampleBuffer.Release();
            // 0だとエラーになるので最低1は確保
            sampleBuffer = new ComputeBuffer(Mathf.Max(1, requiredCount), stride);
        }
    }
    Vector2 currentPosition = Vector2.zero;
    void Update()
    {
        
        var devices = deviceManager._devices.Values
                    .Where(d => d.isVisible)
                    .OrderBy(d => d.address) // アドレスでソート
                    .ToList();

        //初期化
        if (currentDeviceIndex>=appliedNum || devices.Count <= currentDeviceIndex)
        {
            Graphics.Blit(workTexture, displayTexture);

            computeShader.SetTexture(kernelClear, "_Result", workTexture);
            int tx = Mathf.CeilToInt(workTexture.width / 8.0f);
            int ty = Mathf.CeilToInt(workTexture.height / 8.0f);
            computeShader.Dispatch(kernelClear, tx, ty, 1);
            computeShader.SetFloat("_MapSize", appliedZoom);
            computeShader.SetVector("_CurrentPosition", Vector2.zero);
            computeShader.SetFloat("_ZoomLevel", appliedZoom);
            currentPosition = gpsManager.GetCurrentPosition();
            currentDeviceIndex = 0;
        }
        GPUDeviceSample[] gpuSamples;
        Vector2 myPos;
        if (devices.Count == 0)
        {
            myPos = new Vector2(35.6812f, 139.7671f);
            gpuSamples = new GPUDeviceSample[]
            {
            new GPUDeviceSample {
                rssi = -66.0f, // 2m
                worldPosition = new Vector2(0.000018f, 0.000000f)*LAT_DEGREE_TO_METERS,
                color = Color.blue
            },
            new GPUDeviceSample {
                rssi = -74.0f, // 5m
                worldPosition = new Vector2(0.000000f, 0.000045f)*LAT_DEGREE_TO_METERS,
                color = Color.blue
            },
            new GPUDeviceSample {
                rssi = -80.0f, // 10m
                worldPosition = new Vector2(-0.000063f, -0.000063f)*LAT_DEGREE_TO_METERS,
                color = Color.blue
            }
            };
        }
        else
        {
            myPos = currentPosition;
            gpuSamples = devices[currentDeviceIndex].samples.Select(s => new GPUDeviceSample
            {
                rssi = s.rssi,
                worldPosition = s.worldPosition,
                color = devices[currentDeviceIndex].deviceColor
            }).ToArray();
            float latRad = myPos.x * Mathf.Deg2Rad;
            float lonToMeters = LAT_DEGREE_TO_METERS * Mathf.Cos(latRad);
            for (int i = 0; i < gpuSamples.Length; i++) {
                // 緯度の差分 × 緯度1度あたりのメートル
                float dy = (gpuSamples[i].worldPosition.x - myPos.x) * LAT_DEGREE_TO_METERS;
                // 経度の差分 × 緯度補正した経度1度あたりのメートル
                float dx = (gpuSamples[i].worldPosition.y - myPos.y) * lonToMeters;
                gpuSamples[i].worldPosition = new Vector2(dy, dx);
            }
        }
        currentDeviceIndex++;

        // 進捗更新
        if (devices.Count > 0)
        {
            render_progress = (float)currentDeviceIndex / devices.Count;
        }


        int stride = sizeof(float) * 7;
        UpdateBuffer(gpuSamples.Length, stride);
        sampleBuffer.SetData(gpuSamples);

        computeShader.SetBuffer(kernelMain, "_Samples", sampleBuffer);
        computeShader.SetInt("_SampleCount", gpuSamples.Length);
        computeShader.SetVector("_MapSize", new Vector2(workTexture.width, workTexture.height));

        // 設定値の適用
        computeShader.SetFloat("_MeasuredPower", appliedPower);
        computeShader.SetFloat("_EnvironmentalFactor", appliedEnv);
        computeShader.SetVector("_CurrentPosition", Vector2.zero);
        computeShader.SetFloat("_ZoomLevel", appliedZoom);

        computeShader.SetTexture(kernelMain, "_Result", workTexture);
        int threadGroupsX = Mathf.CeilToInt(workTexture.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(workTexture.height / 8.0f);
        computeShader.Dispatch(kernelMain, threadGroupsX, threadGroupsY, 1);

        
    }

    void OnDestroy()
    {
        if (sampleBuffer != null) sampleBuffer.Release();
    }

}