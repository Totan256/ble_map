using System.Collections;
using System.Collections.Generic;
using System.Security;
using UnityEngine;
using UnityEngine.Android;

public class DdeviceManager : MonoBehaviour
{
    [SerializeField] private GPSManager gpsManager;
    [SerializeField] private BLEManager bleManager;
    [SerializeField] private DebugLogger logger;

    // Start is called before the first frame update
    void Start()
    {
        logger.Log("start check perm");
        CheckPermissions();
        logger.Log("finish check perm");
    }

    private string[] permissions = {
        Permission.FineLocation,
        "android.permission.BLUETOOTH_SCAN",
        "android.permission.BLUETOOTH_CONNECT"
    };
    private void CheckPermissions()
    {
        bool allGranted = true;
        foreach (string p in permissions)
        {
            if (!Permission.HasUserAuthorizedPermission(p))
            {
                allGranted = false;
                logger.Log($"not perm {p}");
                break;
            }
        }

        if (!allGranted)
        {
            logger.Log("BLE/位置情報の権限をリクエストします...");
            Permission.RequestUserPermissions(permissions); // まとめてリクエスト
        }
        else
        {
            logger.Log("all permissions granted");
            StartSystem();
        }
    }

    private void StartSystem()
    {
        logger.Log("start system");
        // GPSの取得を開始
        gpsManager.GetCurrentPosition();
        bleManager.StartScan();
    }
    public void OnDeviceFound(string name, int rssi)
    {
        // シーケンス図の手順3: DeviceEntity更新 [cite: 58]
        logger.Log($"デバイス検知: {name} (RSSI: {rssi})");
        // ここで距離計算（DistanceCalculator）へ渡す処理を後で追加します
    }

}
