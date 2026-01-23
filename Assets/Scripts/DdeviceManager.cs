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
    private Dictionary<string, DeviceEntity> _devices = new Dictionary<string, DeviceEntity>();

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
        gpsManager.UpdatePosition();
        bleManager.StartScan();
    }
    public void OnDeviceFound(string address, string name, int rssi)
    {
        
        //ogger.Log($"デバイス検知: {name} アドレス: {address} (RSSI: {rssi})");

        if (!_devices.ContainsKey(address))
        {
            _devices[address] = new DeviceEntity { address = address, name = name };
        }

        // 現在のGPS位置（または推定位置）を取得して記録
        Vector2 currentGpsPos = gpsManager.GetCurrentPosition(); // GPSManager側の実装に合わせて取得
        _devices[address].AddSample(rssi, currentGpsPos);

        logger.Log($"更新: {name} ({address}) RSSI: {rssi} サンプル数: {_devices[address].samples.Count}");
    }

}
