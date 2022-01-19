// ReSharper disable MemberCanBePrivate.Global

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
        /// [Seconds]
        /// Set to 0 to use system default.
        /// </summary>
        public int PollIntervalSec { get; set; }

        /// <summary>
        /// Forth NTLM authentication using HttpSys.
        /// (Windows host only)
        /// </summary>
        public bool UseNtlmAuthentication { get; set; } = false;
        
        /// <summary>
        /// Internally check basic Authentication.
        /// Place .htpasswd file in application directory.
        /// Encoding apache specific salted MD5 (insecure but common).
        /// htpasswd -nbm myName myPassword
        /// </summary>
        public bool UseBasicAuth { get; set; } = false;

        /// <summary>
        /// Path of the pfx certificate to be used with Kestrel.
        /// (not used with HttpSys, you need to "netsh http add sslcert ..." for the the p12 certificate in that case)
        /// On Windows it is better to use IIS as reverse proxy.
        /// </summary>
        public string SslCertificatePath { get; set; }
        /// <summary>
        /// Password of the pfx certificate to be used with Kestrel.
        /// (not used with HttpSys)
        /// </summary>
        public string SslCertificatePassword { get; set; }

        /// <summary>
        /// Host is using the following headers to disable clients
        /// to cache any content.
        ///     Cache-Control: no-cache, no-store, must-revalidate, proxy-revalidate
        ///     Pragma: no-cache
        ///     Expires: 0 
        /// </summary>
        public bool DisableClientCache { get; set; } = false;
        
        /// <summary>
        /// Enable firing WindowResized AppCommand  
        /// </summary>
        public bool HandleWindowResized { get; set; } = false;
        
        public StonehengeHostOptions()
        {
            Title = Assembly.GetEntryAssembly()?.GetName().Name;
            AppFilesPath = Path.Combine(StonehengeApplication.BaseDirectory, "app");
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
                if(PollIntervalSec > 1) return (PollIntervalSec * 1000) + 100;
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
                if(PollIntervalSec > 1) return (PollIntervalSec * 1000) + 100;
                return 10000;
            }
            return 100;
        }
        
        public bool AllowCookies => SessionIdMode != SessionIdModes.UrlParameterOnly;
        public bool AddUrlSessionParameter => SessionIdMode != SessionIdModes.CookiesOnly;

    }
}
