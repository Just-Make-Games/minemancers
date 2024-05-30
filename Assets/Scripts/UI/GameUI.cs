using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class GameUI : MonoBehaviour
{
    public static GameUI Singleton;

    [SerializeField] private Button resumeButton;
    [SerializeField] private Button disconnectButton;
    
    private void Awake()
    {
        if (Singleton != null && Singleton != this)
        {
            Destroy(this);
        }
        else
        {
            Singleton = this;
        }

        resumeButton.onClick.AddListener(ResumeClicked);
        disconnectButton.onClick.AddListener(DisconnectClicked);
    }
    
    private void ResumeClicked()
    {
        UIScreenManager.Singleton.TogglePause();
    }
    
    private void DisconnectClicked()
    {
        NetworkManager.Singleton.Shutdown();
    }
}
