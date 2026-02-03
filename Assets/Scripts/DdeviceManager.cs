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
    public Dictionary<string, DeviceEntity> _devices = new Dictionary<string, DeviceEntity>();

    // Start is called before the first frame update
    void Start()
    {
        logger.Log("start check perm");
        CheckPermissions();
        logger.Log("finish check perm");
        startTime = Time.time;
    }
    private float startTime;
    private bool waited = false;
    private void Update()
    {
        if (!waited && Time.time - startTime >= 5f)
        {
            waited = true;
            bleManager.StartScan();
            //gpsManager.UpdatePosition();
        }
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
    public void OnDeviceFound(string _address, string _name, int _rssi)
    {
        
        //ogger.Log($"デバイス検知: {name} アドレス: {address} (RSSI: {rssi})");

        if (!_devices.ContainsKey(_address))
        {
            _devices[_address] = new DeviceEntity { address = _address, name = _name,
                deviceColor = Random.ColorHSV(0f, 1f, 0.6f, 1f, 0.7f, 1f)
            };
        }

        // 現在のGPS位置（または推定位置）を取得して記録
        Vector2 currentGpsPos = gpsManager.GetCurrentPosition(); // GPSManager側の実装に合わせて取得
        _devices[_address].AddSample(_rssi, currentGpsPos);

        //nameがnullだった場合更新
        if (_devices[_address].name == null && _name != null)
        {
            _devices[_address].name = _name;
            logger.Log("update name");
        }
        if (_devices[_address].name==null)
            logger.Log("no name :");
        else
        {
            logger.Log($"{_name} :");
        }


        logger.Log($"   ({_address}) RSSI: {_rssi}");
        logger.Log($"   サンプル数:{_devices[_address].samples.Count}");
    }

}
