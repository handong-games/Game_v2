using System;
using System.IO;
using UnityEngine;

namespace Game.Core.Managers.Save
{
    public sealed class JsonFileStore<T> where T : ISave, new()
    {
        private readonly string _path;

        public JsonFileStore(string fileName)
        {
            _path = Path.Combine(Application.persistentDataPath, fileName);
        }

        public T Load()
        {
            if (!File.Exists(_path))
            {
                return new T();
            }

            try
            {
                string json = File.ReadAllText(_path);
                return JsonUtility.FromJson<T>(json) ?? new T();
            }
            catch
            {
                return new T();
            }
        }

        public void Save(T data)
        {
            string json = JsonUtility.ToJson(data, true);
            string tempPath = _path + ".tmp";

            Directory.CreateDirectory(Path.GetDirectoryName(_path) ?? string.Empty);
            File.WriteAllText(tempPath, json);

            if (File.Exists(_path))
            {
                File.Delete(_path);
            }

            File.Move(tempPath, _path);
        }
    }
}
