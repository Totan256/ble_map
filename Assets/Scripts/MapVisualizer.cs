using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;

public class MapVisualizer : MonoBehaviour
{
    [SerializeField] private ComputeShader computeShader;
    [SerializeField] private DdeviceManager deviceManager;
    [SerializeField] private GPSManager gpsManager;
    [SerializeField] private RawImage displayImage;
    [SerializeField] private Slider sliderPower;
    [SerializeField] private Slider sliderEnv;

    private RenderTexture workTexture;    // 計算・蓄積用
    private RenderTexture displayTexture; // 表示用
    private ComputeBuffer sampleBuffer;

    private int kernelMain;
    private int kernelClear;
    private int currentDeviceIndex = 0;

    private const float LAT_DEGREE_TO_METERS = 111319.9f;

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
        // 例：1m地点のRSSIを-60、環境係数を2.0に設定
        sliderPower.value = -60f;
        sliderEnv.value = 2.0f;
        
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

    void Update()
    {
        //初期化
        if(currentDeviceIndex>=60 || deviceManager._devices.Count <= currentDeviceIndex)
        {
            Graphics.Blit(workTexture, displayTexture);

            computeShader.SetTexture(kernelClear, "_Result", workTexture);
            int tx = Mathf.CeilToInt(workTexture.width / 8.0f);
            int ty = Mathf.CeilToInt(workTexture.height / 8.0f);
            computeShader.Dispatch(kernelClear, tx, ty, 1);
            currentDeviceIndex = 0;
        }
        Vector2 myPos;
        GPUDeviceSample[] gpuSamples;
        var devices = deviceManager._devices.Values
                    .OrderBy(d => d.address) // アドレスでソート
                    .ToList();
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
            myPos = gpsManager.GetCurrentPosition();
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

        // ストライドは float(1) + float2(2) + float4(4) = 7
        int stride = sizeof(float) * 7;

        if (sampleBuffer != null) sampleBuffer.Release();
        sampleBuffer = new ComputeBuffer(gpuSamples.Length, stride);
        sampleBuffer.SetData(gpuSamples);

        computeShader.SetBuffer(kernelMain, "_Samples", sampleBuffer);
        computeShader.SetInt("_SampleCount", gpuSamples.Length);
        computeShader.SetVector("_MapSize", new Vector2(workTexture.width, workTexture.height));

        // 設定値の適用
        computeShader.SetFloat("_MeasuredPower", sliderPower.value);
        computeShader.SetFloat("_EnvironmentalFactor", sliderEnv.value);
        computeShader.SetVector("_CurrentPosition", Vector2.zero);
        computeShader.SetFloat("_ZoomLevel", 20f);

        computeShader.SetTexture(kernelMain, "_Result", workTexture);
        int threadGroupsX = Mathf.CeilToInt(workTexture.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(workTexture.height / 8.0f);
        computeShader.Dispatch(kernelMain, threadGroupsX, threadGroupsY, 1);

        
    }

    void DispatchShader(DeviceEntity device)
    {
        // DeviceEntity.cs の GPUDeviceSample 構造体に合わせる
        var gpuSamples = device.samples.Select(s => new GPUDeviceSample
        {
            rssi = s.rssi,
            worldPosition = s.worldPosition
        }).ToArray();

        if (sampleBuffer != null) sampleBuffer.Release();
        sampleBuffer = new ComputeBuffer(gpuSamples.Length, sizeof(float) * 7);
        sampleBuffer.SetData(gpuSamples);

        computeShader.SetBuffer(kernelMain, "_Samples", sampleBuffer);
        computeShader.SetInt("_SampleCount", gpuSamples.Length);
        computeShader.SetVector("_MapSize", new Vector2(workTexture.width, workTexture.height));
        computeShader.SetFloat("_MeasuredPower", sliderPower.value);
        computeShader.SetFloat("_EnvironmentalFactor", sliderEnv.value);
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