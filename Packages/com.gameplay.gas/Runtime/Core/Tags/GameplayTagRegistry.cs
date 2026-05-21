using System;
using System.Collections.Generic;

namespace Gameplay.GAS
{
    public static class GameplayTagRegistry
    {
        private static readonly Dictionary<string, int> IdByName = new(StringComparer.Ordinal);
        private static readonly List<string> Names = new();
        private static readonly List<int> DirectParentIds = new();
        private static readonly List<HashSet<int>> ParentIds = new();

        public static GameplayTag Request(string name)
        {
            return new GameplayTag(RequestId(name));
        }

        internal static int RequestId(string name)
        {
            string normalizedName = NormalizeName(name);
            if (string.IsNullOrEmpty(normalizedName))
                return 0;

            if (IdByName.TryGetValue(normalizedName, out int existingId))
                return existingId;

            int directParentId = RequestDirectParentId(normalizedName);
            int id = Names.Count + 1;

            IdByName.Add(normalizedName, id);
            Names.Add(normalizedName);
            DirectParentIds.Add(directParentId);
            ParentIds.Add(BuildParentIds(directParentId));

            return id;
        }

        public static string GetName(GameplayTag tag)
        {
            return tag.IsValid ? Names[tag.Id - 1] : string.Empty;
        }

        public static bool MatchesTag(GameplayTag tag, GameplayTag tagToCheck)
        {
            if (!tag.IsValid || !tagToCheck.IsValid)
                return false;

            if (tag.Equals(tagToCheck))
                return true;

            return ParentIds[tag.Id - 1].Contains(tagToCheck.Id);
        }

        public static GameplayTag RequestDirectParent(GameplayTag tag)
        {
            if (!tag.IsValid)
                return default;

            int parentId = DirectParentIds[tag.Id - 1];
            return parentId > 0 ? new GameplayTag(parentId) : default;
        }

        private static string NormalizeName(string name)
        {
            string normalizedName = name?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(normalizedName))
                return string.Empty;

            if (normalizedName[0] == '.' || normalizedName[normalizedName.Length - 1] == '.')
                return string.Empty;

            if (normalizedName.IndexOf("..", StringComparison.Ordinal) >= 0)
                return string.Empty;

            return normalizedName;
        }

        private static int RequestDirectParentId(string name)
        {
            int separatorIndex = name.LastIndexOf('.');
            return separatorIndex < 0 ? 0 : RequestId(name.Substring(0, separatorIndex));
        }

        private static HashSet<int> BuildParentIds(int directParentId)
        {
            HashSet<int> parentIds = new();
            int parentId = directParentId;

            while (parentId > 0)
            {
                parentIds.Add(parentId);
                parentId = DirectParentIds[parentId - 1];
            }

            return parentIds;
        }
    }
}
