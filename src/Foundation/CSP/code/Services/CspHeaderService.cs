using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Foundation.CSP.Models;
using Foundation.CSP.Services;
using RRA.Foundation.DI;

namespace Foundation.CSP.Services
{
    /// <summary>
    /// Interface for CSP header building service
    /// </summary>
    public interface ICspHeaderService
    {
        /// <summary>
        /// Builds the complete CSP header value from CSP settings
        /// </summary>
        /// <param name="settings">The CSP settings</param>
        /// <param name="nonceService">Optional nonce service for generating nonce tokens</param>
        /// <returns>The formatted CSP header value</returns>
        string BuildCspHeader(ICSPSettings settings, INonceService nonceService = null);

        /// <summary>
        /// Gets the Google Tag Manager script with nonce support
        /// </summary>
        /// <param name="settings">The CSP settings</param>
        /// <param name="currentNonce">The current nonce value</param>
        /// <returns>The Google Tag Manager script with nonce attribute</returns>
        string GetGoogleTagManagerScript(ICSPSettings settings, string currentNonce = null);

        /// <summary>
        /// Gets the Google Tag Manager noscript fallback
        /// </summary>
        /// <param name="settings">The CSP settings</param>
        /// <returns>The Google Tag Manager noscript tag</returns>
        string GetGoogleTagManagerNoScript(ICSPSettings settings);
    }

    /// <summary>
    /// Service for building CSP headers and related functionality
    /// </summary>
    [Service(typeof(ICspHeaderService), Lifetime = Lifetime.Scoped)]
    public class CspHeaderService : ICspHeaderService
    {
        /// <summary>
        /// Builds the complete CSP header value from individual directives
        /// </summary>
        /// <param name="settings">The CSP settings</param>
        /// <param name="nonceService">Optional nonce service for generating nonce tokens</param>
        /// <returns>The formatted CSP header value</returns>
        public string BuildCspHeader(ICSPSettings settings, INonceService nonceService = null)
        {
            if (settings == null || !settings.Enabled)
            {
                return string.Empty;
            }

            var directives = new List<string>();

            // Get current nonce if nonce is enabled
            string currentNonce = null;
            if (settings.EnableNonce && nonceService != null)
            {
                currentNonce = nonceService.GetCurrentNonce();
            }

            AddDirective(directives, "default-src", settings.DefaultSrc);
            AddDirective(directives, "script-src", BuildScriptSrcWithNonce(settings, currentNonce));
            AddDirective(directives, "style-src", settings.StyleSrc);
            AddDirective(directives, "img-src", BuildImgSrcWithGoogleAnalytics(settings));
            AddDirective(directives, "font-src", settings.FontSrc);
            AddDirective(directives, "connect-src", BuildConnectSrcWithGoogleAnalytics(settings));
            AddDirective(directives, "frame-src", settings.FrameSrc);
            AddDirective(directives, "frame-ancestors", settings.FrameAncestors);
            AddDirective(directives, "object-src", settings.ObjectSrc);
            AddDirective(directives, "media-src", settings.MediaSrc);
            AddDirective(directives, "worker-src", settings.WorkerSrc);
            AddDirective(directives, "manifest-src", settings.ManifestSrc);
            AddDirective(directives, "base-uri", settings.BaseUri);
            AddDirective(directives, "form-action", settings.FormAction);
            AddDirective(directives, "child-src", settings.ChildSrc);

            // Handle boolean directives
            if (!string.IsNullOrWhiteSpace(settings.UpgradeInsecureRequests) && 
                (settings.UpgradeInsecureRequests.Equals("1") || settings.UpgradeInsecureRequests.Equals("true", StringComparison.OrdinalIgnoreCase)))
            {
                directives.Add("upgrade-insecure-requests");
            }

            if (settings.BlockAllMixedContent)
            {
                directives.Add("block-all-mixed-content");
            }

            AddDirective(directives, "report-uri", settings.ReportUri);

            return string.Join("; ", directives.Where(d => !string.IsNullOrWhiteSpace(d)));
        }

        /// <summary>
        /// Gets the Google Tag Manager script with nonce support
        /// </summary>
        /// <param name="settings">The CSP settings</param>
        /// <param name="currentNonce">The current nonce value</param>
        /// <returns>The Google Tag Manager script with nonce attribute</returns>
        public string GetGoogleTagManagerScript(ICSPSettings settings, string currentNonce = null)
        {
            if (settings == null || !settings.EnableGoogleAnalytics || string.IsNullOrWhiteSpace(settings.GoogleTagManagerID))
            {
                return string.Empty;
            }

            var nonceAttribute = settings.EnableNonce && !string.IsNullOrWhiteSpace(currentNonce) 
                ? $" nonce=\"{currentNonce}\"" 
                : string.Empty;

            return $@"<script{nonceAttribute}>
(function(w,d,s,l,i){{w[l]=w[l]||[];w[l].push({{'gtm.start':
new Date().getTime(),event:'gtm.js'}});var f=d.getElementsByTagName(s)[0],
j=d.createElement(s),dl=l!='dataLayer'?'&l='+l:'';j.async=true;j.src=
'https://www.googletagmanager.com/gtm.js?id='+i+dl;var n=d.querySelector('[nonce]');
n&&j.setAttribute('nonce',n.nonce||n.getAttribute('nonce'));f.parentNode.insertBefore(j,f);
}})(window,document,'script','dataLayer','{settings.GoogleTagManagerID}');
</script>";
        }

        /// <summary>
        /// Gets the Google Tag Manager noscript fallback
        /// </summary>
        /// <param name="settings">The CSP settings</param>
        /// <returns>The Google Tag Manager noscript tag</returns>
        public string GetGoogleTagManagerNoScript(ICSPSettings settings)
        {
            if (settings == null || !settings.EnableGoogleAnalytics || string.IsNullOrWhiteSpace(settings.GoogleTagManagerID))
            {
                return string.Empty;
            }

            return $@"<noscript><iframe src=""https://www.googletagmanager.com/ns.html?id={settings.GoogleTagManagerID}""
height=""0"" width=""0"" style=""display:none;visibility:hidden""></iframe></noscript>";
        }

        /// <summary>
        /// Adds a directive to the list if the value is not empty
        /// </summary>
        private void AddDirective(List<string> directives, string directiveName, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                directives.Add($"{directiveName} {value.Trim()}");
            }
        }

        /// <summary>
        /// Builds script-src directive with nonce support
        /// </summary>
        private string BuildScriptSrcWithNonce(ICSPSettings settings, string currentNonce)
        {
            var scriptSrcParts = new List<string>();

            // Add base script-src values
            if (!string.IsNullOrWhiteSpace(settings.ScriptSrc))
            {
                scriptSrcParts.AddRange(settings.ScriptSrc.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            }

            // Add Google Analytics domains if enabled
            if (settings.EnableGoogleAnalytics)
            {
                scriptSrcParts.Add("https://*.googletagmanager.com");
                scriptSrcParts.Add("https://www.google-analytics.com");
                
                if (settings.EnableGoogleSignals)
                {
                    scriptSrcParts.Add("https://www.googleadservices.com");
                    scriptSrcParts.Add("https://googleads.g.doubleclick.net");
                }
            }

            // Add nonce if enabled
            if (settings.EnableNonce && !string.IsNullOrWhiteSpace(currentNonce))
            {
                scriptSrcParts.Add($"'nonce-{currentNonce}'");
            }

            return string.Join(" ", scriptSrcParts.Distinct());
        }

        /// <summary>
        /// Builds img-src directive with Google Analytics support
        /// </summary>
        private string BuildImgSrcWithGoogleAnalytics(ICSPSettings settings)
        {
            var imgSrcParts = new List<string>();

            // Add base img-src values
            if (!string.IsNullOrWhiteSpace(settings.ImgSrc))
            {
                imgSrcParts.AddRange(settings.ImgSrc.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            }

            // Add Google Analytics domains if enabled
            if (settings.EnableGoogleAnalytics)
            {
                imgSrcParts.Add("https://*.googletagmanager.com");
                imgSrcParts.Add("https://www.google-analytics.com");
                
                if (settings.EnableGoogleSignals)
                {
                    imgSrcParts.Add("https://www.googleadservices.com");
                    imgSrcParts.Add("https://googleads.g.doubleclick.net");
                }
            }

            return string.Join(" ", imgSrcParts.Distinct());
        }

        /// <summary>
        /// Builds connect-src directive with Google Analytics support
        /// </summary>
        private string BuildConnectSrcWithGoogleAnalytics(ICSPSettings settings)
        {
            var connectSrcParts = new List<string>();

            // Add base connect-src values
            if (!string.IsNullOrWhiteSpace(settings.ConnectSrc))
            {
                connectSrcParts.AddRange(settings.ConnectSrc.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            }

            // Add Google Analytics domains if enabled
            if (settings.EnableGoogleAnalytics)
            {
                connectSrcParts.Add("https://*.googletagmanager.com");
                connectSrcParts.Add("https://www.google-analytics.com");
                
                if (settings.EnableGoogleSignals)
                {
                    connectSrcParts.Add("https://www.googleadservices.com");
                    connectSrcParts.Add("https://googleads.g.doubleclick.net");
                }
            }

            return string.Join(" ", connectSrcParts.Distinct());
        }
    }
}
