using System;
using Domains.Character;
using Game.Core.Managers.Dependency;

namespace Domains.Run
{
    [Dependency]
    public sealed class RunService : IDisposable
    {
        public RunState CurrentRun { get; private set; }

        public void StartNewRun(CharacterState character)
        {
            if (character == null || !character.IsUnlocked)
                throw new InvalidOperationException("Cannot start a run with an invalid character.");

            CurrentRun = new RunState(character);
        }

        public void Dispose()
        {
            CurrentRun = null;
        }
    }
}
