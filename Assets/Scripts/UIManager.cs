using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private MapVisualizer mapVisualizer;

    [SerializeField] private Image progressFill;

    [SerializeField] private Slider sliderPower;
    [SerializeField] private TextMeshProUGUI textPowerValue;
    [SerializeField] private Slider sliderEnv;
    [SerializeField] private TextMeshProUGUI textEnvValue;
    [SerializeField] private Slider sliderZoom;
    [SerializeField] private TextMeshProUGUI textZoomValue;
    [SerializeField] private Slider sliderNum;
    [SerializeField] private TextMeshProUGUI textNumValue;
    [SerializeField] private Button setButton;
    [Header("Pages")]
    public GameObject shaderPage;
    public GameObject listPage;
    public GameObject logPage;

    void Start()
    {
        // 起動時はShader画面だけ表示する
        ShowShaderPage();
        sliderPower.value = -60f;
        sliderEnv.value = 2.0f;
        sliderZoom.value = 20f;
        sliderNum.value = 60;
        ApplySettings();
    }

    private void Update()
    {
        progressFill.fillAmount = mapVisualizer.render_progress;


        textEnvValue.text = "Env   : " + sliderEnv.value;
        textPowerValue.text = "Power : " + sliderPower.value;
        textZoomValue.text = "Zoom  : " + sliderZoom.value;
        textNumValue.text = "Num   : " + sliderNum.value;
        if (mapVisualizer.appliedEnv != sliderEnv.value ||
            mapVisualizer.appliedPower != sliderPower.value ||
            mapVisualizer.appliedZoom != sliderZoom.value ||
            mapVisualizer.appliedNum != sliderNum.value)
        {
            setButton.image.color = Color.yellow;
        }
        else
        {
            setButton.image.color = Color.white;
        }
    }

    public void ShowShaderPage()
    {
        shaderPage.SetActive(true);
        listPage.SetActive(false);
        logPage.SetActive(false);
    }

    public void ShowListPage()
    {
        shaderPage.SetActive(false);
        listPage.SetActive(true);
        logPage.SetActive(false);
    }

    public void ShowLogPage()
    {
        shaderPage.SetActive(false);
        listPage.SetActive(false);
        logPage.SetActive(true);
    }

    public void ApplySettings()
    {
        mapVisualizer.appliedPower = sliderPower.value;
        mapVisualizer.appliedEnv = sliderEnv.value;
        mapVisualizer.appliedZoom = sliderZoom.value;
        mapVisualizer.appliedNum = (int)(sliderNum.value);
    }

    // リセットボタンから呼ぶ関数
    public void ResetSliders()
    {
        sliderPower.value = -60f;
        sliderEnv.value = 2.0f;
        sliderZoom.value = 20f;
        sliderNum.value = 60;
    }
}