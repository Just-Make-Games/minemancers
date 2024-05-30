using UnityEngine;

public class ShieldView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer shieldSprite;
    [SerializeField] private Collider2D shieldCollider;
    
    public bool EnableShield()
    {
        if (IsActive())
        {
            return false;
        }
        
        shieldSprite.enabled = true;
        shieldCollider.enabled = true;
        
        return true;
    }
    
    public void DisableShield()
    {
        if (!IsActive())
        {
            return;
        }
        
        shieldSprite.enabled = false;
        shieldCollider.enabled = false;
    }
    
    private bool IsActive()
    {
        return shieldSprite && shieldCollider && (shieldSprite.enabled || shieldCollider.enabled);
    }
}