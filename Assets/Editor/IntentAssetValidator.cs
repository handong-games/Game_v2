using System;
using System.Collections.Generic;
using Domains.Intent.Data;
using Game.Data;
using UnityEditor;
using UnityEngine;

// Role:
// Validates authored Intent assets that must exist before Adventure combat can run safely.
public static class IntentAssetValidator
{
    private static readonly string[] MonsterFolders =
    {
        "Assets/@Resources/Model/Monsters",
    };

    private static readonly string[] MonsterActionFolders =
    {
        "Assets/@Resources/Model/MonsterActions",
    };

    private static readonly string[] IntentDisplayFolders =
    {
        "Assets/@Resources/Model/Intents",
    };

    [MenuItem("Tools/Codex/Validate Intent Assets")]
    public static void ValidateFromMenu()
    {
        IReadOnlyList<string> errors = Validate();
        if (errors.Count == 0)
        {
            Debug.Log("Intent asset validation passed.");
            return;
        }

        string message = string.Join(Environment.NewLine, errors);
        Debug.LogError(message);
        throw new InvalidOperationException(message);
    }

    public static IReadOnlyList<string> Validate()
    {
        List<string> errors = new();
        HashSet<string> validatedActionPaths = new();
        HashSet<string> validatedDisplayPaths = new();

        ValidateMonsters(errors, validatedActionPaths, validatedDisplayPaths);
        ValidateMonsterActions(errors, validatedActionPaths, validatedDisplayPaths);
        ValidateIntentDisplays(errors, validatedDisplayPaths);

        return errors;
    }

    private static void ValidateMonsters(
        List<string> errors,
        HashSet<string> validatedActionPaths,
        HashSet<string> validatedDisplayPaths)
    {
        foreach (MonsterModel monster in LoadAssets<MonsterModel>(MonsterFolders))
        {
            string path = AssetDatabase.GetAssetPath(monster);
            IReadOnlyList<MonsterActionModel> actionSequence = monster.ActionSequence;
            if (actionSequence.Count == 0)
            {
                errors.Add($"{monster.name}: action sequence is empty in {path}.");
                continue;
            }

            for (int i = 0; i < actionSequence.Count; i++)
            {
                MonsterActionModel action = actionSequence[i];
                if (action == null)
                {
                    errors.Add($"{monster.name}: action sequence has null action at index {i} in {path}.");
                    continue;
                }

                ValidateMonsterAction(action, errors, validatedActionPaths, validatedDisplayPaths);
            }
        }
    }

    private static void ValidateMonsterActions(
        List<string> errors,
        HashSet<string> validatedActionPaths,
        HashSet<string> validatedDisplayPaths)
    {
        foreach (MonsterActionModel action in LoadAssets<MonsterActionModel>(MonsterActionFolders))
        {
            ValidateMonsterAction(action, errors, validatedActionPaths, validatedDisplayPaths);
        }
    }

    private static void ValidateMonsterAction(
        MonsterActionModel action,
        List<string> errors,
        HashSet<string> validatedActionPaths,
        HashSet<string> validatedDisplayPaths)
    {
        string path = AssetDatabase.GetAssetPath(action);
        if (!validatedActionPaths.Add(path))
            return;

        if (action.ExecutionAbility == null)
            errors.Add($"{action.name}: execution ability is missing in {path}.");

        IReadOnlyList<IntentDisplayDefinition> displays = action.IntentDisplays;
        if (displays.Count == 0)
        {
            errors.Add($"{action.name}: intent display list is empty in {path}.");
            return;
        }

        for (int i = 0; i < displays.Count; i++)
        {
            IntentDisplayDefinition display = displays[i];
            if (display == null)
            {
                errors.Add($"{action.name}: intent display definition is null at index {i} in {path}.");
                continue;
            }

            IntentDisplayModel displayModel = display.DisplayModel;
            if (displayModel == null)
            {
                errors.Add($"{action.name}: intent display model is missing at index {i} in {path}.");
                continue;
            }

            if (displayModel.RequiresNumber && display.NumberRule == null)
            {
                errors.Add(
                    $"{action.name}: number rule is missing for {displayModel.name} at index {i} in {path}.");
            }

            ValidateIntentDisplay(displayModel, errors, validatedDisplayPaths);
        }
    }

    private static void ValidateIntentDisplays(
        List<string> errors,
        HashSet<string> validatedDisplayPaths)
    {
        foreach (IntentDisplayModel display in LoadAssets<IntentDisplayModel>(IntentDisplayFolders))
        {
            ValidateIntentDisplay(display, errors, validatedDisplayPaths);
        }
    }

    private static void ValidateIntentDisplay(
        IntentDisplayModel display,
        List<string> errors,
        HashSet<string> validatedDisplayPaths)
    {
        string path = AssetDatabase.GetAssetPath(display);
        if (!validatedDisplayPaths.Add(path))
            return;

        if (display.Icon == null)
            errors.Add($"{display.name}: icon is missing in {path}.");
    }

    private static IEnumerable<T> LoadAssets<T>(string[] folders)
        where T : UnityEngine.Object
    {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", folders);
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
                yield return asset;
        }
    }
}
