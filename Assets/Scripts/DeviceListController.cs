using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DeviceListController : MonoBehaviour
{
    [SerializeField] private DdeviceManager deviceManager; // データの参照元
    [SerializeField] private GameObject itemPrefab;      // 先ほど作ったPrefab
    [SerializeField] private Transform contentParent;    // ScrollViewのContent

    // UI要素を保持しておくためのリスト
    private Dictionary<string, DeviceItemUI> uiEntries = new Dictionary<string, DeviceItemUI>();

    // チェック状態を管理するList<bool>（要望に合わせて実装）
    public List<bool> checkStates = new List<bool>();

    void Update()
    {
        // 1秒に数回など、頻度を抑えて更新しても良い
        RefreshList();
    }

    public void RefreshList()
    {
        int index = 0;
        foreach (var pair in deviceManager._devices)
        {
            string addr = pair.Key;
            DeviceEntity entity = pair.Value;

            if (!uiEntries.ContainsKey(addr))
            {
                // 新しいデバイスが見つかったらUIを生成
                GameObject newItem = Instantiate(itemPrefab, contentParent);
                var ui = newItem.AddComponent<DeviceItemUI>();
                // Prefab内のTMPコンポーネントをセット（実際はPrefab側のスクリプトで保持するのが理想）
                ui.Setup(newItem.transform.Find("text_name").GetComponent<TextMeshProUGUI>(),
                         newItem.transform.Find("text_address").GetComponent<TextMeshProUGUI>(),
                         newItem.transform.Find("Toggle").GetComponent<Toggle>(),
                         newItem.GetComponent<Image>());

                uiEntries.Add(addr, ui);
                checkStates.Add(false); // 初期値
            }

            // UIの内容を更新
            uiEntries[addr].UpdateData(entity.name ?? "Unknown", entity.address, entity.deviceColor);

            // チェックボックスの状態を管理リストと同期（簡易実装）
            int currentIndex = index;
            uiEntries[addr].toggle.onValueChanged.RemoveAllListeners();
            uiEntries[addr].toggle.onValueChanged.AddListener((val) => {
                checkStates[currentIndex] = val;
            });

            index++;
        }
    }
}

// 各行のUI参照を管理する補助クラス
public class DeviceItemUI : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI addrText;
    public Toggle toggle;
    public Image backgroundImage;

    public void Setup(TextMeshProUGUI n, TextMeshProUGUI a, Toggle t, Image bg)
    {
        nameText = n; addrText = a; toggle = t; backgroundImage = bg;
    }

    public void UpdateData(string n, string a, Color c)
    {
        nameText.text = "  "+n;
        addrText.text = a;
        backgroundImage.color = c;
    }
}