using Domains.Settings;
using Game.Core.Managers.Dependency;
using Game.System.Core.Manager;
using Gameplay.GAS;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class GameBootstrap
{
    private const string GameplayCueSetResourcePath = "Gameplay/GAS/RuntimeGameplayCueSet";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        ManagerRegistry.AllDispose();
        
        // 전역 리소스 로드
        Addressables.LoadAssetsAsync<UnityEngine.Object>("Preload", null).WaitForCompletion();
        
        // 매니저 생성
        GameObject root = new GameObject("@Managers");
        Object.DontDestroyOnLoad(root);
        ManagerRegistry.AllInit();
        InitializeGameplayCues();
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