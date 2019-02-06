namespace IctBaden.Stonehenge3.Hosting
{
    public class StonehengeHostOptions
    {
        /// <summary>
        /// Specifies how session id is transported
        /// </summary>
        public SessionIdModes SessionIdMode { get; set; } = SessionIdModes.Automatic;


        public bool AllowCookies => SessionIdMode != SessionIdModes.UrlParameterOnly;
        public bool AddUrlSessionParameter => SessionIdMode != SessionIdModes.CookiesOnly;
    }
}