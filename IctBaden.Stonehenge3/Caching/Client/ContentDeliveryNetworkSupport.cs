using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace IctBaden.Stonehenge3.Caching.Client
{
    // ReSharper disable once UnusedType.Global
    public static class ContentDeliveryNetworkSupport
    {
        private const string CdnConfigurationFileName = "CDN.cfg";
        private static Dictionary<string, string> _cdnLookup;

        public static Dictionary<string, string> CdnLookup
        {
            get
            {
                if (_cdnLookup != null)
                    return _cdnLookup;

                if (File.Exists(CdnConfigurationFileName))
                {
                    _cdnLookup = (from line in File.ReadAllLines(CdnConfigurationFileName)
                                 where !line.StartsWith("#")
                                 let elements = line.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries)
                                 where elements.Length == 2
                                 select elements).ToDictionary(e => e[0], e => e[1]);
                }
                else
                {
                    _cdnLookup = new Dictionary<string, string>();
                }

                return _cdnLookup;
            }
            set => _cdnLookup = value;
        }

        public static string ResolveHostsHtml(string page, bool isSecureConnection)
        {
            if (!File.Exists(CdnConfigurationFileName))
                return page;

            var protocol = isSecureConnection ? "https://" : "http://";
            var script = new Regex("(?<a><script.*src=\"(?<b>(?<c>.*\\.js))\".*)|(?<a><link.*href=\"(?<b>(?<c>.*\\.css))\".*)", RegexOptions.Compiled);

            var resultLines = from line in page.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                              let isScriptSource = script.Match(line)
                              let source = isScriptSource.Groups["c"].Value.Split('/').Last()
                              select (isScriptSource.Success && CdnLookup.ContainsKey(source)) ?
                                     isScriptSource.Groups["a"].Value.Replace(isScriptSource.Groups["b"].Value, CdnLookup[source].Replace("http://", protocol)) : line;

            return string.Join(Environment.NewLine, resultLines);
        }

        public static string ResolveHostsJs(string page, bool isSecureConnection)
        {
            if (!File.Exists(CdnConfigurationFileName))
                return page;

            var protocol = isSecureConnection ? "https://" : "http://";
            var script = new Regex("(?<map>'(?<id>.+)' *: *'(?<path>.*)'.*)", RegexOptions.Compiled);

            var resultLines = from line in page.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                              let isMapPath = script.Match(line)
                              let source = isMapPath.Groups["path"].Value.Split('/').Last() + ".js"
                              select (isMapPath.Success && CdnLookup.ContainsKey(source)) ?
                                     isMapPath.Groups["map"].Value.Replace(isMapPath.Groups["path"].Value, CdnLookup[source].Replace("http://", protocol)).Replace(".js'", "'") : line;

            return string.Join(Environment.NewLine, resultLines);
        }
    }
}