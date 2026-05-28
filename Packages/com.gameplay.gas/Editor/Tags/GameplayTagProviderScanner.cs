using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Gameplay.GAS.Editor
{
    public static class GameplayTagProviderScanner
    {
        private static GameplayTagFieldDefinition[] _cachedDefinitions;

        public static IReadOnlyList<GameplayTagFieldDefinition> FindTagFields()
        {
            if (_cachedDefinitions != null)
                return _cachedDefinitions;

            List<GameplayTagFieldDefinition> definitions = new();

            foreach (Type providerType in FindProviderTypes())
            {
                FieldInfo[] fields = providerType.GetFields(BindingFlags.Public | BindingFlags.Static);

                for (int i = 0; i < fields.Length; i++)
                {
                    FieldInfo field = fields[i];
                    if (field.FieldType != typeof(GameplayTag) || !field.IsInitOnly)
                        continue;

                    GameplayTag tag = (GameplayTag)field.GetValue(null);
                    if (!tag.IsValid)
                        continue;

                    definitions.Add(new GameplayTagFieldDefinition(providerType, field.Name, tag));
                }
            }

            _cachedDefinitions = definitions.ToArray();
            return _cachedDefinitions;
        }

        public static void InvalidateCache()
        {
            _cachedDefinitions = null;
        }

        private static IReadOnlyList<Type> FindProviderTypes()
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(GetTypesSafe)
                .Where(type =>
                    type != null &&
                    type.IsAbstract &&
                    type.IsSealed &&
                    type.GetCustomAttribute<GameplayTagProviderAttribute>() != null)
                .ToArray();
        }

        private static IEnumerable<Type> GetTypesSafe(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(type => type != null);
            }
        }
    }
}
