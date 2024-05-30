using System;

public class PlayerActions
{
    public event Action<StatePayload> onWeaponPrimaryDown;
    public event Action onWeaponSecondaryDown;

    public void Update(StatePayload state)
    {
        if (state.WeaponPrimaryDown)
        {
            onWeaponPrimaryDown?.Invoke(state);
        }
        
        if (state.WeaponSecondaryDown)
        {
            onWeaponSecondaryDown?.Invoke();
        }
    }
}
