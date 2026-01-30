using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Pages")]
    public GameObject shaderPage;
    public GameObject listPage;
    public GameObject logPage;

    void Start()
    {
        // 起動時はShader画面だけ表示する
        ShowShaderPage();
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
}