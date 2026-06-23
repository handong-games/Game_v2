using Game.Core.Composition;
using Gameplay.GAS;
using VContainer;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class GameBootstrap
{
    private const string GameplayCueSetResourcePath = "Gameplay/GAS/RuntimeGameplayCueSet";
    private static RootLifetimeScope _rootLifetimeScope;

    public static RootLifetimeScope RootLifetimeScope => _rootLifetimeScope;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        DisposeRootLifetimeScope();
        
        // 전역 리소스 로드
        Addressables.LoadAssetsAsync<UnityEngine.Object>("Preload", null).WaitForCompletion();
        
        InitializeGameplayCues();
        CreateRootLifetimeScope();
    }

    private static void CreateRootLifetimeScope()
    {
        if (_rootLifetimeScope != null)
            return;

        GameObject scopeObject = new GameObject("@RootLifetimeScope");
        Object.DontDestroyOnLoad(scopeObject);
        _rootLifetimeScope = scopeObject.AddComponent<RootLifetimeScope>();
    }

    private static void DisposeRootLifetimeScope()
    {
        if (_rootLifetimeScope == null)
            return;

        Object.Destroy(_rootLifetimeScope.gameObject);
        _rootLifetimeScope = null;
    }

    public static T ResolveRoot<T>()
    {
        if (_rootLifetimeScope == null || _rootLifetimeScope.Container == null)
        {
            throw new System.InvalidOperationException(
                $"RootLifetimeScope is not ready. Cannot resolve {typeof(T).Name}.");
        }

        return _rootLifetimeScope.Container.Resolve<T>();
    }

    private static void InitializeGameplayCues()
    {
        GameplayCueSet cueSet = Resources.Load<GameplayCueSet>(GameplayCueSetResourcePath);
        if (cueSet == null)
        {
            Debug.LogError($"GameplayCueSet not found: {GameplayCueSetResourcePath}");
            return;
        }

        GameplayCueManager.Instance.Initialize(cueSet);
    }
}
