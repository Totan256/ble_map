using Android.BLE;
using Android.BLE.Commands;
using System.Collections;
using UnityEngine;

public class BLEManager : MonoBehaviour
{
    [SerializeField] private DdeviceManager dDeviceManager;
    [SerializeField] private DebugLogger logger;

    private DiscoverDevices _currentScanCommand = null;
    

    // スキャン設定
    private const int SCAN_DURATION_MS = 10000; // 10秒間スキャン
    private const float SCAN_INTERVAL_SEC = 20f; // 次のスキャンまで20秒待機（計30秒サイクル）

    void Start()
    {
        // 1. 初期化は最初に一度だけ行う
        InitializeBLE();

        // 2. 定期的なスキャンループを開始
        StartCoroutine(ScanRoutine());
    }

    private void InitializeBLE()
    {
        if (!BleManager.IsInitialized)
        {
            try
            {
                BleManager.Instance.Initialize();
                logger.Log("BLE Initialized");
            }
            catch (System.Exception e)
            {
                logger.Log($"Initialize Error: {e.Message}");
            }
        }
    }

    private IEnumerator ScanRoutine()
    {
        while (true)
        {
            if (_currentScanCommand == null)
            {
                StartScan();
            }

            // スキャン時間 + 待機時間分だけ待機
            // Androidのスロットリング（30秒に5回制限）を考慮し、余裕を持たせる
            yield return new WaitForSeconds((SCAN_DURATION_MS / 1000f) + SCAN_INTERVAL_SEC);
        }
    }

    public void StartScan()
    {
        if (_currentScanCommand != null) return;

        logger.Log("BLE scan start");

        // タイムアウト付きのコマンドを作成
        _currentScanCommand = new DiscoverDevices(OnDeviceDiscovered, OnScanFinished, SCAN_DURATION_MS);
        BleManager.Instance.QueueCommand(_currentScanCommand);
    }

    private void OnDeviceDiscovered(string address, string name, int rssi)
    {
        if (dDeviceManager != null)
        {
            dDeviceManager.OnDeviceFound(address, name, rssi);
        }
    }

    private void OnScanFinished()
    {
        logger.Log("Scan finished (timeout)");
        _currentScanCommand = null;
    }

    public void StopScan()
    {
        if (_currentScanCommand != null)
        {
            logger.Log("Stop scan manually");
            _currentScanCommand.End();
            _currentScanCommand = null;
        }
    }
}