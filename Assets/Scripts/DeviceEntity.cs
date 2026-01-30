using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DeviceSample
{
    public int rssi;
    public Vector2 worldPosition; // 計測時のGPS/自己位置
    public float timestamp;
}

// Compute Shaderに渡すための固定サイズ構造体
public struct GPUDeviceSample
{
    public Color color;
    public float rssi;
    public Vector2 worldPosition;
}

public class DeviceEntity
{
    public string address;
    public string name;
    public Color deviceColor;
    public List<DeviceSample> samples = new List<DeviceSample>();
    private const int MaxSampleSize = 100;

    public void AddSample(int rssi, Vector2 currentPos)
    {
        samples.Add(new DeviceSample
        {
            rssi = rssi,
            worldPosition = currentPos,
            timestamp = Time.time,
        });

        if (samples.Count > MaxSampleSize)
        {
            samples.RemoveAt(0); // 古いデータから削除
        }
    }
}