using UnityEngine;


public class UIWindowManager : MonoBehaviour
{
    [SerializeField] private GameObject[] windows;

    public void OpenWindow(int arrayIndex)
    {
        foreach (GameObject window in windows)
        {
            window.SetActive(false);
        }

        GameObject targetWindow = windows[Mathf.Clamp(arrayIndex, 0, windows.Length - 1)];
        if (targetWindow == null)
        {
            Debug.Log($"No Gameobject assigned to window number {arrayIndex}");
        }
        targetWindow.SetActive(true);
    }

    public void CloseWindow(int arrayIndex)
    {
        GameObject targetWindow = windows[Mathf.Clamp(arrayIndex, 0, windows.Length - 1)];
        if (targetWindow == null)
        {
            Debug.Log($"No Gameobject assigned to window number {arrayIndex}");
        }
        targetWindow.SetActive(false);
    }
}
