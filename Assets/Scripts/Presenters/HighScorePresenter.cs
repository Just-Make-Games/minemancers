using UnityEngine;

public class HighScorePresenter : MonoBehaviour
{
    [SerializeField] private HighScoreUI view;
    
    private Player _leader;
    private int _highScore;
    
    private void Awake()
    {
        PlayerScore.OnKill += OnKill;
        GameLogic.OnRoundOver += Reset;
    }
    
    private void OnDestroy()
    {
        PlayerScore.OnKill -= OnKill;
        GameLogic.OnRoundOver -= Reset;
    }
    
    private void OnKill(Player killer, int kills)
    {
        if (kills <= _highScore)
        {
            return;
        }
        
        _highScore = kills;
        var playerName = killer.PlayerName.Value;
        
        view?.UpdateScore(playerName, kills);
    }
    
    private void Reset()
    {
        view?.Reset();
    }
}