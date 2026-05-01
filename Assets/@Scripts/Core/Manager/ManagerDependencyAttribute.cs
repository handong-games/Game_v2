using System;

namespace Game.Core.Managers
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class ManagerDependencyAttribute : Attribute
    {
        public ManagerDependencyAttribute(Type managerType)
        {
            ManagerType = managerType;
        }

        public Type ManagerType { get; }
    }
}
