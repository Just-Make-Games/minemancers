using UnityEngine;

[System.Serializable]
public class PlayerMovement
{
    [SerializeField] private float movementSpeed = 4f;
    [SerializeField] private CharacterController2D characterController2D;
    [SerializeField] private HealthPresenter playerHealthPresenter;
    
    private bool _canMove = true;
    
    public void Initialize()
    {
        playerHealthPresenter.OnDeath += OnPlayerDeath;
        playerHealthPresenter.OnRestore += OnPlayerRestore;
    }
    
    // This function has to simulate movement in a certain time frame accounting for collision
    public void MoveByInput(InputPayload inputPayload, float timeIncrement)
    {
        if (!_canMove)
        {
            return;
        }
        
        characterController2D.move(inputPayload.MovementInput * movementSpeed * timeIncrement);
    }
    
    // This function has to set our player position into the state
    public void SetPlayerToState(StatePayload statePayload, Transform transform)
    {
        transform.position = statePayload.Position;
    }
    
    private void OnPlayerDeath(Player player)
    {
        _canMove = false;
    }
    
    private void OnPlayerRestore()
    {
        _canMove = true;
    }
}