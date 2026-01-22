using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BLEManager : MonoBehaviour
{
    [SerializeField] private Text statusText; // 画面表示用

    public void StartScan()
    {
        Debug.Log("Scanning started...");
        statusText.text = "スキャン中...";

        // プラグインの初期化とスキャン開始処理をここに記述
        // 例: BluetoothLEHardwareInterface.ScanForPeripherals(...)
    }

    public void StopScan()
    {
        // スキャン停止処理
    }

    // デバイスが見つかった時に呼ばれるコールバック（仮）
    private void OnDeviceFound(string deviceName, int rssi)
    {
        statusText.text += $"\nName: {deviceName}, RSSI: {rssi}";
    }
}
