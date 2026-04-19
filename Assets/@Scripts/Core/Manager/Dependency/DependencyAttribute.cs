using System;

namespace Game.Core.Managers.Dependency
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class DependencyAttribute : Attribute
    {
        public string SceneName { get; }
        public bool IsGlobal => string.IsNullOrWhiteSpace(SceneName);

        public DependencyAttribute()
        {
        }

        public DependencyAttribute(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
                throw new ArgumentException("Scene name is invalid.", nameof(sceneName));

            SceneName = sceneName;
        }
    }
}
