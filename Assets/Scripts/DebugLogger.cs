using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugLogger : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI logText;
    [SerializeField] private ScrollRect scrollRect;

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

            // 追加：ログ追加後に一番下までスクロールさせる
            //if (scrollRect != null)
            //{
            //    Canvas.ForceUpdateCanvases(); // レイアウトの更新を待つ
            //    scrollRect.verticalNormalizedPosition = 0f; // 0は一番下を指す
            //}
        }
        else
        {
            Debug.LogError("LogTextがInspectorで設定されていません！");
        }
        Debug.Log(message);
    }
}