using UnityEngine;

public abstract class HealthView : MonoBehaviour
{
    [SerializeField] public HealthPresenter healthPresenter;
    
    public void Awake()
    {
        if (!healthPresenter) return;
        
        healthPresenter.OnInitialize += InitializeValue;
        healthPresenter.OnUpdate += UpdateValue;
    }
    
    public void OnDestroy()
    {
        if (!healthPresenter)
        {
            return;
        }
        
        healthPresenter.OnUpdate -= UpdateValue;
    }
    
    protected abstract void InitializeValue(float minValue, float maxValue);
    
    protected abstract void UpdateValue(float value, bool isGhostMode);
}