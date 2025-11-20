using UnityEngine;
using Zenject;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;

public class PlayerBall : MonoBehaviour
{
    private const float SHOT_SIZE_SEPARATION_MULTIPLIER = 0.5f;
    private const float INITIAL_CHARGE_TIME = 0f;

    [Header("Size Settings")]
    [SerializeField] private float initialSize = GameConstants.INITIAL_PLAYER_SIZE;
    [SerializeField] private float minSize = 0.2f;
    
    [Header("Shot Settings")]
    [SerializeField] private float shotGrowthRate = 0.5f;
    [SerializeField] private Transform target;
    [SerializeField] private float shotSeparationDistance = 0.5f;
    
    [Header("Movement")]
    [SerializeField] private float jumpSpeed = 5f;
    
    private float currentSize;
    private float totalShotSize;
    private bool isChargingShot;
    private float chargeTime;
    private ShotBall currentShot;
    private ShotBall activeLaunchedShot;
    private bool canMove;
    private PathManager pathManager;
    private IGameStateHandler gameStateHandler;
    private GameManager gameManager;
    private Tween movementTween;
    private ShotBall.Factory shotFactory;
    
    [Inject]
    private void Construct(PathManager pathManager, IGameStateHandler gameStateHandler, GameManager gameManager, ShotBall.Factory shotFactory)
    {
        this.pathManager = pathManager;
        this.gameStateHandler = gameStateHandler;
        this.gameManager = gameManager;
        this.shotFactory = shotFactory;
    }
    
    private bool IsGameEnded() => gameManager && gameManager.IsGameEnded;
    
    private void Start()
    {
        currentSize = initialSize;
        UpdateSize();
        
        pathManager?.UpdatePathVisual();
        
        if (!HasEnoughSizeForPassage())
        {
            Debug.LogWarning("Initial player ball size is insufficient! Need to increase initialSize.");
        }
    }
    
    private void OnDestroy()
    {
        movementTween?.Kill();
    }
    
    private void Update()
    {
        if (IsGameEnded())
        {
            StopMovement();
            return;
        }
        
        HandleInput();
        
        if (!canMove && pathManager && pathManager.IsPathClear() && !isChargingShot)
        {
            EnableMovement();
        }
    }
    
    private void HandleInput()
    {
        if (IsGameEnded())
            return;
        
        if (Input.GetMouseButtonDown(0))
        {
            StartChargingShot();
        }
        else if (Input.GetMouseButton(0))
        {
            ContinueChargingShot();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            ReleaseShot();
        }
    }
    
    private void StartChargingShot()
    {
        if (currentSize <= minSize || target == null || shotFactory == null)
            return;
        
        if (canMove || isChargingShot || HasActiveShot())
            return;
        
        canMove = false;
        movementTween?.Kill();
        
        isChargingShot = true;
        chargeTime = INITIAL_CHARGE_TIME;
        
        Vector3 shotPosition = transform.position + GetDirectionToTarget() * shotSeparationDistance;
        
        currentShot = shotFactory.Create();
        currentShot.transform.position = shotPosition;
        currentShot.Initialize(target.position);
    }
    
    private bool HasActiveShot() => (isChargingShot && currentShot) || activeLaunchedShot;
    
    private void ContinueChargingShot()
    {
        if (!isChargingShot || currentShot == null || target == null)
            return;
        
        chargeTime += Time.deltaTime;
        
        float availableSize = currentSize - minSize;
        float shotSize = Mathf.Min(chargeTime * shotGrowthRate, availableSize);
        
        currentShot.SetSize(shotSize);
        
        UpdateShotPosition(shotSize);
        
        currentSize = Mathf.Max(initialSize - totalShotSize - shotSize, minSize);
        UpdateSize();
        
        if (currentSize <= minSize)
            gameStateHandler?.OnPlayerTooSmall();
    }
    
    private Vector3 GetDirectionToTarget() => (target.position - transform.position).normalized;
    
    private void UpdateShotPosition(float shotSize)
    {
        float separation = shotSeparationDistance + shotSize * SHOT_SIZE_SEPARATION_MULTIPLIER;
        currentShot.transform.position = transform.position + GetDirectionToTarget() * separation;
    }
    
    private void ReleaseShot()
    {
        if (!isChargingShot || currentShot == null)
            return;
        
        float availableSize = initialSize - totalShotSize - minSize;
        float shotSize = Mathf.Min(chargeTime * shotGrowthRate, availableSize);
        
        totalShotSize += shotSize;
        currentSize = Mathf.Max(initialSize - totalShotSize, minSize);
        UpdateSize();
        
        currentShot.SetSize(shotSize);
        
        isChargingShot = false;
        currentShot.Launch();
        
        activeLaunchedShot = currentShot;
        currentShot = null;
        chargeTime = INITIAL_CHARGE_TIME;
        
        WaitForShotDestroyAsync(activeLaunchedShot, this.GetCancellationTokenOnDestroy()).Forget();
    }
    
    private async UniTask WaitForShotDestroyAsync(ShotBall shot, CancellationToken cancellationToken)
    {
        while (shot != null && !cancellationToken.IsCancellationRequested)
        {
            await UniTask.Yield(cancellationToken);
        }
        
        if (!cancellationToken.IsCancellationRequested && activeLaunchedShot == shot)
        {
            activeLaunchedShot = null;
        }
    }
    
    private void UpdateSize()
    {
        transform.localScale = Vector3.one * currentSize;
        pathManager?.UpdatePathWidth(currentSize);
    }
    
    public void EnableMovement()
    {
        if (target == null || pathManager == null || IsGameEnded())
            return;
        
        canMove = true;
        MoveToTarget();
    }
    
    private void MoveToTarget()
    {
        if (target == null || pathManager == null || !canMove || IsGameEnded())
        {
            StopMovement();
            return;
        }
        
        movementTween?.Kill();
        
        Vector3 currentPos = transform.position;
        Vector3 targetPos = target.position;
        targetPos.y = currentPos.y;
        
        float distance = Vector3.Distance(currentPos, targetPos);
        
        if (distance < GameConstants.TARGET_REACH_DISTANCE)
        {
            gameStateHandler?.OnPlayerReachedTarget();
            return;
        }
        
        if (!pathManager.IsPathClear())
        {
            canMove = false;
            return;
        }
        
        float duration = distance / jumpSpeed;
        
        movementTween = transform.DOMove(targetPos, duration)
            .SetEase(Ease.Linear)
            .SetUpdate(UpdateType.Normal, true)
            .OnUpdate(() =>
            {
                if (IsGameEnded() || !canMove || pathManager == null || !pathManager.IsPathClear())
                {
                    StopMovement();
                    return;
                }
                
                if (IsTargetReached())
                {
                    StopMovement();
                    gameStateHandler?.OnPlayerReachedTarget();
                }
            })
            .OnComplete(() =>
            {
                if (IsTargetReached())
                {
                    gameStateHandler?.OnPlayerReachedTarget();
                }
                else
                {
                    canMove = false;
                }
            });
    }
    
    public void StopMovement()
    {
        movementTween?.Kill();
        canMove = false;
    }
    
    public bool HasEnoughSizeForPassage()
    {
        if (pathManager == null)
            return currentSize >= minSize;
            
        return currentSize >= pathManager.GetRequiredSize() * GameConstants.SIZE_RESERVE_MULTIPLIER;
    }
    
    private bool IsTargetReached()
    {
        if (target == null)
            return false;
            
        return Vector3.Distance(transform.position, target.position) < GameConstants.TARGET_REACH_DISTANCE;
    }
    
    public bool CanCreateMoreShots()
    {
        return currentSize > minSize;
    }
}

