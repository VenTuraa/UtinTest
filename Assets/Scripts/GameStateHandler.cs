using Zenject;

public interface IGameStateHandler
{
    void OnPlayerTooSmall();
    void OnPathNotClearable();
    void OnPlayerReachedTarget();
    void OnObstacleDestroyed();
    void CheckGameState();
}

public class GameStateHandler : IGameStateHandler
{
    private readonly GameManager gameManager;
    private readonly PlayerBall playerBall;
    private readonly PathManager pathManager;
    
    [Inject]
    public GameStateHandler(
        GameManager gameManager,
        PlayerBall playerBall,
        PathManager pathManager)
    {
        this.gameManager = gameManager;
        this.playerBall = playerBall;
        this.pathManager = pathManager;
    }
    
    public void OnPlayerTooSmall()
    {
        playerBall?.StopMovement();
        gameManager.OnPlayerTooSmall();
    }
    
    public void OnPathNotClearable()
    {
        playerBall?.StopMovement();
        gameManager.OnPathNotClearable();
    }
    
    public void OnPlayerReachedTarget()
    {
        gameManager.OnPlayerReachedTarget();
    }
    
    public void OnObstacleDestroyed()
    {
        CheckGameState();
    }
    
    public void CheckGameState()
    {
        if (gameManager.IsGameEnded || playerBall == null)
            return;
        
        if (!playerBall.HasEnoughSizeForPassage())
        {
            if (!playerBall.CanCreateMoreShots())
                OnPathNotClearable();
            return;
        }
        
        if (pathManager && pathManager.IsPathClear())
            playerBall.EnableMovement();
    }
}

