using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;

// ReSharper disable ConvertToUsingDeclaration

namespace IctBaden.Stonehenge3.Vue.Test
{
    public class RedirectableHttpClient : HttpClient
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public string SessionId { get; set; }

        public string DownloadStringWithSession(string address)
        {
            if (SessionId == null)
            {
                DownloadString(address);
            }

            var url = new UriBuilder(address);
            var query = HttpUtility.ParseQueryString(url.Query);
            query["stonehenge-id"] = SessionId;
            url.Query = query.ToString();
            return DownloadString(url.ToString());
        }

        public string DownloadString(string address)
        {
            for (var redirect = 0; redirect < 10; redirect++)
            {
                var response = GetAsync(address).Result;
                if (response == null) return null;

                var redirectUrl = response.Headers.Location;
                if (redirectUrl == null)
                {
                    address = response.RequestMessage.RequestUri.ToString();
                }

                var match = new Regex("stonehenge-id=([a-f0-9A-F]+)", RegexOptions.RightToLeft)
                    .Match(address);
                if (match.Success)
                {
                    SessionId = match.Groups[1].Value;
                }

                var body = response.Content.ReadAsStringAsync().Result;
                response.Dispose();

                if (redirectUrl == null)
                {
                    return body;
                }

                var newAddress = new Uri(response.RequestMessage.RequestUri, redirectUrl).AbsoluteUri;
                if (newAddress == address)
                    break;

                address = newAddress;
            }

            return null;
        }
    }
}