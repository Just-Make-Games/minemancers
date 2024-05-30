using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitioner : MonoBehaviour
{
    private static readonly int Start = Animator.StringToHash("Start");
    private static readonly int End = Animator.StringToHash("End");
    
    public static SceneTransitioner Singleton;

    [SerializeField] private Animator animator;
    [SerializeField] private float transitionTime;
    
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

    public void LoadGameScene()
    {
        StartCoroutine(LoadLevel("Game"));
    }

    public void LoadMenuScene()
    {
        StartCoroutine(LoadLevel("Menu"));
    }

    private IEnumerator LoadLevel(string level)
    {
        animator.SetTrigger(Start);

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(level);
        animator.SetTrigger(End);
    }
}
