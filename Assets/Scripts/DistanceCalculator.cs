using UnityEngine;
using System.Collections.Generic;

public class DistanceCalculator : MonoBehaviour
{
    //[SerializeField] private DdeviceManager deviceManager;
    [SerializeField] private ComputeShader computeShader;
    [SerializeField] private MeshRenderer displayRenderer; // 結果を表示するPlaneなど

    private ComputeBuffer _sampleBuffer;
    private RenderTexture _resultTexture;
    private const int MaxTotalSamples = 1000;

    void Start()
    {
        _resultTexture = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32);
        _resultTexture.enableRandomWrite = true;
        _resultTexture.Create();

        // Materialにテクスチャをセット
        displayRenderer.material.mainTexture = _resultTexture;
    }

    void Update()
    {
        //UpdateComputeShader();
    }

    // DistanceCalculator.cs などのメンバ変数として保持
    private List<GPUDeviceSample> _allSamplesCache = new List<GPUDeviceSample>(1000);

    public void UpdateComputeShader(Dictionary<string, DeviceEntity>.ValueCollection devices)
    {
        // リストをクリアして再利用（メモリ確保を避ける）
        _allSamplesCache.Clear();

        foreach (var device in devices)
        {
            // 効率のため foreach ではなく、必要なら samples の件数分だけ回す
            foreach (var s in device.samples)
            {
                // GPUDeviceSample は struct である必要があります
                _allSamplesCache.Add(new GPUDeviceSample
                {
                    rssi = (float)s.rssi,
                    worldPosition = s.worldPosition
                });
            }
        }

        if (_allSamplesCache.Count == 0) return;

        _sampleBuffer?.Release();
        _sampleBuffer = new ComputeBuffer(_allSamplesCache.Count, sizeof(float) * 3);
        _sampleBuffer.SetData(_allSamplesCache.ToArray());

        int kernel = computeShader.FindKernel("CSMain");
        computeShader.SetBuffer(kernel, "_Samples", _sampleBuffer);
        computeShader.SetInt("_SampleCount", _allSamplesCache.Count);
        computeShader.SetVector("_MapSize", new Vector2(_resultTexture.width, _resultTexture.height));
        computeShader.SetTexture(kernel, "_Result", _resultTexture);

        // パラメータ設定 (環境に合わせて調整)
        computeShader.SetFloat("_MeasuredPower", -59f);
        computeShader.SetFloat("_EnvironmentalFactor", 2.0f);

        computeShader.Dispatch(kernel, _resultTexture.width / 8, _resultTexture.height / 8, 1);
    }

    void OnDestroy()
    {
        _sampleBuffer?.Release();
    }
}