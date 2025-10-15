using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Foundation.CSP.Services;

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

        // Google Analytics specific settings
        public bool EnableGoogleAnalytics { get; set; }
        public bool EnableGoogleSignals { get; set; }
        public string GoogleTagManagerId { get; set; }
        
        // Nonce support
        public bool EnableNonce { get; set; }
        public string CurrentNonce { get; set; }

        /// <summary>
        /// Builds the complete CSP header value from individual directives
        /// </summary>
        /// <param name="nonceService">Optional nonce service for generating nonce tokens</param>
        /// <returns>The formatted CSP header value</returns>
        public string BuildCspHeader(INonceService nonceService = null)
        {
            if (!Enabled)
            {
                return string.Empty;
            }

            var directives = new List<string>();

            // Get current nonce if nonce is enabled
            if (EnableNonce && nonceService != null)
            {
                CurrentNonce = nonceService.GetCurrentNonce();
            }

            AddDirective(directives, "default-src", DefaultSrc);
            AddDirective(directives, "script-src", BuildScriptSrcWithNonce());
            AddDirective(directives, "style-src", StyleSrc);
            AddDirective(directives, "img-src", BuildImgSrcWithGoogleAnalytics());
            AddDirective(directives, "font-src", FontSrc);
            AddDirective(directives, "connect-src", BuildConnectSrcWithGoogleAnalytics());
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
        /// Builds script-src directive with nonce support
        /// </summary>
        private string BuildScriptSrcWithNonce()
        {
            var scriptSrcParts = new List<string>();

            // Add existing script-src values
            if (!string.IsNullOrWhiteSpace(ScriptSrc))
            {
                scriptSrcParts.Add(ScriptSrc);
            }

            // Add nonce if enabled
            if (EnableNonce && !string.IsNullOrWhiteSpace(CurrentNonce))
            {
                scriptSrcParts.Add($"'nonce-{CurrentNonce}'");
            }

            // Add Google Analytics domains if enabled
            if (EnableGoogleAnalytics)
            {
                scriptSrcParts.Add("https://*.googletagmanager.com");
            }

            return string.Join(" ", scriptSrcParts.Where(p => !string.IsNullOrWhiteSpace(p)));
        }

        /// <summary>
        /// Builds img-src directive with Google Analytics support
        /// </summary>
        private string BuildImgSrcWithGoogleAnalytics()
        {
            var imgSrcParts = new List<string>();

            // Add existing img-src values
            if (!string.IsNullOrWhiteSpace(ImgSrc))
            {
                imgSrcParts.Add(ImgSrc);
            }

            // Add Google Analytics domains if enabled
            if (EnableGoogleAnalytics)
            {
                imgSrcParts.Add("https://*.google-analytics.com");
                imgSrcParts.Add("https://*.googletagmanager.com");

                // Add Google Signals domains if enabled
                if (EnableGoogleSignals)
                {
                    imgSrcParts.Add("https://*.g.doubleclick.net");
                    imgSrcParts.Add("https://*.google.com");
                    imgSrcParts.Add("https://*.google.");
                }
            }

            return string.Join(" ", imgSrcParts.Where(p => !string.IsNullOrWhiteSpace(p)));
        }

        /// <summary>
        /// Builds connect-src directive with Google Analytics support
        /// </summary>
        private string BuildConnectSrcWithGoogleAnalytics()
        {
            var connectSrcParts = new List<string>();

            // Add existing connect-src values
            if (!string.IsNullOrWhiteSpace(ConnectSrc))
            {
                connectSrcParts.Add(ConnectSrc);
            }

            // Add Google Analytics domains if enabled
            if (EnableGoogleAnalytics)
            {
                connectSrcParts.Add("https://*.google-analytics.com");
                connectSrcParts.Add("https://*.analytics.google.com");
                connectSrcParts.Add("https://*.googletagmanager.com");

                // Add Google Signals domains if enabled
                if (EnableGoogleSignals)
                {
                    connectSrcParts.Add("https://*.g.doubleclick.net");
                    connectSrcParts.Add("https://*.google.com");
                    connectSrcParts.Add("https://*.google.");
                    connectSrcParts.Add("https://pagead2.googlesyndication.com");
                }
            }

            return string.Join(" ", connectSrcParts.Where(p => !string.IsNullOrWhiteSpace(p)));
        }

        /// <summary>
        /// Gets the Google Tag Manager script with nonce support
        /// </summary>
        /// <returns>The Google Tag Manager script with nonce attribute</returns>
        public string GetGoogleTagManagerScript()
        {
            if (!EnableGoogleAnalytics || string.IsNullOrWhiteSpace(GoogleTagManagerId))
            {
                return string.Empty;
            }

            var nonceAttribute = EnableNonce && !string.IsNullOrWhiteSpace(CurrentNonce) 
                ? $" nonce=\"{CurrentNonce}\"" 
                : string.Empty;

            return $@"<script{nonceAttribute}>
(function(w,d,s,l,i){{w[l]=w[l]||[];w[l].push({{'gtm.start':
new Date().getTime(),event:'gtm.js'}});var f=d.getElementsByTagName(s)[0],
j=d.createElement(s),dl=l!='dataLayer'?'&l='+l:'';j.async=true;j.src=
'https://www.googletagmanager.com/gtm.js?id='+i+dl;var n=d.querySelector('[nonce]');
n&&j.setAttribute('nonce',n.nonce||n.getAttribute('nonce'));f.parentNode.insertBefore(j,f);
}})(window,document,'script','dataLayer','{GoogleTagManagerId}');
</script>";
        }

        /// <summary>
        /// Gets the Google Tag Manager noscript fallback
        /// </summary>
        /// <returns>The Google Tag Manager noscript tag</returns>
        public string GetGoogleTagManagerNoScript()
        {
            if (!EnableGoogleAnalytics || string.IsNullOrWhiteSpace(GoogleTagManagerId))
            {
                return string.Empty;
            }

            return $@"<noscript><iframe src=""https://www.googletagmanager.com/ns.html?id={GoogleTagManagerId}""
height=""0"" width=""0"" style=""display:none;visibility:hidden""></iframe></noscript>";
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
