using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Singleton;
    
    private PlayerControls _playerControls;
    
    public Vector2 GetPlayerMovement()
    {
        return _playerControls.Player.Movement.ReadValue<Vector2>();
    }
    
    public bool GetWeaponPrimaryDown()
    {
        return _playerControls.Player.WeaponPrimary.WasPressedThisFrame();
    }
    
    public bool GetWeaponSecondaryDown()
    {
        return _playerControls.Player.WeaponSecondary.WasPressedThisFrame();
    }
    
    public bool GetPause()
    {
        return _playerControls.Player.Pause.WasPressedThisFrame();
    }
    
    public bool GetSwitchWeaponDown()
    {
        return _playerControls.Player.SwitchWeapon.WasPressedThisFrame();
    }
    
    private void Awake()
    {
        if (Singleton != null && Singleton != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Singleton = this;
            
            DontDestroyOnLoad(gameObject);
        }
        
        _playerControls = new PlayerControls();
    }
    
    private void OnEnable()
    {
        _playerControls.Enable();
    }
    
    private void OnDisable()
    {
        _playerControls.Disable();
    }
}