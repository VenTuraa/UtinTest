using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    [Header("References")] [SerializeField]
    private GameObject shotPrefab;

    [SerializeField] private PathManager pathManager;
    [SerializeField] private GameManager gameManager;

    public override void InstallBindings()
    {
        Container.Bind<GameManager>()
            .FromInstance(gameManager)
            .AsSingle()
            .NonLazy();

        Container.Bind<PathManager>()
            .FromInstance(pathManager)
            .AsSingle();

        Container.Bind<PlayerBall>()
            .FromComponentInHierarchy()
            .AsSingle();

        Container.BindFactory<ShotBall, ShotBall.Factory>()
            .FromComponentInNewPrefab(shotPrefab)
            .AsSingle();

        Container.BindInterfacesAndSelfTo<GameStateHandler>()
            .AsSingle();
    }
}