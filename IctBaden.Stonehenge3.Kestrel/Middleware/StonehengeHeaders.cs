using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace IctBaden.Stonehenge3.Kestrel.Middleware
{
    public class StonehengeHeaders
    {
        private readonly RequestDelegate _next;
        private static Dictionary<string, string> _headers;

        // ReSharper disable once UnusedMember.Global
        public StonehengeHeaders(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                if (_headers == null)
                    LoadHeaders();
                // ReSharper disable once PossibleNullReferenceException
                foreach (var header in _headers)
                {
                    context.Response.Headers.Add(header.Key, header.Value);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error handling default headers: " + ex.Message);
            }
            await _next.Invoke(context);
        }

        private void LoadHeaders()
        {
            _headers = new Dictionary<string, string>();

            var path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? ".";
            var headersFile = Path.Combine(path, "defaultheaders.txt");
            if (!File.Exists(headersFile)) return;

            Trace.TraceInformation("Adding default headers from: " + headersFile);
            var headers = File.ReadAllLines(headersFile);
            foreach (var header in headers)
            {
                if (string.IsNullOrEmpty(header)) continue;
                if (header.StartsWith("#")) continue;

                var colon = header.IndexOf(':');
                if (colon < 1) continue;
                var key = header.Substring(0, colon).Trim();
                var value = header.Substring(colon + 1).Trim();
                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                {
                    Trace.TraceInformation($"Add header: {key}: {value}");
                    _headers.Add(key, value);
                }
            }
        }
    }
}
