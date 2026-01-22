using UnityEngine;
using TMPro;

public class DebugLogger : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI logText;

    void Awake()
    {
        if (logText == null)
        {
            Debug.LogError("LogTextがInspectorで設定されていません！");
        }
    }
    void Start()
    {
        //[cite_start]// 設計仕様書の「初期化」プロセスの開始を確認
        Log("System Initializing...");
        Log("Hello World! - 実機テスト成功");
    }

    public void Log(string message)
    {
        if (logText != null)
        {
            logText.text += "\n" + "[" + System.DateTime.Now.ToString("HH:mm:ss") + "] " + message;
        }
        else
        {
            Debug.LogError("LogTextがInspectorで設定されていません！");
        }
        Debug.Log(message);
    }
}