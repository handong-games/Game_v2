using System;
using System.IO;
using Domains.Adventure;
using Domains.CharacterSelect;
using Game.Core.Composition;
using Game.Generated;
using Game.Scenes.Adventure;
using Game.Scenes.Adventure.Events.Widgets;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

[InitializeOnLoad]
internal static class CodexPlayModeVerifier
{
    private const string TriggerPath = "Library/CodexPlayModeVerifier.trigger";
    private const string ResultPath = "Logs/CodexPlayModeVerifier.result.txt";
    private const string TitleScenePath = "Assets/Scenes/TitleScene.unity";
    private const string AdventureSceneName = "AdventureScene";
    private const double PlayRequestTimeoutSeconds = 90.0;
    private const double DriverTimeoutSeconds = 90.0;
    private const double ErrorObservationSeconds = 8.0;

    private enum Phase
    {
        Idle,
        PreparingTitleScene,
        RequestingPlayMode,
        WaitingForTitleScope,
        StartingAdventure,
        WaitingForAdventureScene,
        WaitingForAdventureScope,
        ClickingFirstChoice,
        WaitingForChoiceInteraction,
        ObservingAdventure,
        Completed,
        Failed
    }

    private static Phase _phase;
    private static bool _failed;
    private static double _phaseStartedAt;
    private static double _observationStartedAt;
    private static uint _clickedOfferCardId;

    static CodexPlayModeVerifier()
    {
        EditorApplication.update += OnEditorUpdate;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        Application.logMessageReceived += OnLogMessageReceived;
    }

    private static void OnEditorUpdate()
    {
        if (_phase is Phase.Idle or Phase.Completed or Phase.Failed &&
            File.Exists(TriggerPath))
        {
            Begin();
        }

        if (_phase == Phase.Idle || _phase == Phase.Completed || _phase == Phase.Failed)
            return;

        try
        {
            Tick();
        }
        catch (Exception exception)
        {
            Fail($"{exception.GetType().Name}: {exception.Message}\n{exception.StackTrace}");
        }
    }

    private static void Begin()
    {
        Directory.CreateDirectory("Logs");
        File.WriteAllText(ResultPath, "Starting Adventure Play Mode verification.\n");
        _failed = false;

        if (EditorApplication.isPlaying)
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.path == TitleScenePath)
            {
                Append("Editor is already in Play Mode on TitleScene.");
                EnterPhase(Phase.WaitingForTitleScope);
                return;
            }

            Append($"Editor is already in Play Mode on {activeScene.name}; exiting to reopen TitleScene.");
            EnterPhase(Phase.PreparingTitleScene);
            EditorApplication.ExitPlaymode();
            return;
        }

        EnterPhase(Phase.PreparingTitleScene);
    }

    private static void Tick()
    {
        switch (_phase)
        {
            case Phase.PreparingTitleScene:
                PrepareTitleScene();
                break;
            case Phase.RequestingPlayMode:
                CheckPlayModeRequestTimeout();
                break;
            case Phase.WaitingForTitleScope:
                WaitForTitleScopeAndStartAdventure();
                break;
            case Phase.StartingAdventure:
                EnterPhase(Phase.WaitingForAdventureScene);
                break;
            case Phase.WaitingForAdventureScene:
                WaitForAdventureScene();
                break;
            case Phase.WaitingForAdventureScope:
                WaitForAdventureScope();
                break;
            case Phase.ClickingFirstChoice:
                ClickFirstChoice();
                break;
            case Phase.WaitingForChoiceInteraction:
                WaitForChoiceInteraction();
                break;
            case Phase.ObservingAdventure:
                ObserveAdventure();
                break;
        }
    }

    private static void PrepareTitleScene()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            return;

        if (EditorApplication.isPlaying)
            return;

        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.path != TitleScenePath)
        {
            Append($"Opening {TitleScenePath}.");
            EditorSceneManager.OpenScene(TitleScenePath, OpenSceneMode.Single);
            return;
        }

        EnterPhase(Phase.RequestingPlayMode);
        EditorApplication.EnterPlaymode();
    }

    private static void CheckPlayModeRequestTimeout()
    {
        if (EditorApplication.isPlaying)
            return;

        if (ElapsedInPhase() > PlayRequestTimeoutSeconds)
        {
            Fail("Play Mode did not start before timeout.");
        }
    }

    private static void WaitForTitleScopeAndStartAdventure()
    {
        TitleSceneScope titleScope = UnityEngine.Object.FindAnyObjectByType<TitleSceneScope>(
            FindObjectsInactive.Include);

        if (titleScope == null || titleScope.Container == null ||
            GameBootstrap.RootLifetimeScope == null ||
            GameBootstrap.RootLifetimeScope.Container == null)
        {
            CheckDriverTimeout("TitleSceneScope was not ready before timeout.");
            return;
        }

        CharacterSelectController characterSelectController =
            titleScope.Container.Resolve<CharacterSelectController>();
        AdventureStartState startState =
            GameBootstrap.RootLifetimeScope.Container.Resolve<AdventureStartState>();

        startState.SelectedCharacterId = ECharacter.Warrior;
        Append("Starting Adventure through CharacterSelectController.StartNewAdventure(ECharacter.Warrior).");
        characterSelectController.StartNewAdventure(ECharacter.Warrior);
        EnterPhase(Phase.StartingAdventure);
    }

    private static void WaitForAdventureScene()
    {
        if (SceneManager.GetActiveScene().name == AdventureSceneName)
        {
            Append("AdventureScene is active.");
            EnterPhase(Phase.WaitingForAdventureScope);
            return;
        }

        CheckDriverTimeout("AdventureScene did not become active before timeout.");
    }

    private static void WaitForAdventureScope()
    {
        AdventureSceneScope adventureScope = UnityEngine.Object.FindAnyObjectByType<AdventureSceneScope>(
            FindObjectsInactive.Include);

        if (adventureScope == null || adventureScope.Container == null)
        {
            CheckDriverTimeout("AdventureSceneScope was not ready before timeout.");
            return;
        }

        AdventureView adventureView = adventureScope.Container.Resolve<AdventureView>();
        if (adventureView.Root == null)
        {
            CheckDriverTimeout("AdventureView root was not attached before timeout.");
            return;
        }

        Append("AdventureSceneScope and AdventureView are ready.");
        EnterPhase(Phase.ClickingFirstChoice);
    }

    private static void ClickFirstChoice()
    {
        AdventureSceneScope adventureScope = GetAdventureScope();
        if (adventureScope == null)
        {
            CheckDriverTimeout("AdventureSceneScope disappeared before choice interaction.");
            return;
        }

        AdventureStageRuntime stage = adventureScope.Container.Resolve<AdventureStageRuntime>();
        AdventureWidgetEvents widgetEvents = adventureScope.Container.Resolve<AdventureWidgetEvents>();

        if (stage.Bindings.Count == 0)
        {
            CheckDriverTimeout("No Adventure choice bindings were available.");
            return;
        }

        if (widgetEvents.Card.Clicked == null)
        {
            Fail("AdventureWidgetEvents.Card.Clicked is not bound.");
            return;
        }

        AdventureStageOfferBinding binding = FindPreferredChoice(stage);
        _clickedOfferCardId = binding.OfferCardId;

        Append(
            $"Clicking choice card {_clickedOfferCardId} ({binding.Offer.EncounterType}).");
        widgetEvents.Card.Clicked.Invoke(null, _clickedOfferCardId);
        EnterPhase(Phase.WaitingForChoiceInteraction);
    }

    private static void WaitForChoiceInteraction()
    {
        AdventureSceneScope adventureScope = GetAdventureScope();
        if (adventureScope == null)
        {
            CheckDriverTimeout("AdventureSceneScope disappeared after choice click.");
            return;
        }

        AdventureProgress progress = adventureScope.Container.Resolve<AdventureProgress>();
        AdventureCombatRuntime combat = adventureScope.Container.Resolve<AdventureCombatRuntime>();

        if (progress.CurrentPhase == AdventurePhase.Combat &&
            ContainsCardId(combat.EnemyCardIds, _clickedOfferCardId))
        {
            Append("Choice click reached Combat phase and selected card became enemy combat card.");
            _observationStartedAt = EditorApplication.timeSinceStartup;
            EnterPhase(Phase.ObservingAdventure);
            return;
        }

        CheckDriverTimeout(
            $"Choice click did not reach Combat phase before timeout. CurrentPhase={progress.CurrentPhase}.");
    }

    private static void ObserveAdventure()
    {
        if (EditorApplication.timeSinceStartup - _observationStartedAt < ErrorObservationSeconds)
            return;

        Complete(_failed ? "FAILED" : "PASSED");
    }

    private static AdventureStageOfferBinding FindPreferredChoice(AdventureStageRuntime stage)
    {
        AdventureStageOfferBinding first = stage.Bindings[0];
        for (int i = 0; i < stage.Bindings.Count; i++)
        {
            AdventureStageOfferBinding binding = stage.Bindings[i];
            if (binding.Offer.EncounterType is AdventureEncounterType.Combat or AdventureEncounterType.Boss)
                return binding;
        }

        return first;
    }

    private static bool ContainsCardId(
        System.Collections.Generic.IReadOnlyList<uint> cardIds,
        uint cardId)
    {
        for (int i = 0; i < cardIds.Count; i++)
        {
            if (cardIds[i] == cardId)
                return true;
        }

        return false;
    }

    private static AdventureSceneScope GetAdventureScope()
    {
        AdventureSceneScope adventureScope = UnityEngine.Object.FindAnyObjectByType<AdventureSceneScope>(
            FindObjectsInactive.Include);

        if (adventureScope == null || adventureScope.Container == null)
            return null;

        return adventureScope;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (_phase == Phase.Idle)
            return;

        Append($"PlayModeStateChanged: {state}.");

        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            EnterPhase(Phase.WaitingForTitleScope);
            return;
        }

        if (state == PlayModeStateChange.EnteredEditMode &&
            _phase is Phase.PreparingTitleScene or Phase.RequestingPlayMode)
        {
            EnterPhase(Phase.PreparingTitleScene);
            return;
        }

        if (state == PlayModeStateChange.EnteredEditMode &&
            _phase is not (Phase.Completed or Phase.Failed or Phase.Idle))
        {
            Fail("Returned to Edit Mode before verification completed.");
        }
    }

    private static void OnLogMessageReceived(string condition, string stackTrace, LogType type)
    {
        if (_phase == Phase.Idle || type is not (LogType.Error or LogType.Exception or LogType.Assert))
            return;

        _failed = true;
        Append($"{type}: {condition}\n{stackTrace}");
    }

    private static void CheckDriverTimeout(string message)
    {
        if (ElapsedInPhase() > DriverTimeoutSeconds)
        {
            Fail(message);
        }
    }

    private static void EnterPhase(Phase phase)
    {
        _phase = phase;
        _phaseStartedAt = EditorApplication.timeSinceStartup;
        Append($"Phase: {phase}.");
    }

    private static double ElapsedInPhase()
    {
        return EditorApplication.timeSinceStartup - _phaseStartedAt;
    }

    private static void Complete(string result)
    {
        Append(result);
        File.Delete(TriggerPath);

        if (EditorApplication.isPlaying)
            EditorApplication.ExitPlaymode();

        _phase = Phase.Completed;
    }

    private static void Fail(string message)
    {
        Append("FAILED");
        Append(message);
        File.Delete(TriggerPath);

        if (EditorApplication.isPlaying)
            EditorApplication.ExitPlaymode();

        _phase = Phase.Failed;
    }

    private static void Append(string message)
    {
        File.AppendAllText(ResultPath, $"{message}{Environment.NewLine}");
    }
}
