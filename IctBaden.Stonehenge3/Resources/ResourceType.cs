using System.Collections.Generic;
using System.Linq;

// ReSharper disable MemberCanBePrivate.Global

namespace IctBaden.Stonehenge3.Resources
{
    public class ResourceType
    {
        public string Extension { get; }
        public string ContentType { get; }

        public bool IsBinary { get; }

        public ResourceType(string extension, string contentType, bool isBinary)
        {
            Extension = extension;
            ContentType = contentType;
            IsBinary = isBinary;
        }

        public static readonly ResourceType Text = new("txt", "text/plain", false);
        public static readonly ResourceType TextUtf8 = new("txt", "text/plain; charset=utf-8", false);
        public static readonly ResourceType Htm = new("htm", "text/html", false);
        public static readonly ResourceType Html = new("html", "text/html", false);
        public static readonly ResourceType Css = new("css", "text/css", false);
        public static readonly ResourceType Js = new("js", "text/javascript", false);
        public static readonly ResourceType Calendar = new("ics", "text/calendar", false);
        public static readonly ResourceType Csv = new("csv", "text/csv", false);
        public static readonly ResourceType Pdf = new("pdf", "application/pdf", true);
        public static readonly ResourceType Json = new("json", "application/json; charset=utf-8", false);

        public static readonly ResourceType Png = new("png", "image/png", true);
        public static readonly ResourceType Gif = new("gif", "image/gif", true);
        public static readonly ResourceType Jpg = new("jpg", "image/jpeg", true);
        public static readonly ResourceType Jpeg = new("jpeg", "image/jpeg", true);
        public static readonly ResourceType Wav = new("wav", "audio/x-wav", true);
        public static readonly ResourceType Ico = new("ico", "image/x-icon", true);
        public static readonly ResourceType Svg = new("svg", "image/svg+xml", false);

        public static readonly List<ResourceType> KnownTypes = new()
        {
            Text, TextUtf8,
            Htm, Html,
            Css,
            Js,
            Calendar,
            Csv,
            Pdf,
            Json,
            Png, Gif, Jpg, Jpeg,
            Wav,
            Ico,
            Svg
        };

        public static ResourceType GetByExtension(string extension)
        {
            extension = extension.Replace(".", "").ToLower();
            return KnownTypes.FirstOrDefault(rt => rt.Extension == extension) ??
                   new ResourceType(extension, "application/octet-stream", true);
        }
        
    }
}