using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Game.Core.Managers.Dependency;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Game.Core.Managers.Save
{
    [ManagerDependency(typeof(DependencyManager))]
    public sealed class SaveManager : BaseManager<SaveManager>
    {
        private readonly List<SaveEntry> _entries = new();

        protected override void OnInit()
        {
            Application.quitting += SaveAll;
        }

        protected override void OnPostInit()
        {
            // Register save states after all managers finish OnInit.
            RegisterAll();

            // Load saved data into registered states before gameplay starts.
            LoadAll();
        }

        protected override void OnDispose()
        {
            Application.quitting -= SaveAll;

            _entries.Clear();
        }

        public void Register<TState, TSave>(string fileName, string sectionName)
            where TState : class, ISave<TSave>
            where TSave : SaveData, new()
        {
            TState state = DependencyManager.Instance.Resolve<TState>();

            _entries.Add(new SaveEntry(
                fileName,
                sectionName,
                typeof(TSave),
                () => state.ToSave(),
                save => state.LoadFrom((TSave)save)));
        }

        public void SaveAll()
        {
            foreach (IGrouping<string, SaveEntry> group in _entries.GroupBy(entry => entry.FileName))
            {
                JObject root = ReadFile(group.Key);

                foreach (SaveEntry entry in group)
                {
                    root[entry.SectionName] = JToken.FromObject(entry.CreateSave());
                }

                WriteFile(group.Key, root);
            }
        }

        private void RegisterAll()
        {
            for (int i = 0; i < SaveRegistry.All.Length; i++)
            {
                SaveRegistry.All[i].Register(this);
            }
        }

        private void LoadAll()
        {
            Dictionary<string, JObject> files = LoadFiles();

            for (int i = 0; i < _entries.Count; i++)
            {
                SaveEntry entry = _entries[i];

                object save = files.TryGetValue(entry.FileName, out JObject root)
                    ? root[entry.SectionName]?.ToObject(entry.SaveType) ?? Activator.CreateInstance(entry.SaveType)
                    : Activator.CreateInstance(entry.SaveType);

                entry.Load(save);
            }
        }

        private Dictionary<string, JObject> LoadFiles()
        {
            Dictionary<string, JObject> files = new();

            for (int i = 0; i < _entries.Count; i++)
            {
                string fileName = _entries[i].FileName;

                if (files.ContainsKey(fileName))
                    continue;

                files.Add(fileName, ReadFile(fileName));
            }

            return files;
        }

        private JObject ReadFile(string fileName)
        {
            string path = GetPath(fileName);
            if (File.Exists(path))
            {
                return ReadJson(path);
            }

            return new JObject();
        }

        private JObject ReadJson(string path)
        {
            try
            {
                return JObject.Parse(File.ReadAllText(path));
            }
            catch
            {
                return new JObject();
            }
        }

        private void WriteFile(string fileName, JObject root)
        {
            string path = GetPath(fileName);
            string tempPath = path + ".tmp";

            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);
            File.WriteAllText(tempPath, root.ToString(Formatting.Indented));

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            File.Move(tempPath, path);
        }

        private string GetPath(string fileName)
        {
            return Path.Combine(Application.persistentDataPath, fileName);
        }
    }
}
