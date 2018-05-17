namespace IctBaden.Stonehenge3.Hosting
{
    public interface IStonehengeHost
    {
        /// <summary>
        /// The applicatioons title to use.
        /// </summary>
        string AppTitle { get; }

        /// <summary>
        /// Gives the base URL the hosting service is using for the initial page.
        /// </summary>
        string BaseUrl { get; }

        /// <summary>
        /// Disables redirection with session id as URL parameter.
        /// Requires all clients are able to use cookies.
        /// </summary>
        bool DisableSessionIdUrlParameter { get; set; }

        /// <summary>
        /// Start hosting service.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="useSsl">Use secure sockets for hosting.</param>
        /// <param name="hostAddress">IP address to listen on or null to listen on all adresses.</param>
        /// <param name="hostPort">Port number to listen on or 0 for default (80 or 443 for SSL).</param>
        /// <returns>True if successfully started.</returns>
        bool Start(string title, bool useSsl = false, string hostAddress = null, int hostPort = 0);

        /// <summary>
        /// Terminate hosting service.
        /// </summary>
        void Terminate();
    }
}
