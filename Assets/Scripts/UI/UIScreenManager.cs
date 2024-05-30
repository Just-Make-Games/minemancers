using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UIScreen
{
    None,
    Menu,
    Game,
    Pause
}
[System.Serializable]
public struct Screen
{
    public UIScreen EnumType;
    public bool RequireCursor;
    public GameObject PanelObject;
}
public class UIScreenManager : MonoBehaviour
{
    public static UIScreenManager Singleton;

    [SerializeField] private UIScreen startingScreen;
    private UIScreen currentScreen;
    public UIScreen CurrentScreen { get; private set; }
    [SerializeField] private Screen[] screens;

    private void Awake()
    {
        if (Singleton != null && Singleton != this)
        {
            Destroy(this);
        }
        else
        {
            Singleton = this;
            DontDestroyOnLoad(this);
        }
    }

    private void Start()
    {
        SetScreen(startingScreen);
    }

    private void Update()
    {
        switch (currentScreen)
        {
            case UIScreen.Game:
                if (InputManager.Singleton.GetPause())
                    TogglePause();
                break;
            case UIScreen.Pause:
                if (InputManager.Singleton.GetPause())
                    TogglePause();
                break;
            default:
                break;
        }
    }

    public void TogglePause()
    {
        if (currentScreen == UIScreen.Game)
            SetScreen(UIScreen.Pause);
        else if (currentScreen == UIScreen.Pause)
            SetScreen(UIScreen.Game);
    }

    public static void SwitchCursorMode(bool cursorMode)
    {
        if (cursorMode)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void SetScreen()
    {
        SetScreen(currentScreen);
    }
    public void SetScreen(UIScreen targetScreen)
    {
        foreach (Screen screen in screens)
        {
            if (screen.EnumType == targetScreen)
            {
                //enable this screen
                screen.PanelObject.SetActive(true);
                SwitchCursorMode(screen.RequireCursor);
            }
            else
            {
                screen.PanelObject.SetActive(false);
            }
        }
        currentScreen = targetScreen;

    }


}
