using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gameplay.GAS.Editor
{
    public sealed class GameplayTagContainerPickerWindow : EditorWindow
    {
        private sealed class ToggleBinding
        {
            public string Path { get; set; }
            public EventCallback<ChangeEvent<bool>> Callback { get; set; }
        }

        private Action<IReadOnlyList<GameplayTag>> _onSelected;
        private string _rootPath;
        private HashSet<string> _selectedPaths;
        private List<GameplayTagDescriptor> _descriptors;
        private List<GameplayTagDescriptor> _filteredDescriptors;
        private ListView _listView;

        public static void Show(
            Rect screenRect,
            string rootPath,
            HashSet<string> selectedPaths,
            Action<IReadOnlyList<GameplayTag>> onSelected)
        {
            GameplayTagContainerPickerWindow window = CreateInstance<GameplayTagContainerPickerWindow>();
            window._rootPath = rootPath;
            window._selectedPaths = selectedPaths ?? new HashSet<string>(StringComparer.Ordinal);
            window._onSelected = onSelected;
            window.ShowAsDropDown(screenRect, new Vector2(Mathf.Max(screenRect.width, 360f), 420f));
        }

        private void CreateGUI()
        {
            _descriptors = new List<GameplayTagDescriptor>(GameplayTagPickerDataSource.GetDescriptors(_rootPath));
            _filteredDescriptors = new List<GameplayTagDescriptor>(_descriptors);

            rootVisualElement.style.flexDirection = FlexDirection.Column;
            rootVisualElement.style.paddingLeft = 4f;
            rootVisualElement.style.paddingRight = 4f;
            rootVisualElement.style.paddingTop = 4f;
            rootVisualElement.style.paddingBottom = 4f;

            TextField searchField = new() { label = "Search" };
            searchField.RegisterValueChangedCallback(evt => RefreshFilter(evt.newValue));
            rootVisualElement.Add(searchField);

            _listView = new ListView
            {
                itemsSource = _filteredDescriptors,
                selectionType = SelectionType.None,
                fixedItemHeight = 22f
            };
            _listView.style.flexGrow = 1f;
            _listView.makeItem = () => new Toggle();
            _listView.bindItem = (element, index) =>
            {
                Toggle toggle = (Toggle)element;
                GameplayTagDescriptor descriptor = _filteredDescriptors[index];

                toggle.label = descriptor.Path;
                toggle.SetValueWithoutNotify(_selectedPaths.Contains(descriptor.Path));

                EventCallback<ChangeEvent<bool>> callback = evt =>
                {
                    if (evt.newValue)
                        _selectedPaths.Add(descriptor.Path);
                    else
                        _selectedPaths.Remove(descriptor.Path);
                };

                toggle.userData = new ToggleBinding
                {
                    Path = descriptor.Path,
                    Callback = callback
                };
                toggle.RegisterValueChangedCallback(callback);
            };
            _listView.unbindItem = (element, _) =>
            {
                Toggle toggle = (Toggle)element;
                if (toggle.userData is ToggleBinding binding)
                {
                    toggle.UnregisterValueChangedCallback(binding.Callback);
                    toggle.userData = null;
                }
            };
            rootVisualElement.Add(_listView);

            Button applyButton = new(() =>
            {
                List<GameplayTag> selectedTags = new();

                for (int i = 0; i < _descriptors.Count; i++)
                {
                    GameplayTagDescriptor descriptor = _descriptors[i];
                    if (_selectedPaths.Contains(descriptor.Path))
                        selectedTags.Add(descriptor.Tag);
                }

                _onSelected?.Invoke(selectedTags);
                Close();
            })
            {
                text = "Apply"
            };
            rootVisualElement.Add(applyButton);
        }

        private void RefreshFilter(string keyword)
        {
            _filteredDescriptors.Clear();

            for (int i = 0; i < _descriptors.Count; i++)
            {
                GameplayTagDescriptor descriptor = _descriptors[i];
                if (string.IsNullOrWhiteSpace(keyword) ||
                    descriptor.Path.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    _filteredDescriptors.Add(descriptor);
                }
            }

            _listView.Rebuild();
        }
    }
}
