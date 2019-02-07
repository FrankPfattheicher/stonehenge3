namespace IctBaden.Stonehenge3.Hosting
{
    public interface IStonehengeHost
    {
        /// <summary>
        /// The applications title to use.
        /// </summary>
        string AppTitle { get; }

        /// <summary>
        /// Gives the base URL the hosting service is using for the initial page.
        /// </summary>
        string BaseUrl { get; }

        /// <summary>
        /// Options for hosting and serving.
        /// </summary>
        // ReSharper disable once UnusedMemberInSuper.Global
        StonehengeHostOptions Options { get; }
            
        /// <summary>
        /// Start hosting service.
        /// </summary>
        /// <param name="hostAddress">IP address to listen on or null to listen on all addresses.</param>
        /// <param name="hostPort">Port number to listen on or 0 for default (80 or 443 for SSL).</param>
        /// <returns>True if successfully started.</returns>
        bool Start(string hostAddress = null, int hostPort = 0);

        /// <summary>
        /// Terminate hosting service.
        /// </summary>
        void Terminate();
    }
}
