using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gameplay.GAS.Editor
{
    public sealed class GameplayTagPickerWindow : EditorWindow
    {
        private Action<GameplayTag> _onSelected;
        private string _rootPath;
        private List<GameplayTagDescriptor> _descriptors;
        private List<GameplayTagDescriptor> _filteredDescriptors;
        private ListView _listView;

        public static void Show(Rect screenRect, string rootPath, Action<GameplayTag> onSelected)
        {
            GameplayTagPickerWindow window = CreateInstance<GameplayTagPickerWindow>();
            window._rootPath = rootPath;
            window._onSelected = onSelected;
            window.ShowAsDropDown(screenRect, new Vector2(Mathf.Max(screenRect.width, 320f), 360f));
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
                selectionType = SelectionType.Single,
                fixedItemHeight = 22f
            };
            _listView.style.flexGrow = 1f;
            _listView.makeItem = () => new Label();
            _listView.bindItem = (element, index) =>
            {
                ((Label)element).text = _filteredDescriptors[index].Path;
            };
            _listView.itemsChosen += chosenItems =>
            {
                foreach (object chosenItem in chosenItems)
                {
                    GameplayTagDescriptor descriptor = (GameplayTagDescriptor)chosenItem;
                    _onSelected?.Invoke(descriptor.Tag);
                    Close();
                    break;
                }
            };

            rootVisualElement.Add(_listView);
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
