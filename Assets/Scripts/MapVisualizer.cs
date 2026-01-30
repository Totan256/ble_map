using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MapVisualizer : MonoBehaviour
{
    [SerializeField] private ComputeShader bleComputeShader;
    [SerializeField] private RawImage displayImage;
    [SerializeField] private DdeviceManager deviceManager;

    [Header("Sliders")]
    [SerializeField] private Slider measuredPowerSlider;
    [SerializeField] private Slider environmentSlider;

    private RenderTexture resultTexture;
    private ComputeBuffer sampleBuffer;
    private int kernelHandle;
    private int resolution = 512; // テクスチャの解像度

    void Start()
    {
        // RenderTextureの初期化（Compute Shaderの書き込み先）
        resultTexture = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32);
        resultTexture.enableRandomWrite = true; // RWTexture2Dへの書き込みに必要
        resultTexture.Create();

        displayImage.texture = resultTexture;
        kernelHandle = bleComputeShader.FindKernel("CSMain"); //

        // スライダーの初期値設定
        measuredPowerSlider.minValue = -100f;
        measuredPowerSlider.maxValue = -30f;
        measuredPowerSlider.value = -60f;

        environmentSlider.minValue = 1.0f;
        environmentSlider.maxValue = 5.0f;
        environmentSlider.value = 2.0f;
    }

    void Update()
    {
        ExecuteShader();
    }

    void ExecuteShader()
    {
        // デバイスデータの準備
        var devices = deviceManager._devices;
        List<GPUDeviceSample> allSamples = new List<GPUDeviceSample>();

        foreach (var device in devices.Values)
        {
            foreach (var s in device.samples)
            {
                allSamples.Add(new GPUDeviceSample { rssi = s.rssi, worldPosition = s.worldPosition });
            }
        }

        if (allSamples.Count == 0) return;

        // Compute Bufferの作成と転送
        if (sampleBuffer != null) sampleBuffer.Release();
        sampleBuffer = new ComputeBuffer(allSamples.Count, sizeof(float) * 3); // float(rssi) + Vector2(pos)
        sampleBuffer.SetData(allSamples.ToArray());

        // パラメータのセット
        bleComputeShader.SetBuffer(kernelHandle, "_Samples", sampleBuffer); //
        bleComputeShader.SetInt("_SampleCount", allSamples.Count); //
        bleComputeShader.SetVector("_MapSize", new Vector2(resolution, resolution)); //
        bleComputeShader.SetTexture(kernelHandle, "_Result", resultTexture); //

        // スライダーからの入力を反映
        bleComputeShader.SetFloat("_MeasuredPower", measuredPowerSlider.value); //
        bleComputeShader.SetFloat("_EnvironmentalFactor", environmentSlider.value); //

        // Shader実行：[numthreads(8, 8, 1)]に合わせてグループ数を計算
        int groups = resolution / 8;
        bleComputeShader.Dispatch(kernelHandle, groups, groups, 1);
    }

    void OnDestroy()
    {
        if (sampleBuffer != null) sampleBuffer.Release();
    }
}