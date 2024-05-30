using UnityEngine;

public class SpriteHealthTintView : HealthView
{
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private Color tintColor;
    [SerializeField] private Color alternativeTintColor;
    
    private float _minValue;
    private float _maxValue;
    private Color _originalColor;
    private Color _targetColor;
    
    protected override void InitializeValue(float minValue, float maxValue)
    {
        _minValue = minValue;
        _maxValue = maxValue;
        _originalColor = sprite.color;
        _targetColor = tintColor;
    }
    
    protected override void UpdateValue(float value, bool useAlternativeColor)
    {
        if (value <= _minValue)
        {
            sprite.color = _originalColor;
            
            return;
        }
        
        _targetColor = useAlternativeColor ? alternativeTintColor : tintColor;
        var combineRatio = 1 - value / _maxValue;
        sprite.color = Color.Lerp(_originalColor, _targetColor, combineRatio);
    }
}