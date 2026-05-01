using System.Collections.Generic;
using Game.Core.Managers.Garphic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Game.Core.Managers.View
{
    [ManagerDependency(typeof(GraphicManager))]
    public partial class ViewManager : BaseManager<ViewManager>
    {
        private UIDocument _document;
        private PanelSettings _panelSettings;
        private GameObject _managerObject;
        private VisualElement _rootLayer;
        private VisualElement _viewLayer;
        private VisualElement _overlayLayer;
        private readonly Stack<BaseView> _views = new();

        public VisualElement RootLayer => _rootLayer;
        public VisualElement OverlayLayer => _overlayLayer;

        protected override void OnInit()
        {
            /* Event */
            SceneManager.sceneUnloaded += OnSceneUnloaded;

            /* ViewManagerBehavior */
            _managerObject = new GameObject("@ViewManager");
            Object.DontDestroyOnLoad(_managerObject);
            _managerObject.AddComponent<ViewManagerBehavior>();
            
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
        }

        protected override void OnPostInit()
        {
            GraphicManager.ViewAspectChanged += OnViewAspectChanged;
            OnViewAspectChanged(GraphicManager.Instance.GetAspectPreset());
        }

        protected override void OnDispose()
        {
            /* Event */
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            GraphicManager.ViewAspectChanged -= OnViewAspectChanged;

            if (_managerObject != null)
            {
                Object.Destroy(_managerObject);
                _managerObject = null;
            }

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
            _viewLayer.Add(container);
            _views.Push(baseView);

            OnViewportSizeChanged(Screen.width, Screen.height);
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

            _viewLayer.Clear();
            /* Todo : 리팩토링 필요 */
            _overlayLayer.ClearClassList();
            _overlayLayer.AddToClassList("app-overlay-layer");
        }

        public BaseView Peek()
        {
            return _views.Count > 0 ? _views.Peek() : null;
        }
    }
}
