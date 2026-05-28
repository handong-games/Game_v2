using System;
using System.Collections.Generic;

namespace Gameplay.GAS
{
    public static class GameplayTagRegistry
    {
        private static readonly Dictionary<string, int> IdByPath = new(StringComparer.Ordinal);
        private static readonly List<string> PathById = new();
        private static readonly List<int> ParentIdById = new();

        private static int _nextId = 1;

        public static GameplayTag Define(string path)
        {
            if (!TryNormalizePath(path, out string normalizedPath))
                return default;

            int runtimeId = GetOrCreateId(normalizedPath);
            return new GameplayTag(normalizedPath, runtimeId);
        }

        public static GameplayTag ResolveDefined(string path)
        {
            if (!TryNormalizePath(path, out string normalizedPath))
                return default;

            return IdByPath.TryGetValue(normalizedPath, out int runtimeId)
                ? new GameplayTag(normalizedPath, runtimeId)
                : default;
        }

        public static int ResolveRuntimeId(string path)
        {
            if (!TryNormalizePath(path, out string normalizedPath))
                return 0;

            return IdByPath.TryGetValue(normalizedPath, out int runtimeId)
                ? runtimeId
                : 0;
        }

        public static int EnsureRuntimeId(ref GameplayTag tag)
        {
            if (tag.RuntimeId > 0)
                return tag.RuntimeId;

            int runtimeId = ResolveRuntimeId(tag.Path);
            if (runtimeId > 0)
                tag = tag.WithRuntimeId(runtimeId);

            return runtimeId;
        }

        public static bool MatchesTag(GameplayTag tag, GameplayTag tagToCheck)
        {
            int tagId = EnsureRuntimeId(ref tag);
            int tagToCheckId = EnsureRuntimeId(ref tagToCheck);

            if (tagId <= 0 || tagToCheckId <= 0)
                return false;

            if (tagId > ParentIdById.Count || tagToCheckId > ParentIdById.Count)
                return false;

            if (tagId == tagToCheckId)
                return true;

            int parentId = ParentIdById[tagId - 1];
            int remainingParents = ParentIdById.Count;
            while (parentId > 0 && remainingParents-- > 0)
            {
                if (parentId > ParentIdById.Count)
                    return false;

                if (parentId == tagToCheckId)
                    return true;

                parentId = ParentIdById[parentId - 1];
            }

            return false;
        }

        public static GameplayTag GetDirectParent(GameplayTag tag)
        {
            int runtimeId = EnsureRuntimeId(ref tag);
            if (runtimeId <= 0)
                return default;

            if (runtimeId > ParentIdById.Count)
                return default;

            int parentId = ParentIdById[runtimeId - 1];
            return parentId > 0 && parentId <= PathById.Count
                ? new GameplayTag(PathById[parentId - 1], parentId)
                : default;
        }

        private static int GetOrCreateId(string path)
        {
            if (IdByPath.TryGetValue(path, out int existingId))
                return existingId;

            int parentId = ResolveParentId(path);
            int runtimeId = _nextId++;
            IdByPath.Add(path, runtimeId);
            PathById.Add(path);
            ParentIdById.Add(parentId);
            return runtimeId;
        }

        private static int ResolveParentId(string path)
        {
            string parentPath = GetParentPath(path);
            return string.IsNullOrWhiteSpace(parentPath)
                ? 0
                : GetOrCreateId(parentPath);
        }

        private static string GetParentPath(string path)
        {
            int separatorIndex = path.LastIndexOf('.');
            return separatorIndex < 0 ? null : path.Substring(0, separatorIndex);
        }

        private static bool TryNormalizePath(string path, out string normalizedPath)
        {
            normalizedPath = path?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(normalizedPath))
                return false;

            if (normalizedPath.StartsWith(".", StringComparison.Ordinal) ||
                normalizedPath.EndsWith(".", StringComparison.Ordinal) ||
                normalizedPath.Contains("..", StringComparison.Ordinal))
            {
                return false;
            }

            return true;
        }
    }
}
