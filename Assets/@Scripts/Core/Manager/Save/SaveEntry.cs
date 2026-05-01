using System;

namespace Game.Core.Managers.Save
{
    internal sealed class SaveEntry
    {
        public SaveEntry(
            string fileName,
            string sectionName,
            Type saveType,
            Func<object> createSave,
            Action<object> load)
        {
            FileName = fileName;
            SectionName = sectionName;
            SaveType = saveType;
            CreateSave = createSave;
            Load = load;
        }

        public string FileName { get; }
        public string SectionName { get; }
        public Type SaveType { get; }
        public Func<object> CreateSave { get; }
        public Action<object> Load { get; }
    }
}
