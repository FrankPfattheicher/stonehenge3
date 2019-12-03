// ReSharper disable MemberCanBePrivate.Global

using System;
using System.IO;
using System.Reflection;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

namespace IctBaden.Stonehenge3.Hosting
{
    public class StonehengeHostOptions
    {
        /// <summary>
        /// Title to be shown in the Title bar.
        /// Default is the entry assembly name.
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Initial page to be activated.
        /// By default the first page (by sort index) is used. 
        /// </summary>
        public string StartPage { get; set; }

        /// <summary>
        /// Index page to be used as application frame.
        /// Without specified index.html is used.
        /// </summary>
        public string IndexPage { get; set; }
        
        /// <summary>
        /// Path to the file based content.
        /// </summary>
        public string AppFilesPath { get; set; }

        /// <summary>
        /// Specifies how session id is transported.
        /// </summary>
        public SessionIdModes SessionIdMode { get; set; } = SessionIdModes.Automatic;

        /// <summary>
        /// Method to use for server initiated data transfer to the client.
        /// </summary>
        public ServerPushModes ServerPushMode { get; set; } = ServerPushModes.Automatic;
        
        /// <summary>
        /// Interval for client site polling modes.
        /// [Milliseconds]
        /// Set to 0 to use system default.
        /// </summary>
        public int PollIntervalMs { get; set; }


        public StonehengeHostOptions()
        {
            Title = Assembly.GetEntryAssembly()?.GetName().Name;
            AppFilesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app");
        }
        
        
        /// <summary>
        /// Delay [ms] the client should wait for new poll.
        /// </summary>
        /// <returns></returns>
        public int GetPollDelayMs()
        {
            if (ServerPushMode == ServerPushModes.LongPolling)
            {
                return 100;
            }
            if (ServerPushMode == ServerPushModes.ShortPolling)
            {
                if(PollIntervalMs > 1) return (PollIntervalMs * 1000) + 100;
            }
            return 5000;
        }
        /// <summary>
        /// Timeout the server max waits to respond to event query
        /// if there is no event.
        /// </summary>
        /// <returns></returns>
        public int GetEventTimeoutMs()
        {
            if(ServerPushMode == ServerPushModes.LongPolling)
            {
                if(PollIntervalMs > 1) return PollIntervalMs + 100;
                return 10000;
            }
            return 100;
        }
        public bool AllowCookies => SessionIdMode != SessionIdModes.UrlParameterOnly;
        public bool AddUrlSessionParameter => SessionIdMode != SessionIdModes.CookiesOnly;
    }
}
