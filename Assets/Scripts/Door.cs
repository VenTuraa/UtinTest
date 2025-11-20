using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;
using System.Threading;

public class Door : MonoBehaviour
{
    private const float MIN_DISTANCE_THRESHOLD = 0.01f;

    [Header("Settings")]
    [SerializeField] private float openDistance = 5f;
    [SerializeField] private float openSpeed = 2f;
    [SerializeField] private Vector3 openOffset = new Vector3(0, 3, 0);
    
    [Header("References")]
    [SerializeField] private Transform doorTransform;
    
    private Vector3 openPosition;
    private bool isOpen;
    private CancellationTokenSource openCts;
    private PlayerBall playerBall;
    
    [Inject]
    private void Construct(PlayerBall playerBall)
    {
        this.playerBall = playerBall;
    }
    
    private void Start()
    {
        if (doorTransform == null)
            doorTransform = transform;
            
        openPosition = doorTransform.position + openOffset;
        CheckPlayerDistanceAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }
    
    private void OnDestroy()
    {
        openCts?.Cancel();
        openCts?.Dispose();
    }
    
    private async UniTask CheckPlayerDistanceAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && !isOpen)
        {
            if (playerBall != null)
            {
                float distance = Vector3.Distance(transform.position, playerBall.transform.position);
                if (distance <= openDistance)
                {
                    await OpenDoorAsync(cancellationToken);
                    break;
                }
            }
            
            await UniTask.Delay(GameConstants.DOOR_CHECK_INTERVAL_MS, cancellationToken: cancellationToken);
        }
    }
    
    private async UniTask OpenDoorAsync(CancellationToken cancellationToken)
    {
        if (isOpen || doorTransform == null)
            return;
        
        openCts?.Cancel();
        openCts?.Dispose();
        openCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        
        float distance = Vector3.Distance(doorTransform.position, openPosition);
        float duration = distance / openSpeed;
        float elapsedTime = 0f;
        Vector3 startPos = doorTransform.position;
        
        try
        {
            while (elapsedTime < duration && !openCts.Token.IsCancellationRequested)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);
                doorTransform.position = Vector3.Lerp(startPos, openPosition, t);
                
                await UniTask.Yield(openCts.Token);
            }
            
            if (!openCts.Token.IsCancellationRequested && doorTransform != null)
            {
                if (Vector3.Distance(doorTransform.position, openPosition) > MIN_DISTANCE_THRESHOLD)
                    doorTransform.position = openPosition;
                    
                isOpen = true;
            }
        }
        finally
        {
            openCts?.Dispose();
        }
    }
}


