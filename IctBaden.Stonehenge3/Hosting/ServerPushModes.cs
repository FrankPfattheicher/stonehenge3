namespace IctBaden.Stonehenge3.Hosting
{
    public enum ServerPushModes
    {
        Automatic,
        /// <summary>
        /// (client pull)
        /// </summary>
        ShortPolling,
        /// <summary>
        /// (client pull)
        /// </summary>
        LongPolling,
        /// <summary>
        /// (server push)
        /// </summary>
        WebSockets,
        /// <summary>
        /// (server push)
        /// </summary>
        ServerSentEvents
    }
}