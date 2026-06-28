using System;
using Game.Scenes.Adventure;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class AdventureSceneScopeCompositionVerifier
{
    private const string ScenePath = "Assets/Scenes/AdventureScene.unity";
    private const string ScopeObjectName = "@AdventureSceneScope";

    public static void Run()
    {
        try
        {
            VerifyAdventureSceneScopePlacement();
            Debug.Log("AdventureSceneScope composition verification passed.");
            EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
    }

    private static void VerifyAdventureSceneScopePlacement()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        AdventureSceneScope[] scopes = UnityEngine.Object.FindObjectsByType<AdventureSceneScope>(FindObjectsInactive.Include);
        if (scopes.Length != 1)
            throw new InvalidOperationException($"Expected exactly one AdventureSceneScope in {ScenePath}, found {scopes.Length}.");

        AdventureSceneScope scope = scopes[0];
        if (scope.gameObject.name != ScopeObjectName)
        {
            throw new InvalidOperationException(
                $"AdventureSceneScope must be placed on {ScopeObjectName}, but was found on {scope.gameObject.name}.");
        }

        GameObject scopeObject = GameObject.Find(ScopeObjectName);
        if (scopeObject == null)
            throw new InvalidOperationException($"{ScopeObjectName} GameObject was not found in {ScenePath}.");

        if (scopeObject.GetComponent<AdventureSceneScope>() == null)
            throw new InvalidOperationException($"{ScopeObjectName} does not contain AdventureSceneScope.");
    }
}
