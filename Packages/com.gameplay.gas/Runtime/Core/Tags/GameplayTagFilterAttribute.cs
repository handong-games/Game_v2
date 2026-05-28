using System;
using UnityEngine;

namespace Gameplay.GAS
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class GameplayTagFilterAttribute : PropertyAttribute
    {
        public GameplayTagFilterAttribute(string rootPath)
        {
            RootPath = rootPath;
        }

        public string RootPath { get; }
    }
}
