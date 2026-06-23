namespace Domains.Adventure
{
    public sealed class AdventureRunState
    {
        public AdventureRun CurrentRun { get; private set; }

        public void SetCurrent(AdventureRun run)
        {
            CurrentRun = run;
        }

        public void Clear()
        {
            CurrentRun = null;
        }
    }
}
