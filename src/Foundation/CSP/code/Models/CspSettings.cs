using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Foundation.CSP.Models
{
    /// <summary>
    /// Represents Content Security Policy settings
    /// </summary>
    public class CspSettings
    {
        public bool Enabled { get; set; }
        public string DefaultSrc { get; set; }
        public string ScriptSrc { get; set; }
        public string StyleSrc { get; set; }
        public string ImgSrc { get; set; }
        public string FontSrc { get; set; }
        public string ConnectSrc { get; set; }
        public string FrameSrc { get; set; }
        public string FrameAncestors { get; set; }
        public string ObjectSrc { get; set; }
        public string MediaSrc { get; set; }
        public string WorkerSrc { get; set; }
        public string ManifestSrc { get; set; }
        public string BaseUri { get; set; }
        public string FormAction { get; set; }
        public string ChildSrc { get; set; }
        public string UpgradeInsecureRequests { get; set; }
        public string BlockAllMixedContent { get; set; }

        /// <summary>
        /// Builds the complete CSP header value from individual directives
        /// </summary>
        /// <returns>The formatted CSP header value</returns>
        public string BuildCspHeader()
        {
            if (!Enabled)
            {
                return string.Empty;
            }

            var directives = new List<string>();

            AddDirective(directives, "default-src", DefaultSrc);
            AddDirective(directives, "script-src", ScriptSrc);
            AddDirective(directives, "style-src", StyleSrc);
            AddDirective(directives, "img-src", ImgSrc);
            AddDirective(directives, "font-src", FontSrc);
            AddDirective(directives, "connect-src", ConnectSrc);
            AddDirective(directives, "frame-src", FrameSrc);
            AddDirective(directives, "frame-ancestors", FrameAncestors);
            AddDirective(directives, "object-src", ObjectSrc);
            AddDirective(directives, "media-src", MediaSrc);
            AddDirective(directives, "worker-src", WorkerSrc);
            AddDirective(directives, "manifest-src", ManifestSrc);
            AddDirective(directives, "base-uri", BaseUri);
            AddDirective(directives, "form-action", FormAction);
            AddDirective(directives, "child-src", ChildSrc);

            // Special directives without values
            if (!string.IsNullOrWhiteSpace(UpgradeInsecureRequests) && 
                UpgradeInsecureRequests.Equals("1", StringComparison.OrdinalIgnoreCase))
            {
                directives.Add("upgrade-insecure-requests");
            }

            if (!string.IsNullOrWhiteSpace(BlockAllMixedContent) && 
                BlockAllMixedContent.Equals("1", StringComparison.OrdinalIgnoreCase))
            {
                directives.Add("block-all-mixed-content");
            }

            return string.Join("; ", directives.Where(d => !string.IsNullOrWhiteSpace(d)));
        }

        /// <summary>
        /// Adds a directive to the list if it has a value
        /// </summary>
        private void AddDirective(List<string> directives, string directiveName, string directiveValue)
        {
            if (!string.IsNullOrWhiteSpace(directiveValue))
            {
                var sanitizedValue = SanitizeDirectiveValue(directiveValue);
                directives.Add($"{directiveName} {sanitizedValue}");
            }
        }

        /// <summary>
        /// Sanitizes directive values by removing extra whitespace and ensuring proper formatting
        /// </summary>
        private string SanitizeDirectiveValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            // Remove multiple spaces and trim
            return string.Join(" ", value.Split(new[] { ' ', '\r', '\n', '\t' }, 
                StringSplitOptions.RemoveEmptyEntries));
        }
    }
}

