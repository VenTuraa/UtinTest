using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathManager : MonoBehaviour
{
    private const float MIN_PATH_DISTANCE = 0.1f;
    private const float PATH_HEIGHT_OFFSET = 0.1f;
    private const float PATH_WIDTH_HALF_MULTIPLIER = 0.5f;
    private const float MIN_PLAYER_SIZE_THRESHOLD = 0f;

    [Header("Path Settings")]
    [SerializeField] private float minPathWidth = 0.5f;
    [SerializeField] private Transform pathStart;
    [SerializeField] private Transform pathEnd;
    [SerializeField] private PlayerBall playerBall;

    [Header("Visual")]
    [SerializeField] private LineRenderer pathRenderer;
    [SerializeField] private Material pathMaterial;

    private float currentPathWidth;
    private HashSet<Obstacle> obstacles = new();

    private void Start()
    {
        if (pathRenderer == null)
        {
            Debug.LogError("PathRenderer is not assigned in PathManager!");
            return;
        }

        pathRenderer.material = pathMaterial ?? CreateDefaultMaterial();
        pathRenderer.useWorldSpace = true;
        pathRenderer.positionCount = 2;

        StartCoroutine(InitializePathWidth());
    }
    
    private IEnumerator InitializePathWidth()
    {
        yield return null;

        if (!playerBall) yield break;
        float playerSize = playerBall.transform.localScale.x;
        if (playerSize > MIN_PLAYER_SIZE_THRESHOLD)
        {
            UpdatePathWidth(playerSize);
        }
    }

    public void UpdatePathWidth(float playerSize)
    {
        currentPathWidth = Mathf.Max(playerSize, minPathWidth);

        if (!pathRenderer) return;
        
        pathRenderer.startWidth = currentPathWidth;
        pathRenderer.endWidth = currentPathWidth;
    }

    public bool IsPathClear()
    {
        if (!pathStart || !pathEnd)
            return false;

        Vector3 pathStartPos = playerBall ? playerBall.transform.position : pathStart.position;
        Vector3 pathEndPos = pathEnd.position;
        float pathDistance = Vector3.Distance(pathStartPos, pathEndPos);

        if (pathDistance < MIN_PATH_DISTANCE)
            return true;

        Vector3 pathDir = (pathEndPos - pathStartPos).normalized;
        float pathRadius = currentPathWidth * PATH_WIDTH_HALF_MULTIPLIER;

        foreach (Obstacle obstacle in obstacles)
        {
            if (obstacle == null || obstacle.IsDestroyed)
                continue;

            if (IsObstacleBlockingPath(obstacle, pathStartPos, pathEndPos, pathDir, pathDistance, pathRadius))
                return false;
        }

        return true;
    }

    private bool IsObstacleBlockingPath(Obstacle obstacle, Vector3 pathStartPos, Vector3 pathEndPos, 
        Vector3 pathDir, float pathDistance, float pathRadius)
    {
        Vector3 obstaclePos = obstacle.transform.position;
        Vector3 toObstacle = obstaclePos - pathStartPos;
        float projection = Vector3.Dot(toObstacle, pathDir);

        if (projection < 0f || projection > pathDistance)
            return false;

        Vector3 closestPoint = pathStartPos + pathDir * projection;
        float distanceToPath = Vector3.Distance(obstaclePos, closestPoint);

        return distanceToPath < pathRadius;
    }

    public float GetRequiredSize()
    {
        return currentPathWidth * GameConstants.PATH_SIZE_RATIO;
    }

    public void UpdatePathVisual()
    {
        if (pathRenderer == null || pathStart == null || pathEnd == null)
            return;

        Vector3 startPos = new Vector3(pathStart.position.x, PATH_HEIGHT_OFFSET, pathStart.position.z);
        Vector3 endPos = new Vector3(pathEnd.position.x, PATH_HEIGHT_OFFSET, pathEnd.position.z);

        pathRenderer.SetPosition(0, startPos);
        pathRenderer.SetPosition(1, endPos);
    }

    private Material CreateDefaultMaterial()
    {
        var mat = new Material(Shader.Find("Sprites/Default"))
        {
            color = Color.red
        };
        return mat;
    }

    public void RegisterObstacle(Obstacle obstacle)
    {
        if (obstacle != null)
            obstacles.Add(obstacle);
    }

    public void UnregisterObstacle(Obstacle obstacle)
    {
        if (obstacle != null)
            obstacles.Remove(obstacle);
    }
}