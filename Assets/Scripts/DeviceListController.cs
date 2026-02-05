using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeviceListController : MonoBehaviour
{
    [SerializeField] private DdeviceManager deviceManager;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Transform contentParent;
    [SerializeField] private TMP_Dropdown sortDropdown;

    private Dictionary<string, DeviceItemUI> uiEntries = new Dictionary<string, DeviceItemUI>();

    public enum SortMode { NewestDesc, NewestAsc, RSSIDesc, RSSIAsc }

    private void Start()
    {
        sortDropdown.ClearOptions();
        sortDropdown.AddOptions(new List<string> { "Latest", "Oldest", "RSSI(High→Low)", "RSSI(Low→High)" });
        sortDropdown.onValueChanged.AddListener(_ => RefreshList());

        // 初回実行
        RefreshList();
    }

    // Updateでの毎フレーム更新はやめ、必要な時だけ外部（DdeviceManagerなど）から呼ぶ運用を推奨
    private float lastUpdateTime;
    void Update()
    {
        // デバイスが増えた、または値が変わったタイミングで呼ぶ（1秒おきなど）
        if (Time.time - lastUpdateTime > 1f)
        {
            RefreshList();
            lastUpdateTime = Time.time;
        }
    }

    public void RefreshList()
    {
        // 1. ソート処理（nullチェックとCountチェックを追加）
        var sortedDevices = deviceManager._devices.Values.ToList();

        switch ((SortMode)sortDropdown.value)
        {
            case SortMode.NewestDesc:
                sortedDevices = sortedDevices.OrderByDescending(d => d.samples.Count > 0 ? d.samples[d.samples.Count - 1].timestamp : -1f).ToList();
                break;
            case SortMode.NewestAsc:
                sortedDevices = sortedDevices.OrderBy(d => d.samples.Count > 0 ? d.samples[d.samples.Count - 1].timestamp : float.MaxValue).ToList();
                break;
            case SortMode.RSSIDesc:
                sortedDevices = sortedDevices.OrderByDescending(d => d.samples.Count > 0 ? d.samples[d.samples.Count - 1].rssi : -200).ToList();
                break;
            case SortMode.RSSIAsc:
                sortedDevices = sortedDevices.OrderBy(d => d.samples.Count > 0 ? d.samples[d.samples.Count - 1].rssi : 100).ToList();
                break;
        }

        // 2. UIの生成と並び替え
        for (int i = 0; i < sortedDevices.Count; i++)
        {
            var entity = sortedDevices[i];

            if (!uiEntries.ContainsKey(entity.address))
            {
                GameObject newItem = Instantiate(itemPrefab, contentParent);
                // DeviceItemUIの取得または追加
                var ui = newItem.GetComponent<DeviceItemUI>() ?? newItem.AddComponent<DeviceItemUI>();

                // Prefabの構造に合わせてセットアップ（ここは元の実装を維持）
                ui.Setup(newItem.transform.Find("text_name").GetComponent<TextMeshProUGUI>(),
                         newItem.transform.Find("text_address").GetComponent<TextMeshProUGUI>(),
                         newItem.transform.Find("Toggle").GetComponent<Toggle>(),
                         newItem.GetComponent<Image>());

                uiEntries.Add(entity.address, ui);
            }

            // UIデータの更新
            var lastSample = entity.samples.Count > 0 ? entity.samples[entity.samples.Count - 1] : null;
            string displayName = string.IsNullOrEmpty(entity.name) ? "Unknown" : entity.name;
            uiEntries[entity.address].UpdateData(displayName, entity.address, entity.deviceColor);

            // 並び順（SiblingIndex）をソート結果に合わせる
            uiEntries[entity.address].transform.SetSiblingIndex(i);

            // イベントの再登録
            uiEntries[entity.address].toggle.onValueChanged.RemoveAllListeners();
            uiEntries[entity.address].toggle.isOn = entity.isVisible;
            uiEntries[entity.address].toggle.onValueChanged.AddListener((val) => {
                entity.isVisible = val;
            });
        }
    }
    public void SetAllVisible(bool visible)
    {
        foreach (var entity in deviceManager._devices.Values)
        {
            entity.isVisible = visible;
        }
        RefreshList();
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