using System;
using System.Net;
using System.Text.RegularExpressions;

namespace IctBaden.Stonehenge3.Vue.Test
{
    public class RedirectableWebClient : WebClient
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public string SessionId { get; set; }

        public string DownloadStringWithSession(string address)
        {
            if (SessionId == null)
            {
                DownloadString(address);
            }
            return DownloadString(address + $"?stonehenge-id={SessionId}");
        }
        
        public new string DownloadString(string address)
        {
            for (var redirect = 0; redirect < 10; redirect++)
            {
                var request = (HttpWebRequest)WebRequest.Create(address);
                request.AllowAutoRedirect = true;

                WebResponse response;
                try
                {
                    response = GetWebResponse(request);
                    if (response == null) return null;
                }
                catch (WebException ex)
                {
                    response = ex.Response;
                }

                var redirectUrl = response.Headers["Location"];
                if (redirectUrl == null)
                {
                    address = response.ResponseUri.ToString();
                }

                var match = new Regex("stonehenge-id=([a-f0-9A-F]+)", RegexOptions.RightToLeft)
                    .Match(address);
                if (match.Success)
                {
                    SessionId = match.Groups[1].Value;
                }
                
                response.Close();

                if (redirectUrl == null)
                    break;

                var newAddress = new Uri(request.RequestUri, redirectUrl).AbsoluteUri;
                if (newAddress == address)
                    break;

                address = newAddress;
            }

            return base.DownloadString(address);
        }
    }
}
