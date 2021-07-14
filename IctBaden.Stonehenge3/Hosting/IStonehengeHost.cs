using Microsoft.Extensions.Logging;

namespace IctBaden.Stonehenge3.Hosting
{
    public interface IStonehengeHost
    {
        /// <summary>
        /// Gives the base URL the hosting service is using for the initial page.
        /// This is a browsable address.
        /// </summary>
        string BaseUrl { get; }

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

        /// <summary>
        /// Set the log level for all stonehenge logs.
        /// </summary>
        /// <param name="level">The new log level</param>
        void SetLogLevel(LogLevel level);

        /// <summary>
        /// Enable route (page) of all app sessions 
        /// </summary>
        /// <param name="route"></param>
        /// <param name="enabled"></param>
        void EnableRoute(string route, bool enabled);
    }
}
