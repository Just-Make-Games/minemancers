using Unity.Netcode;
using UnityEngine;

public class PlayerScorePresenter : NetworkBehaviour
{
    [SerializeField] private PlayerScore model;
    [SerializeField] private ScoreViewUI view;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        model.OnScoreChanged += ScoreChanged;
        
        UpdateView(model.Kills, model.Deaths);
    }
    
    public override void OnNetworkDespawn()
    {
        model.OnScoreChanged -= ScoreChanged;
        
        base.OnNetworkDespawn();
    }
    
    public void Killed()
    {
        model.AddKillCount();
    }
    
    public void Died()
    {
        model.AddDeathCount();
    }
    
    private void ScoreChanged(int kills, int deaths)
    {
        UpdateView(kills, deaths);
    }
    
    private void UpdateView(int kills, int deaths)
    {
        view.UpdateText(kills, deaths);
    }
}