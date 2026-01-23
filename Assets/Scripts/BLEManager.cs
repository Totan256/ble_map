using Android.BLE;
using Android.BLE.Commands;
using System;
using UnityEngine;
using static Android.BLE.Commands.DiscoverDevices;

public class BLEManager : MonoBehaviour
{
    [SerializeField] private DdeviceManager dDeviceManager;
    [SerializeField] private DebugLogger logger;
    private DiscoverDevices _currentScanCommand = null;

    public void StartScan()
    {
        logger.Log("ble scan start");
        // 1. プラグインの初期化を確認
        if (!BleManager.IsInitialized)
        {
            logger.Log("scan");
            try
            {
                BleManager.Instance.Initialize();
            }
            catch (System.Exception e)
            {
                logger.Log($"Initialize Error: {e.Message}");
            }
            logger.Log("scan");
        }

        // すでにスキャン中の場合は一度停止する
        if (_currentScanCommand != null)
        {
            StopScan();
        }

        // 2. DiscoverDevices コマンドの作成
        // 引数: デバイス発見時のコールバック, 終了時のコールバック, スキャン時間(ミリ秒)
        // 手動で停止(StopScan)する運用のため、スキャン時間は長めに設定するか、デフォルト値を使用します。
        _currentScanCommand = new DiscoverDevices(OnDeviceDiscovered, OnScanFinished, 10000); //

        // 3. コマンドをキューに追加して実行
        BleManager.Instance.QueueCommand(_currentScanCommand);
    }

    public void StopScan()
    {
        logger.Log("stop scan");
        if (_currentScanCommand != null)
        {
            _currentScanCommand.End(); //
            _currentScanCommand = null;
        }
    }
    private void OnDeviceDiscovered(string address, string name, int rssi)
    {
        if (dDeviceManager != null)
        {
            dDeviceManager.OnDeviceFound(address, name, rssi);
        }
    }

    /// スキャンが規定時間経過、または明示的に終了した際の処理
    private void OnScanFinished()
    {
        _currentScanCommand = null;
    }
}