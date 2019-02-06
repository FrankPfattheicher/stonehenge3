namespace IctBaden.Stonehenge3.Hosting
{
    public enum SessionIdModes
    {
        /// <summary>
        /// Use Cookies or URL parameters
        /// </summary>
        Automatic,
        
        /// <summary>
        /// Disables redirection with session id as URL parameter.
        /// Requires all clients are able to use cookies.
        /// </summary>
        CookiesOnly,

        /// <summary>
        /// Do NOT use cookies at all
        /// </summary>
        UrlParameterOnly
    }
}