using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
class Tab
{
    public Button Button;
    public Image ButtonImage;
    public GameObject Content;
}

public class TabManager : MonoBehaviour
{

    [SerializeField] private Tab[] tabs;
    [SerializeField] Color defaultColor;
    [SerializeField] Color selectedColor;


    private void Start()
    {
        InitializeTabs();
    }

    private void InitializeTabs()
    {
        foreach (Tab tab in tabs)
            tab.Button.onClick.AddListener(() => TabButtonAction(tab));
        if (tabs[0] != null)
            TabButtonAction(tabs[0]);
    }

    private void TabButtonAction(Tab myTabClass)
    {
        foreach (var tab in tabs)
        {
            tab.Content.SetActive(false);
            tab.ButtonImage.color = defaultColor;
        }
        myTabClass.Content.SetActive(true);
        myTabClass.ButtonImage.color = selectedColor;
    }
}
