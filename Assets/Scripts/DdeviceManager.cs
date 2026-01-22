using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public class DdeviceManager : MonoBehaviour
{
    [SerializeField] private GPSManager gpsManager;
    [SerializeField] private DebugLogger logger;

    // Start is called before the first frame update
    void Start()
    {
        logger.Log("start check perm");
        CheckPermissions();
    }

    private void CheckPermissions()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            logger.Log("位置情報権限をリクエスト中...");
            // 権限をリクエスト
            Permission.RequestUserPermission(Permission.FineLocation);
            // リクエスト後の判定は本来コールバックが必要ですが、
            // 今回は簡易的に「もう一度ボタンを押す」か、再起動で確認する流れにします
        }
        else
        {
            logger.Log("権限確認済み。GPSを開始します。");
            StartSystem();
        }
    }

    private void StartSystem()
    {
        // GPSの取得を開始
        gpsManager.GetCurrentPosition();
    }
}
