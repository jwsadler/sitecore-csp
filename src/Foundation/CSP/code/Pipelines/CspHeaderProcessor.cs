using System;
using System.Web;
using Foundation.CSP.Models;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.HttpRequest;

namespace Foundation.CSP.Pipelines
{
    /// <summary>
    /// Pipeline processor that injects Content Security Policy headers into HTTP responses
    /// </summary>
    public class CspHeaderProcessor : HttpRequestProcessor
    {
        private readonly ICspSettingsProvider _settingsProvider;
        private const string CspHeaderName = "Content-Security-Policy";

        public CspHeaderProcessor()
        {
            _settingsProvider = new CspSettingsProvider();
        }

        // Constructor for dependency injection (optional)
        public CspHeaderProcessor(ICspSettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider ?? new CspSettingsProvider();
        }

        public override void Process(HttpRequestArgs args)
        {
            Assert.ArgumentNotNull(args, nameof(args));

            try
            {
                // Skip processing for certain requests
                if (ShouldSkipProcessing(args))
                {
                    return;
                }

                // Check if CMS-based CSP is enabled
                if (!_settingsProvider.IsCspEnabled())
                {
                    Log.Debug("CSP: CMS-based CSP is disabled. Web.config CSP header will be used if configured.", this);
                    return;
                }

                // Get CSP settings
                var cspSettings = _settingsProvider.GetCspSettings();
                if (cspSettings == null || !cspSettings.Enabled)
                {
                    Log.Debug("CSP: No valid CSP settings found.", this);
                    return;
                }

                // Build and inject CSP header
                var cspHeaderValue = cspSettings.BuildCspHeader();
                if (string.IsNullOrWhiteSpace(cspHeaderValue))
                {
                    Log.Warn("CSP: Generated CSP header is empty.", this);
                    return;
                }

                InjectCspHeader(args.Context, cspHeaderValue);
                Log.Debug($"CSP: Header injected successfully - {cspHeaderValue}", this);
            }
            catch (Exception ex)
            {
                Log.Error("CSP: Error processing CSP headers", ex, this);
                // Don't throw - we don't want to break the request pipeline
            }
        }

        /// <summary>
        /// Determines if CSP processing should be skipped for this request
        /// </summary>
        private bool ShouldSkipProcessing(HttpRequestArgs args)
        {
            if (args?.Context?.Response == null)
            {
                return true;
            }

            // Skip for Sitecore backend requests
            if (Sitecore.Context.Site != null && 
                (Sitecore.Context.Site.Name.Equals("shell", StringComparison.OrdinalIgnoreCase) ||
                 Sitecore.Context.Site.Name.Equals("admin", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            // Skip for static resources if needed
            var requestPath = args.Context.Request.Path.ToLowerInvariant();
            if (requestPath.EndsWith(".js") || 
                requestPath.EndsWith(".css") || 
                requestPath.EndsWith(".jpg") || 
                requestPath.EndsWith(".png") || 
                requestPath.EndsWith(".gif") ||
                requestPath.EndsWith(".svg") ||
                requestPath.EndsWith(".woff") ||
                requestPath.EndsWith(".woff2"))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Injects the CSP header into the HTTP response
        /// </summary>
        private void InjectCspHeader(HttpContext context, string cspHeaderValue)
        {
            if (context?.Response == null)
            {
                return;
            }

            try
            {
                // Remove any existing CSP headers first (to avoid duplicates)
                if (context.Response.Headers[CspHeaderName] != null)
                {
                    context.Response.Headers.Remove(CspHeaderName);
                    Log.Debug("CSP: Removed existing CSP header (likely from web.config)", this);
                }

                // Add the new CSP header
                context.Response.Headers.Add(CspHeaderName, cspHeaderValue);
            }
            catch (Exception ex)
            {
                Log.Error("CSP: Error injecting CSP header into response", ex, this);
            }
        }
    }
}

