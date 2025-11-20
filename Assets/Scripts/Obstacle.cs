using UnityEngine;
using Zenject;

public class Obstacle : MonoBehaviour
{
    private bool isDestroyed;
    private IGameStateHandler gameStateHandler;
    private PathManager pathManager;
    
    [Inject]
    private void Construct(IGameStateHandler gameStateHandler, PathManager pathManager)
    {
        this.gameStateHandler = gameStateHandler;
        this.pathManager = pathManager;
        pathManager?.RegisterObstacle(this);
    }
    
    private void OnDestroy()
    {
        pathManager?.UnregisterObstacle(this);
    }
    
    public void Destroy()
    {
        if (isDestroyed)
            return;
        
        isDestroyed = true;
        
        gameStateHandler?.OnObstacleDestroyed();
        
        Destroy(gameObject);
    }
    
    public bool IsDestroyed => isDestroyed;
}

