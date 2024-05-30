using UnityEngine;

public class TimePresenter : MonoBehaviour
{
    [SerializeField] private TimeUI view;
    
    private void Awake()
    {
        GameLogic.OnRoundTimerUpdated += UpdateView;
    }
    
    private void OnDestroy()
    {
        GameLogic.OnRoundTimerUpdated -= UpdateView;
    }
    
    private void UpdateView(float time)
    {
        view?.UpdateTime(time);
    }
}