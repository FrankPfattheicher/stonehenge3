using System;
using System.Net;

namespace IctBaden.Stonehenge3.Vue.Test
{
    public class RedirectableWebClient : WebClient
    {
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

                var redirUrl = response.Headers["Location"];
                if (redirUrl == null)
                {
                    address = response.ResponseUri.ToString();
                }

                response.Close();

                if (redirUrl == null)
                    break;

                var newAddress = new Uri(request.RequestUri, redirUrl).AbsoluteUri;
                if (newAddress == address)
                    break;

                address = newAddress;
            }

            return base.DownloadString(address);
        }
    }
}
