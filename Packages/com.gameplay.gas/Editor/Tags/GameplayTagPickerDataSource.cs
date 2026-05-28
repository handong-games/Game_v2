using System;
using System.Collections.Generic;

namespace Gameplay.GAS.Editor
{
    public static class GameplayTagPickerDataSource
    {
        public static IReadOnlyList<GameplayTagDescriptor> GetDescriptors(string rootPath = null)
        {
            IReadOnlyList<GameplayTagFieldDefinition> fields = GameplayTagProviderScanner.FindTagFields();
            List<GameplayTagDescriptor> descriptors = new();

            for (int i = 0; i < fields.Count; i++)
            {
                GameplayTagFieldDefinition field = fields[i];
                string path = field.Tag.Path;

                if (!string.IsNullOrWhiteSpace(rootPath) &&
                    path != rootPath &&
                    !path.StartsWith(rootPath + ".", StringComparison.Ordinal))
                {
                    continue;
                }

                descriptors.Add(new GameplayTagDescriptor(
                    path,
                    field.ProviderType.Name,
                    field.FieldName,
                    field.Tag));
            }

            descriptors.Sort((a, b) => StringComparer.Ordinal.Compare(a.Path, b.Path));
            return descriptors;
        }
    }
}
