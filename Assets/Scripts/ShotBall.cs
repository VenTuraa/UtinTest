using UnityEngine;
using Zenject;

public class ShotBall : MonoBehaviour
{
    public class Factory : PlaceholderFactory<ShotBall> { }
    
    [Header("Movement")]
    [SerializeField] private float speed = 10f;
    
    [Header("Explosion")]
    [SerializeField] private float explosionRadiusMultiplier = 2f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private Rigidbody rb;
    
    private float currentSize = GameConstants.INITIAL_SHOT_SIZE;
    private Vector3 targetPosition;
    private bool isLaunched;
 
    
    public void Initialize(Vector3 target)
    {
        targetPosition = target;
        UpdateSize();
    }
    
    public void SetSize(float size)
    {
        currentSize = size;
        UpdateSize();
    }
    
    private void UpdateSize()
    {
        transform.localScale = Vector3.one * currentSize;
    }
    
    public void Launch()
    {
        if (rb == null)
        {
            Debug.LogError("Rigidbody is not assigned in ShotBall!");
            return;
        }
        
        isLaunched = true;
        rb.isKinematic = false;
        
        Vector3 direction = (targetPosition - transform.position).normalized;
        rb.linearVelocity = direction * speed;
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        CheckObstacleCollision(collision.gameObject);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        CheckObstacleCollision(other.gameObject);
    }
    
    private void CheckObstacleCollision(GameObject obj)
    {
        if (isLaunched && obj.TryGetComponent<Obstacle>(out _))
            Explode();
    }
    
    private void Explode()
    {
        float explosionRadius = currentSize * explosionRadiusMultiplier;
        
        Collider[] obstacles = Physics.OverlapSphere(transform.position, explosionRadius, obstacleLayer);
        
        foreach (Collider col in obstacles)
        {
            if (col.TryGetComponent<Obstacle>(out var obstacle) && !obstacle.IsDestroyed)
                obstacle.Destroy();
        }
        
        Destroy(gameObject);
    }
    
    private void OnDrawGizmosSelected()
    {
        if (isLaunched)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, currentSize * explosionRadiusMultiplier);
        }
    }
}

