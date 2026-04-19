using System.Collections.Generic;
using Domains.Settings;
using Game.Core.Managers.Garphic;
using Game.Core.Managers.Save;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Game.Core.Managers.View
{
    public class ViewManager : BaseManager<ViewManager>
    {
        public readonly struct StageLayoutState
        {
            public Vector2Int LogicalResolution { get; }
            public StageScaleMode ScaleMode { get; }
            public AspectBucket AspectBucket { get; }
            public float ActualAspectRatio { get; }
            public Rect ViewportRect { get; }
            public Vector2Int RootSize { get; }

            public StageLayoutState(
                Vector2Int logicalResolution,
                StageScaleMode scaleMode,
                AspectBucket aspectBucket,
                float actualAspectRatio,
                Rect viewportRect,
                Vector2Int rootSize)
            {
                LogicalResolution = logicalResolution;
                ScaleMode = scaleMode;
                AspectBucket = aspectBucket;
                ActualAspectRatio = actualAspectRatio;
                ViewportRect = viewportRect;
                RootSize = rootSize;
            }
        }

        public enum StageScaleMode
        {
            Keep,
            KeepHeight,
            Expand,
            KeepWidth
        }

        public enum AspectBucket
        {
            Narrow,
            Standard,
            UltraWide
        }

        private readonly struct StageLayout
        {
            public readonly Vector2Int LogicalSize;
            public readonly StageScaleMode ScaleMode;
            public readonly AspectBucket AspectBucket;

            public StageLayout(Vector2Int logicalSize, StageScaleMode scaleMode, AspectBucket aspectBucket)
            {
                LogicalSize = logicalSize;
                ScaleMode = scaleMode;
                AspectBucket = aspectBucket;
            }
        }

        private const float MaxNarrowRatio = 1.3333334f;
        private const float MaxWideRatio = 2.3888888f;
        private static readonly Vector2Int Ratio4x3StageSize = new(1680, 1260);
        private static readonly Vector2Int Ratio16x10StageSize = new(1920, 1200);
        private static readonly Vector2Int Ratio16x9StageSize = new(1920, 1080);
        private static readonly Vector2Int Ratio21x9StageSize = new(2580, 1080);
        private static readonly Vector2Int AutoNarrowStageSize = new(1680, 1260);
        private static readonly Vector2Int AutoStandardStageSize = new(1680, 1080);
        private static readonly Vector2Int AutoUltraWideStageSize = new(2580, 1080);

        private UIDocument _document;
        private PanelSettings _panelSettings;
        private GraphicSaveData _graphicSaveData;
        private EDisplayAspectPreset _aspectPreset;
        private GameObject _managerObject;
        private VisualElement _sharedRoot;
        private VisualElement _letterbox;
        private VisualElement _viewport;
        private VisualElement _stage;
        private VisualElement _viewLayer;
        private readonly Stack<BaseView> _views = new();

        public VisualElement SharedViewRoot => _sharedRoot;
        public StageLayoutState CurrentStageLayoutState { get; private set; }

        protected override void OnInit()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            _graphicSaveData = SettingManager.Instance.SaveData.GraphicSaveData;
            _aspectPreset = _graphicSaveData.aspectPreset;

            _panelSettings = Addressables.LoadAssetAsync<PanelSettings>("PanelSettings").WaitForCompletion();
            _panelSettings.sortingOrder = 0;
            _panelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            _panelSettings.referenceResolution = Ratio16x9StageSize;
            _panelSettings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            _panelSettings.match = 0.5f;
            _panelSettings.themeStyleSheet = Addressables.LoadAssetAsync<ThemeStyleSheet>("DefaultViewTheme").WaitForCompletion();

            _managerObject = new GameObject("@ViewManager");
            Object.DontDestroyOnLoad(_managerObject);

            _document = _managerObject.AddComponent<UIDocument>();
            _document.panelSettings = _panelSettings;

            SetupRoot();
            _document.rootVisualElement.RegisterCallback<GeometryChangedEvent>(OnRootGeometryChanged);
            GraphicManager.Instance.WindowSizeChanged += OnWindowSizeChanged;
        }

        protected override void OnDispose()
        {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            _document?.rootVisualElement.UnregisterCallback<GeometryChangedEvent>(OnRootGeometryChanged);
            if (GraphicManager.Instance != null)
            {
                GraphicManager.Instance.WindowSizeChanged -= OnWindowSizeChanged;
            }

            if (_managerObject != null)
            {
                Object.Destroy(_managerObject);
                _managerObject = null;
            }

            _graphicSaveData = null;
            Clear();
        }

        private void OnSceneUnloaded(UnityEngine.SceneManagement.Scene unloadedScene)
        {
            Clear();
        }

        public void Push(BaseView baseView)
        {
            if (baseView == null)
            {
                return;
            }

            VisualTreeAsset visualTreeAsset =
                Addressables.LoadAssetAsync<VisualTreeAsset>(baseView.GetType().Name).WaitForCompletion();

            if (visualTreeAsset == null)
            {
                Debug.LogError($"Failed to show view {baseView.GetType().Name}: VisualTreeAsset is null.");
                return;
            }

            VisualElement container = new VisualElement
            {
                name = $"{baseView.GetType().Name}-container"
            };

            container.style.position = Position.Absolute;
            container.style.left = 0;
            container.style.top = 0;
            container.style.right = 0;
            container.style.bottom = 0;

            visualTreeAsset.CloneTree(container);
            baseView.Bind(container);

            _viewLayer.Add(container);
            _views.Push(baseView);
        }

        public void Pop()
        {
            if (_views.Count == 0)
            {
                return;
            }

            BaseView top = _views.Pop();
            top.Dispose();

            if (top.Root != null)
            {
                top.Root.RemoveFromHierarchy();
            }

            if (_views.Count > 0)
            {
                _views.Peek().SetVisible(true);
            }
        }

        public void Clear()
        {
            while (_views.Count > 0)
            {
                BaseView top = _views.Pop();
                top.Dispose();

                if (top.Root != null)
                {
                    top.Root.RemoveFromHierarchy();
                }
            }

            _viewLayer?.Clear();
        }

        public BaseView Peek()
        {
            return _views.Count > 0 ? _views.Peek() : null;
        }

        public void RefreshStageLayout()
        {
            UpdateStageLayout();
        }

        public EDisplayAspectPreset GetAspectPreset()
        {
            return _aspectPreset;
        }

        public void SetAspectPreset(EDisplayAspectPreset preset)
        {
            if (_aspectPreset == preset)
            {
                return;
            }

            _aspectPreset = preset;
            _graphicSaveData.aspectPreset = preset;
            UpdateStageLayout();
        }

        private void SetupRoot()
        {
            VisualElement root = _document.rootVisualElement;
            root.Clear();

            _sharedRoot = new VisualElement { name = "shared-view-root" };
            _sharedRoot.style.flexGrow = 1;
            _sharedRoot.style.position = Position.Relative;
            _sharedRoot.style.overflow = Overflow.Hidden;

            _letterbox = new VisualElement { name = "letterbox-frame" };
            _letterbox.AddToClassList("app-letterbox");

            _viewport = new VisualElement { name = "viewport-frame" };
            _viewport.AddToClassList("app-viewport");
            _viewport.style.position = Position.Absolute;
            _viewport.style.overflow = Overflow.Hidden;

            _stage = new VisualElement { name = "stage-frame" };
            _stage.AddToClassList("app-stage");
            _stage.style.position = Position.Relative;
            _stage.style.left = 0;
            _stage.style.top = 0;
            _stage.style.width = Length.Percent(100);
            _stage.style.height = Length.Percent(100);

            _viewLayer = new VisualElement { name = "view-layer" };
            _viewLayer.AddToClassList("app-view-layer");
            _viewLayer.style.position = Position.Relative;
            _viewLayer.style.width = Length.Percent(100);
            _viewLayer.style.height = Length.Percent(100);

            _stage.Add(_viewLayer);
            _viewport.Add(_stage);
            _letterbox.Add(_viewport);
            _sharedRoot.Add(_letterbox);
            root.Add(_sharedRoot);

            UpdateStageLayout();
        }

        private void OnRootGeometryChanged(GeometryChangedEvent evt)
        {
            UpdateStageLayout();
        }

        public void OnWindowSizeChanged(int width, int height)
        {
            UpdateStageLayout();
        }

        private void UpdateStageLayout()
        {
            if (_sharedRoot == null || _viewport == null || _stage == null)
            {
                return;
            }

            float frameWidth = _sharedRoot.resolvedStyle.width;
            float frameHeight = _sharedRoot.resolvedStyle.height;
            if (frameWidth <= 0f || frameHeight <= 0f)
            {
                return;
            }

            StageLayoutState stageLayoutState = BuildStageLayoutState(frameWidth, frameHeight);
            ApplyStageLayout(stageLayoutState);
        }

        private StageLayoutState BuildStageLayoutState(float frameWidth, float frameHeight)
        {
            float currentAspectRatio = frameWidth / frameHeight;
            StageLayout stageLayout = GetStageLayout(currentAspectRatio);
            Rect viewportRect = new Rect(
                (frameWidth - stageLayout.LogicalSize.x) * 0.5f,
                (frameHeight - stageLayout.LogicalSize.y) * 0.5f,
                stageLayout.LogicalSize.x,
                stageLayout.LogicalSize.y);

            return new StageLayoutState(
                stageLayout.LogicalSize,
                stageLayout.ScaleMode,
                stageLayout.AspectBucket,
                currentAspectRatio,
                viewportRect,
                new Vector2Int(Mathf.RoundToInt(frameWidth), Mathf.RoundToInt(frameHeight)));
        }

        private StageLayout GetStageLayout(float currentAspectRatio)
        {
            switch (_aspectPreset)
            {
                case EDisplayAspectPreset.Ratio4x3:
                    return new StageLayout(Ratio4x3StageSize, StageScaleMode.Keep, AspectBucket.Narrow);
                case EDisplayAspectPreset.Ratio16x10:
                    return new StageLayout(Ratio16x10StageSize, StageScaleMode.Keep, AspectBucket.Narrow);
                case EDisplayAspectPreset.Ratio16x9:
                    return new StageLayout(Ratio16x9StageSize, StageScaleMode.Keep, AspectBucket.Standard);
                case EDisplayAspectPreset.Ratio21x9:
                    return new StageLayout(Ratio21x9StageSize, StageScaleMode.Keep, AspectBucket.UltraWide);
                default:
                    if (currentAspectRatio > MaxWideRatio)
                    {
                        return new StageLayout(AutoUltraWideStageSize, StageScaleMode.KeepWidth, AspectBucket.UltraWide);
                    }

                    if (currentAspectRatio < MaxNarrowRatio)
                    {
                        return new StageLayout(AutoNarrowStageSize, StageScaleMode.KeepHeight, AspectBucket.Narrow);
                    }

                    return new StageLayout(GetExpandedStageSize(currentAspectRatio), StageScaleMode.Expand, AspectBucket.Standard);
            }
        }

        private static Vector2Int GetExpandedStageSize(float currentAspectRatio)
        {
            float baseWidth = AutoStandardStageSize.x;
            float baseHeight = AutoStandardStageSize.y;
            float baseAspectRatio = baseWidth / baseHeight;

            if (currentAspectRatio >= baseAspectRatio)
            {
                return new Vector2Int(Mathf.RoundToInt(baseHeight * currentAspectRatio), AutoStandardStageSize.y);
            }

            return new Vector2Int(AutoStandardStageSize.x, Mathf.RoundToInt(baseWidth / currentAspectRatio));
        }

        private void ApplyStageLayout(StageLayoutState state)
        {
            if (_panelSettings == null)
            {
                return;
            }

            CurrentStageLayoutState = state;
            ApplyPanelScale(state);
            ApplySharedRootClasses(state);

            _viewport.style.width = state.ViewportRect.width;
            _viewport.style.height = state.ViewportRect.height;
            _viewport.style.left = state.ViewportRect.x;
            _viewport.style.top = state.ViewportRect.y;
        }

        private void ApplyPanelScale(StageLayoutState state)
        {
            _panelSettings.referenceResolution = state.LogicalResolution;

            switch (state.ScaleMode)
            {
                case StageScaleMode.KeepWidth:
                    _panelSettings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
                    _panelSettings.match = 0f;
                    break;
                case StageScaleMode.KeepHeight:
                    _panelSettings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
                    _panelSettings.match = 1f;
                    break;
                case StageScaleMode.Expand:
                    _panelSettings.screenMatchMode = PanelScreenMatchMode.Expand;
                    break;
                default:
                    _panelSettings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
                    _panelSettings.match = state.ActualAspectRatio > ((float)state.LogicalResolution.x / state.LogicalResolution.y)
                        ? 1f
                        : 0f;
                    break;
            }
        }

        private void ApplySharedRootClasses(StageLayoutState state)
        {
            _sharedRoot.EnableInClassList("app-stage--keep", state.ScaleMode == StageScaleMode.Keep);
            _sharedRoot.EnableInClassList("app-stage--keep-height", state.ScaleMode == StageScaleMode.KeepHeight);
            _sharedRoot.EnableInClassList("app-stage--expand", state.ScaleMode == StageScaleMode.Expand);
            _sharedRoot.EnableInClassList("app-stage--keep-width", state.ScaleMode == StageScaleMode.KeepWidth);

            _sharedRoot.EnableInClassList("app--aspect-narrow", state.AspectBucket == AspectBucket.Narrow);
            _sharedRoot.EnableInClassList("app--aspect-standard", state.AspectBucket == AspectBucket.Standard);
            _sharedRoot.EnableInClassList("app--aspect-ultrawide", state.AspectBucket == AspectBucket.UltraWide);
        }
    }
}
