# ViewManager Redesign Plan

## Current Status Note

Read this document together with `docs/vcontainer-adoption-doc-index.md`.

This document is the current authority for View ownership and ViewManager redesign decisions.

## Purpose

This document records the ViewManager redesign direction required before moving view ownership into VContainer scene scopes.

This is not a VContainer registration problem. It is a view ownership problem.

Confirmed long-term direction:

```text
Final view owner = SceneScope
Final ViewManager role = Root UI Host
```

Cold assessment:

```text
The current ViewManager is not only a host.
It also loads view assets, binds views, keeps the stack, and disposes views.
Connecting SceneScope-owned views before separating these responsibilities can create double-dispose, stale references, and hidden root-to-scene ownership.
```

## Current ViewManager Responsibilities

Current `ViewManager` owns or performs:

```text
Root UI lifetime
- creates @ViewManager GameObject
- owns UIDocument
- owns PanelSettings
- owns root-layer, view-layer, overlay-layer
- listens to sceneUnloaded
- listens to GraphicManager.ViewAspectChanged

View asset work
- loads VisualTreeAsset from Addressables by view type name
- clones UXML into a logical root

View lifecycle work
- calls BaseView.Bind
- stores views in a stack
- Push adds a view
- Pop disposes the top view and removes it from hierarchy
- Clear disposes all views and clears layers
- Attach adds a view to the host without taking disposal ownership
- Detach removes a hosted view without disposing it

View factory work
- receives IViewFactory through constructor injection
```

This means current `ViewManager` is:

```text
View host
+ View asset loader
+ View binder
+ View navigator
+ View lifetime owner
+ Legacy view factory holder
```

## Current View Lifecycle

Current transient lifecycle:

```text
IViewFactory.Create<TView>
-> creates pure C# view
-> injects dependencies through DependencyManager

ViewManager.Push(view)
-> loads VisualTreeAsset
-> creates container/logicalRoot
-> CloneTree
-> BaseView.Bind
-> stores view in stack

ViewManager.Pop()
-> removes view from stack
-> view.Dispose()
-> view.Root.RemoveFromHierarchy()

ViewManager.Clear()
-> disposes all stacked views
-> detaches non-owned attached views
-> clears view layer
```

This lifecycle is internally coherent for transient views, but it conflicts with the future `SceneScope owns views` direction.

## Problem Statement

### Root Object Owns Scene Views

`ViewManager` has root/application lifetime, but it currently owns scene-specific views.

Current ownership checkpoint:

```text
ViewManager is no longer ManagerRegistry-owned.
ViewManager is VContainer-created as a root singleton.
GameRootEntryPoint calls ViewManager.Initialize once.
ViewManager.Dispose is handled by VContainer when RootLifetimeScope is disposed.
```

Risk:

```text
Scene ends, but root lifetime ViewManager still references scene view.
Root clear order can conflict with SceneScope disposal.
```

### Navigation And Ownership Are Mixed

Current `Pop` means:

```text
remove from navigation stack
+ dispose view
+ remove root from hierarchy
```

This prevents cached views and SceneScope-owned views from using the same API safely.

### Clear Is Scene-Unload Cleanup And Ownership Cleanup

`ViewManager.OnSceneUnloaded` calls `Clear`.

That is safe only if `ViewManager` is the owner of all views. It is unsafe if `SceneScope` becomes the owner.

### View Asset Handle Ownership Is Unclear

`Push` loads `VisualTreeAsset` through Addressables but does not keep a handle.

Risk:

```text
UXML asset release policy is not explicit.
Changing view lifetime policy without fixing asset ownership can hide leaks.
```

### Close Flow Was Directly Coupled To ViewManager

Some views close themselves through `ViewManager.Instance.Pop()`, and some have already been partially extracted.

Examples:

```text
SettingsView.OnClose -> close callback supplied by creation path
CharacterSelectView back/close flow -> ViewManager stack pop behavior
```

Direct view-owned close behavior makes it hard to change close policy from dispose-based transient views to hide-based scoped views.

## Target Responsibilities

### ViewManager

Final role:

```text
Root UI Host
```

Allowed responsibilities:

```text
- own UIDocument
- own root/view/overlay layers
- expose attach/detach points
- apply viewport/aspect changes
- expose overlay layer for transitions
- receive small root dependencies through constructor injection
```

Responsibilities to remove or delegate:

```text
- deciding scene view ownership
- disposing scene-owned views
- deciding transient vs cached policy
- direct view factory ownership
- scene-specific screen flow
```

Current concrete injection audit:

| Consumer | Current reason | Later direction |
| --- | --- | --- |
| `GameRootEntryPoint` | Calls `ViewManager.Initialize()` as root startup ordering. | Keep until there are multiple root initializables that justify a broader root orchestration interface. |
| `ViewOverlaySceneTransitionPlayer` | Reads `OverlayLayer` for scene fade. | Replace concrete dependency with an overlay host port. |
| `ViewManagerBehavior` | Internal MonoBehaviour forwards viewport size changes to its owner. | Accept as internal helper, not a public dependency boundary. |

### SceneScope

Final role:

```text
View and controller owner for the scene.
```

For TitleScene:

```text
TitleSceneScope
-> TitleView
-> CharacterSelectView
-> SettingsView
-> TitleViewController
-> CharacterSelectController
-> TitleSceneNavigator
```

### ViewNavigator

Final role:

```text
Navigation policy.
```

It decides stack, modal, show, and hide behavior. It should not own root UI lifetime.

Important rule:

```text
Navigator should not dispose SceneScope-owned views.
```

### Scene Navigator

Final role:

```text
Scene-specific screen flow.
```

For TitleScene:

```text
TitleSceneNavigator
-> ShowTitle
-> ShowCharacterSelect
-> ShowSettings
-> HideCurrent
```

It should not become a second `ViewManager`.

### ViewBinder / ViewHost

Possible future extraction:

```text
ViewBinder
-> load VisualTreeAsset
-> create container/logicalRoot
-> CloneTree
-> call BaseView.Bind
-> optionally track Addressables handle

ViewHost
-> attach/detach already-bound visual roots to ViewManager layers
```

Do not introduce these abstractions before the current lifecycle has been fully mapped.

## API Direction

Current API:

```csharp
public void Push(BaseView view);
public void Pop();
public void Clear();
```

Current explicit ownership API:

```csharp
public void PushAndOwn(BaseView view);
public void PopAndDispose();
public void ClearOwnedViewsAndDetachAll();
```

`Push`, `Pop`, and `Clear` remain compatibility wrappers. New migration code should prefer the explicit names when it means legacy stack ownership.

Current scene-owned host split:

```csharp
public interface IViewHost
{
    void Attach(BaseView view);
    void Detach(BaseView view);
    void DetachAll();
}
```

Legacy root `IViewNavigator` / `ViewManagerAdapter` has been removed. Do not reintroduce a generic root navigator for the legacy owning stack unless an unmigrated runtime path explicitly needs it.

Possible future scene navigation policy:

```csharp
public interface ISceneViewNavigator
{
    void Show(BaseView view);
    void Hide(BaseView view);
}
```

Naming should make ownership explicit.

Bad:

```text
Pop
```

Better during migration:

```text
PopAndDispose
DetachTop
HideWithoutDispose
```

Cold assessment:

```text
Keeping the name Pop while changing whether it disposes is dangerous.
The method name should expose ownership semantics.
```

## Migration Plan

### Phase 0: Preserve Current Runtime

Do not change runtime behavior while VContainer scope skeletons are being added.

Current behavior remains:

```text
ViewManager owns transient view stack.
Pop disposes.
Clear disposes.
```

### Phase 1: Document Current Close Paths

Find every direct close/navigation call:

```text
Legacy direct calls to ViewManager.Push
Legacy direct calls to ViewManager.Pop
Legacy direct calls to ViewManager.Clear
Legacy direct calls to ViewManager.Peek
```

Classify each call:

```text
Navigation intent
Ownership cleanup
Scene unload cleanup
Modal close
Back action
```

Do not change APIs before this map exists.

Current call-site map:

| File | Call | Classification | Current meaning | Future direction |
| --- | --- | --- | --- | --- |
| `Assets/@Scripts/Domains/Scene/Title/TitleSceneEntryPoint.cs` | `ITitleSceneNavigator.ShowTitle()` | Screen-flow request | Entry point asks the TitleScene navigator to show the root TitleView. | Keep as TitleSceneScope-owned navigation boundary. |
| `Assets/@Scripts/Domains/View/TitleView/TitleViewController.cs` | `ITitleSceneNavigator.ShowCharacterSelect()` | Screen-flow request | Controller asks the TitleScene navigator to show CharacterSelect. | Keep controller free of view creation and ViewManager access. |
| `Assets/@Scripts/Domains/View/TitleView/TitleViewController.cs` | `ITitleSceneNavigator.ShowSettings()` | Screen-flow request | Controller asks the TitleScene navigator to show Settings. | Keep controller free of view creation and ViewManager access. |
| `Assets/@Scripts/Domains/Settings/View/SettingsViewController.cs` | `ITitleSceneNavigator.HideCurrent()` | Close request | Controller saves settings, then asks the TitleScene navigator to hide the current view. | Keep close navigation out of the view. |
| `Assets/@Scripts/Domains/Settings/View/SettingsView.cs` | `IViewHost.RootLayer` | View expression | SettingsView applies root layer classes through the host port because the visual class is part of view expression. | Extract a narrower display/root-layer boundary later only if this grows beyond view expression. |
| `Assets/@Scripts/Domains/View/CharacterSelect/CharacterSelectController.cs` | `ITitleSceneNavigator.HideCurrent()` | Back close request | Back transition completion reaches the controller, which asks the navigator to hide the current view. | Keep close navigation out of the view. |
| `Assets/@Scripts/Domains/View/CharacterSelect/CharacterSelectController.cs` | `ISceneLoader.Load(GameSceneId.Adventure)` through `LegacyAdventureSessionStarter` | Scene navigation from controller | Controller initializes adventure state through a bridge and requests scene loading through a narrow port. | Replace the legacy bridge with an AdventureSessionScope starter when run-state ownership moves. |
| `Assets/@Scripts/Domains/Scene/Adventure/AdventureSceneStartup.cs` | `IViewHost.Attach/Detach` | Scene-owned view hosting | AdventureSceneStartup attaches AdventureView through the root host without transferring disposal ownership to ViewManager. | Continue auditing Adventure scene lifetime through AdventureSceneScope and BaseSceneLifecycleRunner. |
| `Assets/@Scripts/Core/Manager/View/ViewManagerBehavior.cs` | `ViewManager.Initialize(this)` owner reference | Root UI host update | MonoBehaviour forwards screen size changes to the VContainer-created ViewManager instance. | Accept as internal helper owned by ViewManager. |

TitleScene-first priority:

```text
1. SettingsView Save/Audio/Graphic/Locale dependencies through ISettingsGateway / LegacySettingsGateway.
2. Remove remaining LegacyAdventureSessionStarter DependencyManager registry path when safe. Done.
3. Keep the legacy ViewManager transient path only as compatibility surface until every remaining caller is audited.
```

Cold assessment:

```text
The riskiest call sites are inside views and controllers, not inside ViewManager itself.
As long as views directly call Pop or LoadScene, SceneScope-owned view lifetime will be fragile.
```

### Phase 2: Add Explicit Host Methods

Add methods that do not dispose:

```text
Attach
Detach
DetachAll
```

Keep existing `Push/Pop/Clear` stack-owned disposal behavior unchanged at this phase.

Purpose:

```text
Allow new SceneScope-owned view flow to attach/detach without changing legacy behavior.
```

Current implementation status:

```text
Done:
- ViewManager.Attach(BaseView)
- ViewManager.Detach(BaseView)
- ViewManager.DetachAll()
- IViewHost
- ISceneViewNavigator
- SceneViewNavigator
- ViewManager implements IViewHost directly.
- TitleSceneNavigator uses ISceneViewNavigator.
- SceneViewNavigator uses BaseView.Show/Hide for scoped visibility lifecycle.
- TitleScene views are resolved from TitleSceneScope and attached/detached through the non-disposing host path.

Still not done:
- Split ViewManager concrete root bridge dependencies into narrower ports.
- Addressables UXML handle ownership split from ViewManager.
```

### Phase 2A: Make Legacy Ownership Names Explicit

Goal:

```text
Expose that the legacy stack path owns and disposes views.
```

Done:

```text
ViewManager.PushAndOwn(BaseView) exists.
ViewManager.PopAndDispose() exists.
ViewManager.ClearOwnedViewsAndDetachAll() exists.
Existing Push/Pop/Clear remain as compatibility wrappers.
Root IViewNavigator / ViewManagerAdapter has been removed because no runtime code used it.
```

Reason:

```text
This changes no runtime behavior, but it removes ambiguity at the legacy root navigation boundary.
The next migration step can compare:
- legacy owning navigation: PushAndOwn / PopAndDispose
- scene host navigation: Attach / Detach
```

Cold assessment:

```text
Changing Pop to stop disposing would be dangerous.
Adding explicit names first is the safer step because it lets both ownership models coexist without pretending they mean the same thing.
```

### Phase 3: Extract Navigation Policy

Introduce a navigator implementation that uses the host API.

Important:

```text
The new navigator must be clear about whether it disposes.
For SceneScope-owned views, it should not dispose.
```

Current implementation status:

```text
SceneViewNavigator exists.
It uses IViewHost.Attach/Detach.
It uses BaseView.Show/Hide for TitleScene-scoped visibility lifecycle.
BaseView.OnVisualTreeCloned remains the one-time post-CloneTree UXML binding hook.
BaseView.OnShown/OnHidden is the scene navigator visibility hook.
It detaches views on Hide/HideCurrent/HideAll.
It does not call Dispose on views.
It is registered in the connected TitleSceneScope.
TitleSceneNavigator uses it for TitleScene-scoped views.
```

Cold assessment:

```text
This navigator is now safe only for TitleScene-scoped views.
Do not reuse it for legacy transient views created through LegacyViewFactory unless another owner disposes those views.
```

### Phase 4: Move View Close Intent Out Of Views

Views should stop directly calling `ViewManager.Instance.Pop()`.

Preferred direction:

```text
View button click
-> View controller
-> ViewFlow
-> Navigator
```

For example:

```text
SettingsView close
-> SettingsViewController.OnClose
-> save settings
-> ITitleSceneNavigator.HideCurrent
```

Detailed examples:

### Example: SettingsView Close

Original view-owned flow:

```text
SettingsView.OnClose
-> SaveManager.Instance.SaveAll
-> close callback supplied by creation path
```

Problem:

```text
SettingsView no longer directly knows ViewManager.Pop, but it still knows saving policy.
This is acceptable for Phase 2A and should be removed in a later SettingsView controller pass.
```

Target flow:

```text
SettingsView
-> observes close button click
-> calls SettingsViewController.OnCloseRequested

SettingsViewController
-> saves settings
-> asks ITitleSceneNavigator to hide current view

TitleSceneNavigator
-> delegates HideCurrent to ISceneViewNavigator
```

Example shape:

```csharp
public sealed class SettingsView : BaseView
{
    private SettingsViewController _controller;

    public void Initialize(SettingsViewController controller)
    {
        _controller = controller;
    }

    private void OnCloseClicked()
    {
        _controller.OnCloseRequested();
    }
}
```

```csharp
public sealed class SettingsViewController
{
    private readonly ISaveStore _saveStore;
    private readonly ITitleSceneNavigator _navigator;

    public void OnCloseRequested()
    {
        _saveStore.SaveAll();
        _navigator.HideCurrent();
    }
}
```

Current implemented first phase:

```text
SettingsView
-> observes UI events
-> calls SettingsViewController methods
-> runs view-local root-layer class expression through IViewHost.RootLayer

SettingsViewController
-> depends on ISettingsGateway for settings operations
-> saves settings and asks ITitleSceneNavigator to hide the current view

LegacySettingsGateway
-> temporarily bridges to SaveManager, AudioManager, GraphicManager, LocaleManager
```

This is not the final clean design. It is a containment step: legacy manager access moved out of the view and controller, but still exists in a named legacy adapter. Future `CloseSettings` can move close navigation into a TitleScene flow method without changing the view.

SettingsView final goal:

```text
SettingsView is not the object that changes settings.
SettingsView is the UI object that displays settings and forwards user intent to a controller.
```

Allowed final responsibilities:

```text
- query VisualElements
- subscribe/unsubscribe UI events
- display a SettingsViewModel
- update visual classes and UI state
- run view-local animation/visual behavior
```

Direct dependencies to remove over time:

```text
SettingsView -> SaveManager
SettingsView -> AudioManager
SettingsView -> GraphicManager
SettingsView -> LocaleManager
```

Recommended SettingsView extraction order:

```text
1. Close/navigation intent
   -> remove direct ViewManager.Pop.

2. Save responsibility
   -> remove direct SaveManager.SaveAll.

3. Audio settings responsibility
   -> remove direct AudioManager calls.

4. Graphic settings responsibility
   -> remove direct GraphicManager calls.

5. Locale settings responsibility
   -> remove direct LocaleManager calls.

6. Root layer state responsibility
   -> keep in SettingsView while it remains pure visual expression.
   -> extract only if it becomes cross-view display policy.
```

Do not attempt all of these in the ViewManager ownership pass. The first blocker for SceneScope-owned views is direct close/navigation ownership.

### Example: CharacterSelect Back

Current flow:

```text
CharacterSelectView back click
-> close animation
-> transition end
-> back close callback supplied by creation path
```

Target flow:

```text
CharacterSelectView
-> observes back click
-> runs close animation
-> tells CharacterSelectController that back close animation finished

CharacterSelectController
-> asks ITitleSceneNavigator to hide current view

TitleSceneNavigator
-> delegates HideCurrent to ISceneViewNavigator
```

Example shape:

```csharp
private void OnClose(TransitionEndEvent evt)
{
    if (!_isClosing || evt.target != _screenRoot)
        return;

    CloseReason reason = _closeReason;
    _isClosing = false;
    _closeReason = CloseReason.None;

    if (reason == CloseReason.Back)
    {
        _controller.OnBackCloseAnimationFinished();
    }
}
```

```csharp
public void OnBackCloseAnimationFinished()
{
    _navigator.HideCurrent();
}
```

### Example: CharacterSelect Start

Current flow:

```text
CharacterSelectView start click
-> CharacterSelectController.StartNewAdventure
-> controller initializes adventure state through LegacyAdventureSessionStarter
-> controller calls ISceneLoader.Load(GameSceneId.Adventure)
```

Problem:

```text
The view used to contain a scene-load branch, but current runtime loading is in CharacterSelectController.
The remaining problem is that CharacterSelectController still owns both adventure-state preparation and scene navigation timing.
It no longer directly accesses SceneManagerEx; scene loading now goes through ISceneLoader and UnitySceneLoader.
```

Target flow:

```text
CharacterSelectView
-> observes start click
-> asks controller whether adventure can start
-> runs close animation if accepted
-> tells controller that start close animation finished

CharacterSelectController
-> validates character
-> prepares adventure state using current legacy bridge for now
-> later asks IAdventureSessionFactory / IAdventureSessionStarter
-> asks ISceneLoader to load GameSceneId.Adventure after close animation
```

Example shape:

```csharp
public bool RequestStartAdventure(ECharacter characterId)
{
    if (!IsUnlocked(characterId))
        return false;

    StartNewAdventureState(characterId);
    return true;
}

public void OnStartCloseAnimationFinished()
{
    _sceneLoader.Load(GameSceneId.Adventure);
}
```

Do not load the next scene before close animation completion unless the UX policy is explicitly changed.

Current applied state:

```text
CharacterSelectView no longer calls SceneManagerEx.LoadScene directly.
CharacterSelectController no longer calls SceneManagerEx.LoadScene directly.
CharacterSelectController uses ISceneLoader through the TitleSceneScope LegacyAdventureSessionStarter bridge.
Current runtime does not wait for CharacterSelect close animation before calling ISceneLoader.Load(GameSceneId.Adventure).
TitleScene-scoped CharacterSelectView is now shown through ISceneViewNavigator and is not disposed by ViewManager.
```

Recommended extraction order:

```text
Done:
- SettingsView close direct Pop removal.
- CharacterSelectView Back direct Pop removal.
- CharacterSelectView Start direct SceneManagerEx removal.
- CharacterSelectController direct SceneManagerEx removal.

Done:
1. TitleSceneNavigator uses ISceneViewNavigator for scoped TitleScene views.
2. TitleSceneNavigator is TitleSceneScope-owned.
3. Temporary historical TitleSceneViewFlow / TitleSceneChildNavigator split has been removed from TitleScene.

Next:
1. Map AdventureScene view ownership before applying SceneViewNavigator there.
2. Move adventure run-state services behind an AdventureSessionScope boundary.
```

### Phase 5: Register Scene Views As Scoped

Only after host/navigator ownership is separated:

```csharp
builder.Register<TitleView>(Lifetime.Scoped);
builder.Register<CharacterSelectView>(Lifetime.Scoped);
builder.Register<SettingsView>(Lifetime.Scoped);
```

Current status:

```text
Done for TitleScene.
These registrations exist inside the connected TitleSceneScope.
TitleScene views receive controllers through VContainer constructor injection.
TitleSceneNavigator resolves scoped views through the TitleSceneScope resolver and delegates attach/detach to ISceneViewNavigator.
SceneViewNavigator uses BaseView.Show/Hide for TitleScene-scoped visibility lifecycle.
ViewManager does not dispose these TitleScene-scoped views.
```

### Phase 6: Remove Legacy View Ownership

After scoped view ownership is stable:

```text
ViewManager no longer owns scene view disposal.
SceneScope disposal owns scene view disposal.
Legacy Push/Pop/Clear can be renamed, deprecated, or removed.
```

## TitleScene Impact

Current TitleScene skeleton:

```text
TitleSceneNavigator resolves TitleScene scoped views and delegates display to ISceneViewNavigator.
ViewManager attaches/detaches visual roots but does not own disposal.
```

Future TitleScene target:

```text
TitleSceneScope creates and owns TitleView, CharacterSelectView, SettingsView.
TitleSceneNavigator remains the TitleScene navigation boundary.
ViewManager attaches/detaches visual roots but does not own disposal.
```

## Non-Goals For First Pass

Do not do these in the first pass:

```text
- Rewrite all UI navigation.
- Introduce cached views before detach/hide semantics exist.
- Move every view to constructor injection.
- Remove ViewManager stack in one change.
- Replace Addressables loading policy globally.
- Move ViewManager itself into VContainer.
```

## Acceptance Criteria For Redesign Readiness

Before connecting SceneScope-owned views to runtime:

```text
1. All ViewManager direct call sites are mapped.
2. Pop/Detach/Hide/Dispose semantics are explicitly separated.
3. SceneScope-owned views are not disposed by ViewManager.
4. Scene unload cleanup order is defined.
5. UXML Addressables ownership policy is defined.
6. TitleScene close/back/settings flows no longer directly depend on ViewManager.Pop.
```

If any of these are not true, connecting VContainer-owned views is premature.
