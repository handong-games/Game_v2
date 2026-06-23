using System.Collections.Generic;
using Game.Core.Managers.Garphic;
using Game.Core.Ports;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Game.Core.Managers.View
{
    public partial class ViewManager : IViewHost, global::System.IDisposable
    {
        private UIDocument _document;
        private PanelSettings _panelSettings;
        private GameObject _managerObject;
        private VisualElement _rootLayer;
        private VisualElement _viewLayer;
        private VisualElement _overlayLayer;
        private readonly Stack<BaseView> _views = new();
        private readonly HashSet<BaseView> _attachedViews = new();
        private readonly GraphicManager _graphicManager;
        private bool _initialized;

        public VisualElement RootLayer => _rootLayer;
        public VisualElement OverlayLayer => _overlayLayer;

        public ViewManager(GraphicManager graphicManager)
        {
            _graphicManager = graphicManager;
        }

        public void Initialize()
        {
            if (_initialized)
                return;

            _initialized = true;

            /* Event */
            SceneManager.sceneUnloaded += OnSceneUnloaded;

            /* ViewManagerBehavior */
            _managerObject = new GameObject("@ViewManager");
            Object.DontDestroyOnLoad(_managerObject);
            _managerObject.AddComponent<ViewManagerBehavior>().Initialize(this);
            
            /* PanelSettings */
            _panelSettings = Addressables.LoadAssetAsync<PanelSettings>("PanelSettings").WaitForCompletion();
            _panelSettings.themeStyleSheet = Addressables.LoadAssetAsync<ThemeStyleSheet>("DefaultViewTheme").WaitForCompletion();
            _panelSettings.scaleMode = PanelScaleMode.ConstantPixelSize;
            _panelSettings.scale = 1f;
            
            _document = _managerObject.AddComponent<UIDocument>();
            _document.panelSettings = _panelSettings;

            /* VisualElement Layer */
            VisualElement root = _document.rootVisualElement;
            
            _rootLayer = new VisualElement { name = "root-layer" };
            _rootLayer.style.width = Length.Percent(100);
            _rootLayer.style.height = Length.Percent(100);
            _rootLayer.style.position = Position.Relative;
            _rootLayer.style.overflow = Overflow.Hidden;
            _rootLayer.style.backgroundColor = Color.black;

            _viewLayer = new VisualElement { name = "view-layer" };
            _viewLayer.AddToClassList("app-view-layer");
            _viewLayer.style.position = Position.Absolute;
            _viewLayer.style.overflow = Overflow.Hidden;

            _overlayLayer = new VisualElement
            {
                name = "overlay-layer",
                pickingMode = PickingMode.Ignore
            };
            _overlayLayer.AddToClassList("app-overlay-layer");
            _overlayLayer.style.position = Position.Absolute;
            _overlayLayer.style.left = 0;
            _overlayLayer.style.top = 0;
            _overlayLayer.style.right = 0;
            _overlayLayer.style.bottom = 0;
            _overlayLayer.style.backgroundColor = Color.black;
            
            _rootLayer.Add(_viewLayer);
            _rootLayer.Add(_overlayLayer);
            
            root.Add(_rootLayer);

            GraphicManager.ViewAspectChanged += OnViewAspectChanged;
            OnViewAspectChanged(_graphicManager.GetAspectPreset());
        }

        public void Dispose()
        {
            if (!_initialized)
                return;

            _initialized = false;

            /* Event */
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            GraphicManager.ViewAspectChanged -= OnViewAspectChanged;

            Clear();

            if (_managerObject != null)
            {
                Object.Destroy(_managerObject);
                _managerObject = null;
            }

            _document = null;
            _panelSettings = null;
            _rootLayer = null;
            _viewLayer = null;
            _overlayLayer = null;
        }

        private void OnSceneUnloaded(UnityEngine.SceneManagement.Scene unloadedScene)
        {
            Clear();
        }

        public void Push(BaseView baseView)
        {
            PushAndOwn(baseView);
        }

        public void PushAndOwn(BaseView baseView)
        {
            if (baseView == null)
            {
                return;
            }

            if (_attachedViews.Contains(baseView))
            {
                Debug.LogError($"Failed to push view {baseView.GetType().Name}: view is already attached as a non-owned view.");
                return;
            }

            if (!EnsureViewBound(baseView))
                return;

            AttachRootToViewLayer(baseView);
            _views.Push(baseView);

            OnViewportSizeChanged(Screen.width, Screen.height);
        }

        public void Attach(BaseView baseView)
        {
            if (baseView == null)
            {
                return;
            }

            if (_views.Contains(baseView))
            {
                Debug.LogError($"Failed to attach view {baseView.GetType().Name}: view is already owned by the legacy stack.");
                return;
            }

            if (!EnsureViewBound(baseView))
                return;

            AttachRootToViewLayer(baseView);
            _attachedViews.Add(baseView);

            OnViewportSizeChanged(Screen.width, Screen.height);
        }

        public void Detach(BaseView baseView)
        {
            if (baseView == null)
            {
                return;
            }

            _attachedViews.Remove(baseView);

            if (baseView.Root != null)
            {
                baseView.Root.RemoveFromHierarchy();
            }
        }

        public void DetachAll()
        {
            List<BaseView> attachedViews = new(_attachedViews);
            for (int i = 0; i < attachedViews.Count; i++)
            {
                Detach(attachedViews[i]);
            }
        }

        public void Pop()
        {
            PopAndDispose();
        }

        public void PopAndDispose()
        {
            if (_views.Count == 0)
            {
                return;
            }

            BaseView top = _views.Pop();
            DisposeAndDetachOwnedView(top);

            if (_views.Count > 0)
            {
                _views.Peek().SetVisible(true);
            }
        }

        public void Clear()
        {
            ClearOwnedViewsAndDetachAll();
        }

        public void ClearOwnedViewsAndDetachAll()
        {
            while (_views.Count > 0)
            {
                BaseView top = _views.Pop();
                DisposeAndDetachOwnedView(top);
            }

            DetachAll();
            if (_viewLayer == null || _overlayLayer == null)
                return;

            _viewLayer.Clear();
            /* Todo : 리팩토링 필요 */
            _overlayLayer.ClearClassList();
            _overlayLayer.AddToClassList("app-overlay-layer");
        }

        public BaseView Peek()
        {
            return _views.Count > 0 ? _views.Peek() : null;
        }

        private static void DisposeAndDetachOwnedView(BaseView view)
        {
            view.Dispose();

            if (view.Root != null)
            {
                view.Root.RemoveFromHierarchy();
            }
        }

        private bool EnsureViewBound(BaseView baseView)
        {
            if (baseView.Root != null)
                return true;

            VisualTreeAsset visualTreeAsset =
                Addressables.LoadAssetAsync<VisualTreeAsset>(baseView.GetType().Name).WaitForCompletion();

            if (visualTreeAsset == null)
            {
                Debug.LogError($"Failed to show view {baseView.GetType().Name}: VisualTreeAsset is null.");
                return false;
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

            VisualElement logicalRoot = new VisualElement
            {
                name = $"{baseView.GetType().Name}-logical-root"
            };
            
            logicalRoot.AddToClassList("app-view__logical-root");
            logicalRoot.style.position = Position.Absolute;
            logicalRoot.style.left = 0;
            logicalRoot.style.top = 0;

            container.Add(logicalRoot);
            visualTreeAsset.CloneTree(logicalRoot);
            baseView.Bind(container, logicalRoot);
            return true;
        }

        private void AttachRootToViewLayer(BaseView baseView)
        {
            if (baseView.Root == null)
                return;

            if (baseView.Root.parent == _viewLayer)
                return;

            baseView.Root.RemoveFromHierarchy();
            _viewLayer.Add(baseView.Root);
        }
    }
}
