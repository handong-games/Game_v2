using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Data/Save Catalog")]
    public sealed class SaveCatalogModel : ScriptableObject
    {
        [SerializeField]
        private SaveCatalogFile[] _files;

        public IReadOnlyList<SaveCatalogFile> Files => _files;
    }

    [Serializable]
    public sealed class SaveCatalogFile
    {
        [SerializeField]
        private string _fileName;

        [SerializeField]
        private SaveCatalogSection[] _sections;

        public string FileName => _fileName;
        public IReadOnlyList<SaveCatalogSection> Sections => _sections;
    }

    [Serializable]
    public sealed class SaveCatalogSection
    {
        [SerializeField]
        private string _sectionName;

#if UNITY_EDITOR
        [SerializeField]
        private MonoScript _stateScript;
#endif

        public string SectionName => _sectionName;

#if UNITY_EDITOR
        public Type StateType => _stateScript != null ? _stateScript.GetClass() : null;
#endif
    }
}
