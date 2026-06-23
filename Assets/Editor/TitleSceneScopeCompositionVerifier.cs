using System;
using Domains.CharacterSelect;
using Game.Core.Composition;
using Game.Core.Managers.Dependency.Generated;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Views.TitleView;

public static class TitleSceneScopeCompositionVerifier
{
    private const string ScenePath = "Assets/Scenes/TitleScene.unity";
    private const string ScopeObjectName = "@TitleSceneScope";

    public static void Run()
    {
        try
        {
            VerifyDependencyManagerDoesNotOwnTitleSceneScopedTypes();
            VerifyTitleSceneScopePlacement();
            Debug.Log("TitleSceneScope composition verification passed.");
            EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
    }

    private static void VerifyDependencyManagerDoesNotOwnTitleSceneScopedTypes()
    {
        for (int i = 0; i < DependencyRegistry.All.Length; i++)
        {
            Type type = DependencyRegistry.All[i].Type;
            if (type == typeof(TitleViewController) ||
                type == typeof(CharacterSelectController))
            {
                throw new InvalidOperationException(
                    $"{type.Name} must not be registered in DependencyManager while TitleSceneScope owns the TitleScene path.");
            }
        }
    }

    private static void VerifyTitleSceneScopePlacement()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        TitleSceneScope[] scopes = UnityEngine.Object.FindObjectsByType<TitleSceneScope>(FindObjectsInactive.Include);
        if (scopes.Length != 1)
            throw new InvalidOperationException($"Expected exactly one TitleSceneScope in {ScenePath}, found {scopes.Length}.");

        TitleSceneScope scope = scopes[0];
        if (scope.gameObject.name != ScopeObjectName)
        {
            throw new InvalidOperationException(
                $"TitleSceneScope must be placed on {ScopeObjectName}, but was found on {scope.gameObject.name}.");
        }

        GameObject scopeObject = GameObject.Find(ScopeObjectName);
        if (scopeObject == null)
            throw new InvalidOperationException($"{ScopeObjectName} GameObject was not found in {ScenePath}.");

        if (scopeObject.GetComponent<TitleSceneScope>() == null)
            throw new InvalidOperationException($"{ScopeObjectName} does not contain TitleSceneScope.");
    }
}
