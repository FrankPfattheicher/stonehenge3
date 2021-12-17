using System;
using System.Net;
using static System.Net.WebRequest;

#pragma warning disable CS0618

namespace IctBaden.Stonehenge3.Test.Hosting
{
    public class RedirectableWebClient : WebClient
    {
        public new string DownloadString(string address)
        {
            for (var redirect = 0; redirect < 10; redirect++)
            {
                var request = (HttpWebRequest)Create(address);
                request.AllowAutoRedirect = true;

                var response = GetWebResponse(request);
                if (response == null) return null;

                var redirectUrl = response.Headers["Location"];
                if (redirectUrl == null)
                {
                    address = response.ResponseUri.ToString();
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
