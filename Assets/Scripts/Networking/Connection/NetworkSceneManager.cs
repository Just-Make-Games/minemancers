using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkSceneManager : MonoBehaviour
{
    [SerializeField] private string menuScene;
    [SerializeField] private string gameScene;
    
    private void Start()
    {
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        NetworkManager.Singleton.OnClientStarted += OnClientStarted;
        NetworkManager.Singleton.OnClientStopped += OnClientStopped;
        
        SceneManager.LoadScene(menuScene);
    }
    
    private void OnServerStarted()
    {
        var status = NetworkManager.Singleton.SceneManager.LoadScene(gameScene, LoadSceneMode.Single);
        
        if (status != SceneEventProgressStatus.Started)
        {
            Debug.LogWarning($"Failed to load {gameScene} " +
                             $"with a {nameof(SceneEventProgressStatus)}: {status}");
        }
    }
    
    private void OnClientStarted()
    {
        UIScreenManager.Singleton.SetScreen(UIScreen.Game);
    }
    
    private void OnClientStopped(bool obj)
    {
        if (SceneManager.GetSceneByName(gameScene).isLoaded)
            SceneManager.LoadScene(menuScene);
        
        UIScreenManager.Singleton.SetScreen(UIScreen.Menu);
    }
}