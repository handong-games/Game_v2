namespace Core.Message
{
    public enum EAwaitMode
    {
        /// <summary>Invoke 시 이 핸들러 완료까지 대기.</summary>
        Blocking,

        /// <summary>Invoke 시 이 핸들러를 기다리지 않음 (백그라운드).</summary>
        NonBlocking
    }
}