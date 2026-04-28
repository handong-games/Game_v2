using Domains.Settings;
using Game.Core.Managers.Dependency;
using Game.System.Core.Manager;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class GameBootstrap
{
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
    }
}



