using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPSManager : MonoBehaviour
{
    [SerializeField] private DebugLogger logger;

    public void GetCurrentPosition()
    {
        StartCoroutine(GetLocationCoroutine());
    }

    private IEnumerator GetLocationCoroutine()
    {
        // サービスが有効かチェック
        if (!Input.location.isEnabledByUser)
        {
            logger.Log("端末のGPS設定が無効です。");
            yield break;
        }

        // サービスの開始
        Input.location.Start();
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
        else
        {
            // 緯度経度の取得に成功
            float lat = Input.location.lastData.latitude;
            float lon = Input.location.lastData.longitude;
            logger.Log($"現在地取得成功！\n緯度: {lat}\n経度: {lon}");
        }

        // バッテリー消費を抑えるため、確認できたら一度停止させる
        Input.location.Stop();
    }
}
