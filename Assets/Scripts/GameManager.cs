using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private const float PAUSED_TIME_SCALE = 0f;
    private const float NORMAL_TIME_SCALE = 1f;

    [Header("Game State")]
    [SerializeField] private bool gameEnded;
    
    [Header("UI")]
    [SerializeField] private EndGamePopup endGamePopup;
    
    public bool IsGameEnded => gameEnded;
    
    private void Start()
    {
        endGamePopup?.Hide();
    }
    
    public void OnPlayerTooSmall()
    {
        if (gameEnded) return;
        EndGame(() => LoseGame("Player ball became too small!"));
    }
    
    public void OnPathNotClearable()
    {
        if (gameEnded) return;
        EndGame(() => LoseGame("Not enough shots to clear the path!"));
    }
    
    public void OnPlayerReachedTarget()
    {
        if (gameEnded) return;
        EndGame(WinGame);
    }
    
    private void EndGame(System.Action action)
    {
        gameEnded = true;
        action();
    }
    
    private void WinGame()
    {
        Debug.Log("Victory!");
        endGamePopup?.Show(true);
        Time.timeScale = PAUSED_TIME_SCALE;
    }
    
    private void LoseGame(string reason)
    {
        Debug.Log($"Defeat: {reason}");
        Time.timeScale = PAUSED_TIME_SCALE;
        endGamePopup?.Show(false);
    }
    
    public void RestartGame()
    {
        Time.timeScale = NORMAL_TIME_SCALE;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

