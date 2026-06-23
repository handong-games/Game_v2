# TitleScene VContainer Lifecycle Design

## Current Status Note

Read this document together with `docs/vcontainer-adoption-doc-index.md` and `docs/title-scene-vcontainer-implementation-phases.md`.

Current applied state:

```text
TitleScene no longer directly starts TitleScene startup.
TitleScene contains a scene-placed @TitleSceneScope component.
TitleSceneScope registers TitleSceneEntryPoint.
CharacterSelectController is registered in TitleSceneScope.
TitleViewController has been removed from DependencyManager and is registered in TitleSceneScope.
TitleSceneEntryPoint delegates initial TitleView display to ITitleSceneNavigator.
TitleViewController delegates CharacterSelect/Settings navigation to ITitleSceneNavigator.
TitleSceneEntryPoint contains the startup sequence directly.
TitleSceneScope inherits root ports from RootLifetimeScope where still needed.
TitleSceneScope uses ISceneViewNavigator for TitleScene-scoped views.
TitleScene delegates fade-out to ISceneTransitionPlayer resolved from RootLifetimeScope.
ViewManager still owns transient view lifetime for unmigrated scenes.
```

## Purpose

This document narrows the VContainer migration design to the current `TitleScene` flow.

The goal is to keep the connected `TitleSceneScope` boundary honest: define which objects now belong to it, which objects must stay outside it, and which current ownership problems still need migration.

Architecture decision:

```text
Unity Scene + VContainer LifetimeScope is now the adopted target direction.
The current pure C# BaseScene path remains only as a temporary bridge.
```

## Top-Down Decisions Before TitleScene

The VContainer migration starts conceptually at `GameBootstrap`, not at `TitleScene`.

Confirmed direction:

```text
GameBootstrap
-> remains the application boot orchestrator

ManagerRegistry
-> remains the legacy root for now
-> keeps current Create / Init / PostInit / Dispose order

RootLifetimeScope
-> follows the application lifetime
-> does not create existing managers in the first phase
-> registers narrow adapters and factories for child scopes
-> is exposed by GameBootstrap so scene scopes can use the intended root parent
```

Cold assessment:

```text
Moving every manager into RootLifetimeScope is possible later, but it is not the first target.
The current managers are coupled to static Instance access, ManagerRegistry ordering, Unity scene events, and DontDestroyOnLoad objects.
Replacing that immediately would turn the VContainer migration into a full boot architecture rewrite.
```

Root first-phase registration is limited to boundaries needed by TitleScene:

```text
IAudioPlayer
IViewHost
ISceneLoader
ISceneTransitionPlayer
IViewFactory
```

Recommended folder placement:

```text
Assets/@Scripts/Core/Composition/
-> RootLifetimeScope.cs
-> SceneLifetimeScope.cs
-> AdventureSessionLifetimeScope.cs
-> TitleSceneScope.cs

Assets/@Scripts/Core/Ports/
-> IAudioPlayer.cs
-> IViewHost.cs
-> ISceneLoader.cs
-> ISceneTransitionPlayer.cs

Assets/@Scripts/Core/Adapters/
-> AudioManagerAdapter.cs
-> UnitySceneLoader.cs
-> ViewOverlaySceneTransitionPlayer.cs

Assets/@Scripts/Core/Manager/View/
-> IViewFactory.cs
-> LegacyViewFactory.cs
-> ViewManager.cs implements IViewHost directly

Assets/@Scripts/Domains/Scene/Title/
-> TitleSceneBgmOwner.cs
-> TitleSceneLocalizationOwner.cs
-> TitleSceneNavigator.cs
-> ITitleSceneNavigator.cs
```

Reason:

```text
Composition contains VContainer registration roots.
Ports contain narrow interfaces used by VContainer-owned objects.
Adapters contain legacy Manager.Instance access.
Title scene owners/flows stay in the Title scene domain.
```

Do not create a large architecture folder hierarchy before the migration proves it needs one.

Current skeleton status:

```text
Created:
- Core/Ports interfaces
- Core/Adapters legacy adapters
- Domains/Scene/Title owner and view flow classes
- TitleSceneScope scene composition root

Connected through TitleSceneScope registration:
- TitleSceneScope owns TitleSceneLocalizationOwner.
- TitleSceneScope owns TitleSceneBgmOwner with IAudioPlayer inherited from RootLifetimeScope.
- TitleSceneScope owns ITitleSceneNavigator / TitleSceneNavigator with ISceneViewNavigator.
- TitleSceneEntryPoint owns startup ordering directly.
- TitleScene resolves ISceneTransitionPlayer from RootLifetimeScope for fade-out.
- CharacterSelectController is registered in TitleSceneScope and injected into CharacterSelectView through the constructor.
- LegacyAdventureSessionStarter exposes a constructor-injection path, is no longer registered in DependencyManager, but still bridges to legacy DependencyManager-owned adventure services.
- TitleScene views are registered as scoped objects and displayed through SceneViewNavigator.

- GameBootstrap creates RootLifetimeScope after ManagerRegistry initialization.
- TitleSceneScope parent lookup targets the GameBootstrap-created RootLifetimeScope.
- The Unity TitleScene asset contains @TitleSceneScope, so TitleSceneEntryPoint can start the scene.
```

Adapter rule:

```text
Adapters are legacy bridges, not permanent architecture.
Example: IAudioPlayer -> AudioManagerAdapter -> AudioManager.Instance.
When a VContainer-native implementation exists, the registration can change to IAudioPlayer -> VContainerAudioPlayer.
The adapter is removed only after no code path still depends on it.
```

Do not create one adapter per manager by default. Add adapters only when a VContainer-owned object needs a narrow boundary.

## Current TitleScene Flow

Current load path:

```text
TitleScene.OnLoaded
-> no direct startup work

TitleSceneScope
-> TitleSceneEntryPoint.Start()
-> TitleSceneLocalizationOwner preloads localization tables
-> TitleSceneBgmOwner loads TitleMenuBgm through Addressables
-> IAudioPlayer is inherited from RootLifetimeScope
-> AudioManagerAdapter asks AudioManager to play BGM
-> ITitleSceneNavigator.ShowTitle()
-> TitleSceneNavigator resolves the scoped TitleView at display time
-> ISceneViewNavigator attaches TitleView through the ViewManager host path
```

Current unload path:

```text
TitleScene.OnBeforeUnload
-> ISceneTransitionPlayer.FadeOut()
-> ISceneTransitionPlayer is resolved from RootLifetimeScope
-> legacy transition player uses ViewManager overlay and ViewTransitionManager internally

TitleScene.OnUnloaded
-> no direct cleanup work

TitleSceneScope disposed by Unity scene unload
-> TitleSceneLocalizationOwner releases preloaded tables
-> TitleSceneBgmOwner stops BGM and releases Addressables handle
```

Current controller path:

```text
TitleView button
-> TitleViewController
-> ITitleSceneNavigator.ShowCharacterSelect or ShowSettings
-> ISceneViewNavigator attaches CharacterSelectView or SettingsView through the ViewManager host path
```

Current new-adventure path:

```text
CharacterSelectController.StartNewAdventure
-> AdventureService.StartNew
-> CardService/CardBoardService clear
-> PlayerService initialize
-> CardDeckService initialize
-> ISceneLoader loads AdventureScene
```

## Current Ownership Map

| Object | Current owner | Current lifetime | Target owner | Notes |
| --- | --- | --- | --- | --- |
| `TitleScene Unity scene` | Unity + `TitleSceneScope` | Unity scene lifetime | Unity + `TitleSceneScope` | Scene startup is owned by `TitleSceneEntryPoint`; pure C# `TitleScene` is no longer created by the legacy BaseScene runner. |
| `TitleViewController` | `TitleSceneScope` | Title scene | `TitleSceneScope` | Should be `Scoped`. It should die when TitleScene ends. |
| `CharacterSelectController` | `TitleSceneScope` | Title scene | `TitleSceneScope` | Should be `Scoped`, but its adventure session starter remains a legacy bridge. |
| `ITitleSceneNavigator` / `TitleSceneNavigator` | `TitleSceneScope` | Title scene | `TitleSceneScope` | Single TitleScene navigation boundary. It resolves scoped views at display time to avoid constructor cycles and delegates display to `ISceneViewNavigator`. |
| `TitleView` | `TitleSceneScope` | Title scene | Title scene scope | Scoped pure C# view object. It binds Unity `VisualElement` objects and is attached by SceneViewNavigator. |
| `CharacterSelectView` | `TitleSceneScope` | Title scene | Title scene scope | Scoped pure C# view object. It is attached/detached by SceneViewNavigator and disposed by TitleSceneScope. |
| `SettingsView` | `TitleSceneScope` | Title scene | Title scene scope | Scoped pure C# view object. It is attached/detached by SceneViewNavigator and disposed by TitleSceneScope. Internal manager access remains deferred cleanup. |
| Title UXML assets | `ViewManager` | Until view removal | View asset loader / view factory | Current `ViewManager` loads by view type name. Release policy should be explicit later. |
| `TitleSceneBgmOwner` | `TitleSceneScope` | Title scene | `TitleSceneScope` | Keeps Addressables handle and releases it on scope disposal. |
| `TitleSceneLocalizationOwner` | `TitleSceneScope` | Title scene | `TitleSceneScope` | Uses one table list for preload/release and releases on scope disposal. |
| `AudioManager` | manager singleton | App lifetime | Root | TitleScene should request play/stop, not own the manager. |
| `ViewManager` | manager singleton | App lifetime | Root or adapter boundary | It owns UI layers and stack operations. |
| `ViewTransitionManager` | manager singleton | App lifetime | Root | Transition execution can remain app-level. |

## Target TitleSceneScope Boundary

`TitleSceneScope` should own objects whose logical lifetime is exactly:

```text
TitleScene loaded -> TitleScene unloaded
```

Recommended future registrations:

```csharp
builder.Register<TitleSceneLocalizationOwner>(Lifetime.Scoped);
builder.Register<TitleSceneBgmOwner>(Lifetime.Scoped);
builder.Register<ITitleSceneNavigator, TitleSceneNavigator>(Lifetime.Scoped);
builder.Register<TitleViewController>(Lifetime.Scoped);
builder.Register<CharacterSelectController>(Lifetime.Scoped);
```

`TitleSceneLocalizationOwner` and `TitleSceneBgmOwner` are now `TitleSceneScope`-owned. The remaining risk is not their creation path; it is whether scope disposal runs at the expected Unity scene unload point.

Expected disposal behavior:

```text
TitleSceneScope disposed
-> TitleSceneBgmOwner.Dispose stops/releases BGM resources
-> TitleSceneLocalizationOwner.Dispose releases preloaded tables
-> TitleSceneNavigator released with the scope
-> TitleViewController.Dispose if needed
-> CharacterSelectController.Dispose if needed
-> TitleView / CharacterSelectView / SettingsView disposed by the scope
```

This matches VContainer's documented `Scoped` behavior: one instance per `LifetimeScope`, with `IDisposable` objects disposed when the scope is destroyed.

## What TitleSceneScope Must Not Own

`TitleSceneScope` should not own adventure run state.

These types currently appear in `CharacterSelectController`, but their final owner should be `AdventureSessionScope`:

```text
AdventureService
CardDeckService
CardService
CardBoardService
PlayerService
CombatService
```

Reason:

```text
TitleScene is menu lifetime.
AdventureSession is run lifetime.
CombatScene is battle scene lifetime.
```

If TitleScene owns these services, a run can accidentally inherit stale state from title menu logic, or TitleScene unload can dispose objects that the adventure still needs.

## CharacterSelectController Boundary Problem

Current code:

```text
CharacterSelectController
-> directly initializes adventure/session services
-> loads AdventureScene
```

This mixes two responsibilities:

```text
1. Title UI decision: which character did the player select?
2. Adventure session creation: create run state, deck, player, board, and combat state.
```

VContainer-oriented target:

```text
CharacterSelectController
-> validates selected character
-> asks IAdventureSessionFactory to create a session
-> asks scene navigation to load AdventureScene
```

The factory, not `TitleSceneScope`, owns session creation:

```text
TitleSceneScope
-> CharacterSelectController
-> IAdventureSessionFactory
-> creates AdventureSessionScope
-> AdventureScene consumes AdventureSessionScope
```

This prevents TitleScene from resolving session-scoped services before an adventure session exists.

First-phase decision:

```text
CharacterSelectController is TitleSceneScope-owned.
CharacterSelectController reads character catalog data through ICharacterSelectCatalog, not CharacterService directly.
CharacterSelectController reads unlock state through ICharacterUnlockGateway, not SaveManager directly.
AdventureService/CardDeckService/CardService/CardBoardService/PlayerService/CombatService final ownership is not decided in the TitleScene pass.
Scene loading goes through ISceneLoader and UnitySceneLoader.
Adventure run-state creation should later move behind IAdventureSessionFactory or IAdventureSessionStarter.
LegacyAdventureSessionStarter is resolved from TitleSceneScope as IAdventureSessionStarter.
LegacyAdventureSessionStarter is an interim bridge and must not become the final TitleSceneScope-owned session creator.
```

Current unlock-state bridge:

```text
CharacterSelectController
-> ICharacterSelectCatalog
-> LegacyCharacterSelectCatalog
-> DependencyManager / CharacterService / DBManager

CharacterSelectController
-> ICharacterUnlockGateway
-> LegacyCharacterUnlockGateway
-> SaveManager / ProgressState
```

These bridges are acceptable only while character catalog and save/progress ownership remain legacy-owned.

Do not reintroduce a direct-created `CharacterSelectController` path while `TitleSceneScope` owns the controller.

Current TitleScene boundary check:

```text
TitleSceneScope has no direct DependencyManager.Resolve calls.
TitleScene View/Controller types have no direct SaveManager, AudioManager, GraphicManager, LocaleManager, ViewManager, or SceneManagerEx calls.
Legacy access remains in LegacySettingsGateway, LegacyCharacterUnlockGateway, LegacyCharacterSelectCatalog, and LegacyAdventureSessionStarter.
```

## View Creation Policy

`IViewFactory` only centralizes creation. It does not decide whether a view is cached or recreated.

Final ownership decision:

```text
Final view owner = SceneScope
Final ViewManager role = Root UI Host
```

This means the long-term target is:

```text
TitleSceneScope owns TitleScene views and controllers.
ViewManager owns UIDocument, root layers, overlay layers, and attach/detach mechanics.
ViewManager should not be the final owner that decides scene view disposal.
```

Current `ViewManager.Push/Pop/Clear` is incompatible with that final model because `Pop` and `Clear` dispose views. TitleScene therefore uses the separate `Attach/Detach` host path through `ISceneViewNavigator`.

ViewManager redesign details are tracked in `docs/view-manager-redesign-plan.md`.

Previous transient behavior:

```text
TitleViewController.OnNewGame
-> historical TitleSceneViewFlow.ShowCharacterSelect()
-> IViewFactory.Create<CharacterSelectView>()
-> IViewNavigator.Push(view)

CharacterSelectView back
-> ViewManager.Pop()
-> Dispose view
-> remove root from hierarchy
```

Therefore, `CharacterSelectView` is recreated every time the player opens it.

Current TitleScene decision:

```text
Use scoped views for TitleScene.
Open -> resolve existing scoped view -> attach/show.
Close -> detach/hide through SceneViewNavigator.
TitleScene unload -> TitleSceneScope disposes the scoped views.
```

Reason:

```text
The non-disposing ViewManager host path now exists.
TitleScene views no longer directly call ViewManager.Pop.
SceneViewNavigator is connected only to TitleScene-scoped views, so ViewManager is not the disposal owner.
```

Applied scoped policy:

```text
TitleSceneScope
-> owns TitleScene view instances

TitleSceneNavigator
-> resolves scoped TitleScene views from the current TitleSceneScope
-> decides which scoped view to show/hide

ViewManager/ViewHost
-> attaches/detaches view roots
-> does not dispose scoped views
```

Scoped view preconditions:

```text
1. ViewManager exposes Hide/Show without Dispose.
2. Pop and Hide have explicit separate meanings.
3. CharacterSelectView does not call ViewManager.Pop directly for cached-close behavior.
4. SettingsView does not call ViewManager.Pop directly for cached-close behavior.
5. TitleSceneScope owns cached/scoped view instances.
6. TitleSceneNavigator resolves those views but does not become their disposal owner.
7. TitleSceneScope disposal disposes scoped views exactly once.
```

Do not apply this policy silently to other scenes. AdventureScene still uses the legacy transient path.

`TitleSceneNavigator` exists to keep TitleScene screen decisions out of views.

Its responsibility:

```text
TitleSceneEntryPoint
-> asks for "show title"

TitleViewController
-> asks ITitleSceneNavigator for "show character select" or "show settings"

TitleSceneNavigator
-> resolves the requested TitleScene scoped view
-> delegates display to ISceneViewNavigator
```

It should not own audio, localization, adventure session creation, or root UI layers.

Current TitleScene shape:

```csharp
public sealed class TitleSceneNavigator : ITitleSceneNavigator
{
    private readonly ISceneViewNavigator _viewNavigator;
    private readonly IObjectResolver _resolver;

    public TitleSceneNavigator(
        ISceneViewNavigator viewNavigator,
        IObjectResolver resolver)
    {
        _viewNavigator = viewNavigator;
        _resolver = resolver;
    }

    public void ShowTitle()
    {
        _viewNavigator.Show(_resolver.Resolve<TitleView>());
    }

    public void ShowCharacterSelect()
    {
        _viewNavigator.Show(_resolver.Resolve<CharacterSelectView>());
    }

    public void ShowSettings()
    {
        _viewNavigator.Show(_resolver.Resolve<SettingsView>());
    }
}
```

Applied direction after ViewManager host split:

```text
View owner = SceneScope
ViewManager = Root UI Host
TitleSceneNavigator = TitleScene screen navigation boundary
```

Current decision:

```text
TitleSceneNavigator may use IObjectResolver internally.
Resolve targets are limited to TitleSceneScope-owned views.
Views and controllers must not use IObjectResolver directly.
TitleSceneNavigator must not resolve domain services or legacy managers.
```

Final VContainer registration direction:

```csharp
builder.Register<TitleView>(Lifetime.Scoped);
builder.Register<CharacterSelectView>(Lifetime.Scoped);
builder.Register<SettingsView>(Lifetime.Scoped);
builder.Register<ISceneViewNavigator, SceneViewNavigator>(Lifetime.Scoped);
builder.Register<ITitleSceneNavigator, TitleSceneNavigator>(Lifetime.Scoped);
```

Do not let `TitleSceneNavigator` become a second `ViewManager` or a general service locator.
It should contain TitleScene-specific navigation decisions only.

There is intentionally no `TitleSceneFlow` orchestration class in the current design. `TitleScene` already owns `OnLoaded`, `OnBeforeUnload`, and `OnUnloaded`; adding another flow object would be an extra indirection before there is evidence it removes real complexity.

## Unity And VContainer Boundary

TitleScene now uses `TitleSceneScope` as the scene-owned MonoBehaviour composition root. The legacy pure C# `TitleScene` class is no longer created or mirrored by the BaseScene runner.

Adopted target strategy:

```text
Unity scene contains TitleSceneScope MonoBehaviour
-> VContainer builds the Title scene scope when the Unity scene loads
-> TitleSceneScope owns pure C# controllers and disposable resource owners
-> TitleSceneEntryPoint starts the Title scene flow
-> ViewManager continues to own UIDocument/layers/VisualElement attachment until UI hosting is redesigned
```

Temporary bridge:

```text
AdventureScene still uses the legacy BaseScene bridge.
TitleScene does not.
```

MonoBehaviour registration rule:

```text
Use RegisterComponentInHierarchy only for real scene components that already exist in the Unity scene.
Do not convert pure C# controllers into MonoBehaviours just to receive lifecycle callbacks.
```

Cold assessment:

```text
The adopted direction does not mean every object becomes a MonoBehaviour.
It means the scene composition root becomes a MonoBehaviour because Unity owns scene loading and scene destruction.
```

## Resource Ownership Risks Found

### Addressables BGM

Original risk:

```text
TitleScene loaded TitleMenuBgm and stored only the resulting AudioClip.
Addressables handle ownership was not explicit.
Stopping AudioManager did not necessarily release the loaded Addressables asset.
```

Current mitigation:

```text
TitleSceneEntryPoint uses TitleSceneBgmOwner through TitleSceneScope.
TitleSceneBgmOwner keeps AsyncOperationHandle<AudioClip>.
Dispose stops BGM and releases the handle.
```

Remaining limitation:

```text
TitleSceneBgmOwner is registered in TitleSceneScope.
The Unity TitleScene contains @TitleSceneScope for this ownership to run.
Its IAudioPlayer dependency is inherited from RootLifetimeScope at the scene composition boundary.
```

Confirmed first-phase design:

```text
TitleSceneBgmOwner is TitleScene-specific.
Do not generalize it into SceneBgmOwner yet.
If CombatScene later needs BGM, create CombatSceneBgmOwner first and only extract common logic after policy duplication is proven.
```

`TitleSceneBgmOwner` owns:

```text
TitleMenuBgm Addressables handle
Load/play request
Stop/release on Dispose
```

It does not own:

```text
AudioManager creation
AudioSource creation
Volume settings
Global BGM policy
Other scene music policy
```

Dispose order:

```text
1. Stop BGM through IAudioPlayer.
2. Release Addressables handle.
```

Do not release the handle before stopping audio, because the legacy audio manager may still reference the clip.

### Localization Preload

Original preload included:

```text
TitleView
CharacterSelectView
SettingsView
CharacterNames
```

Original unload released only:

```text
TitleView
CharacterSelectView
SettingsView
```

Risk:

```text
CharacterNames is preloaded but not released by the same owner.
```

Current mitigation:

```text
TitleScene now uses TitleSceneLocalizationOwner directly.
TitleSceneLocalizationOwner stores one table list.
Preload and Dispose use the same table list.
```

Confirmed first-phase design:

```csharp
public sealed class TitleSceneLocalizationOwner : IDisposable
{
    private const string CharacterNamesTable = "CharacterNames";

    private static readonly TableReference[] Tables =
    {
        nameof(TitleView),
        nameof(CharacterSelectView),
        nameof(SettingsView),
        CharacterNamesTable,
    };

    private bool _isPreloaded;

    public void Preload()
    {
        if (_isPreloaded)
            return;

        LocalizationSettings.StringDatabase
            .PreloadTables(Tables)
            .WaitForCompletion();

        _isPreloaded = true;
    }

    public void Dispose()
    {
        if (!_isPreloaded)
            return;

        for (int i = 0; i < Tables.Length; i++)
        {
            LocalizationSettings.StringDatabase.ReleaseTable(Tables[i]);
        }

        _isPreloaded = false;
    }
}
```

Risks to keep visible:

```text
WaitForCompletion can stall scene entry.
CharacterNames may become shared with another scene later.
If another owner also preloads/releases the same table, ownership becomes unclear.
```

## Recommended Migration Steps For TitleScene

Detailed implementation sequencing is tracked in `docs/title-scene-vcontainer-implementation-phases.md`.

### Step 1: Keep Runtime As-Is, Freeze The Boundary

Do not register TitleScene runtime types in VContainer yet.

Record the intended ownership:

```text
TitleViewController -> TitleSceneScope
CharacterSelectController -> TitleSceneScope
Adventure run services -> AdventureSessionScope
Managers -> Root or legacy manager singletons
```

### Step 2: Extract Disposable Resource Owners

Create pure C# owners without connecting them to VContainer first:

```text
TitleSceneLocalizationOwner
TitleSceneBgmOwner
```

This makes resource lifetime testable before DI changes.

### Step 3: Add Narrow Root Adapters

Add only the adapter boundaries required by TitleScene:

```text
IAudioPlayer -> AudioManagerAdapter
IViewHost -> ViewManager
ISceneLoader -> UnitySceneLoader
ISceneTransitionPlayer -> ViewOverlaySceneTransitionPlayer
IViewFactory -> existing LegacyViewFactory or future VContainerViewFactory
```

Do not move existing managers into VContainer in this step.

### Step 4: Introduce TitleSceneNavigator

Historical first phase used transient view policy:

```text
TitleViewController
-> historical TitleSceneViewFlow
-> IViewFactory.Create
-> IViewNavigator.Push
```

Current TitleScene uses scoped view policy:

```text
TitleSceneNavigator
-> scoped TitleScene views
-> ISceneViewNavigator.Show / HideCurrent
-> ViewManager Attach / Detach host path
```

### Step 5: Introduce TitleSceneScope In Parallel

Done for TitleScene.

TitleSceneScope is now connected through the Unity scene and owns TitleScene startup, controllers, and views.

Remaining DependencyManager bridge:

```text
LegacyAdventureSessionStarter is no longer registered in DependencyManager.
TitleSceneScope registers LegacyAdventureSessionStarter directly as IAdventureSessionStarter.
LegacyAdventureSessionStarter still resolves adventure/session services from DependencyManager internally.
Those services must move later to AdventureSessionScope, not TitleSceneScope.
```

The rule remains:

```text
A type is owned by either DependencyManager or VContainer, never both at runtime.
```

### Step 6: Move Controllers To Constructor Injection

Move `TitleViewController` and `CharacterSelectController` from field injection to constructor injection when they become VContainer-owned.

Do not move adventure run services directly into `CharacterSelectController` as scoped dependencies. Introduce an `IAdventureSessionFactory` boundary first.

### Step 7: Revisit Cached View Policy

The original first-phase policy was transient:

```text
Transient stack view: simple, recreated on every open.
```

The current TitleScene policy is scoped/cached per scene:

```text
TitleScene scoped view: created once per TitleSceneScope, hidden/detached on close, disposed when TitleSceneScope ends.
```

This was changed deliberately after the non-disposing host path existed.
Do not apply the same policy to AdventureScene until its close/dispose flow is mapped.

## First Implementation Status

The first resource-ownership implementation has been applied without connecting VContainer runtime ownership.

Applied:

```text
1. Add TitleSceneLocalizationOwner.
2. Add TitleSceneBgmOwner.
3. Use them inside TitleScene directly. Done historically; current ownership is TitleSceneScope.
4. Verify Unity script compilation.
```

This improves memory and lifetime correctness immediately while preserving current game flow.

Current result:

```text
Those owners have moved from direct TitleScene fields into TitleSceneScope ownership.
```

## References

- VContainer LifetimeScope and scoped lifetime: https://vcontainer.hadashikick.jp/scoping/lifetime-overview
- VContainer entry points and registration examples: https://vcontainer.hadashikick.jp/integrations/entrypoint
- VContainer MonoBehaviour registration: https://vcontainer.hadashikick.jp/registering/register-monobehaviour
