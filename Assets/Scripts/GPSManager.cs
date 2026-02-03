using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPSManager : MonoBehaviour
{
    private Vector2 currentPosition;
    [SerializeField] private DebugLogger logger;

    public void UpdatePosition()
    {
        StartCoroutine(GetLocationCoroutine());
    }
    private void Start()
    {
    }
    private void Update()
    {
        
    }

    public Vector2 GetCurrentPosition()
    {
        return currentPosition;
    }

    private IEnumerator GetLocationCoroutine()
    {
        if (!Input.location.isEnabledByUser)
        {
            logger.Log("端末のGPS設定が無効です。");
            yield break;
        }

        // 精度5m、更新距離1mに設定して起動
        Input.location.Start(5f, 1f);
        logger.Log("GPSサービス起動中...");

        // 初期化を待機（最大20秒）
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait < 1)
        {
            logger.Log("GPS初期化タイムアウト");
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            logger.Log("GPS取得失敗");
        }

        // Running状態である限り、位置を更新し続ける
        while (Input.location.status == LocationServiceStatus.Running)
        {
            LocationInfo data = Input.location.lastData;
            currentPosition = new Vector2(data.latitude, data.longitude);
            logger.Log($"現在地取得成功\n緯度: {data.latitude}\n経度: {data.longitude}");
            // 必要に応じてログ出力
            yield return new WaitForSeconds(1);
        }

    }
}
