using System;

namespace Game.Core.Managers.Dependency
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true)]
    public sealed class InjectAttribute : Attribute
    {
    }
}
