using System;
using System.Web;
using Foundation.CSP.Models;
using Foundation.CSP.Services;
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
        private readonly INonceService _nonceService;
        private const string CspHeaderName = "Content-Security-Policy";

        public CspHeaderProcessor()
        {
            _settingsProvider = new CspSettingsProvider();
            _nonceService = new NonceService();
        }

        // Constructor for dependency injection (optional)
        public CspHeaderProcessor(ICspSettingsProvider settingsProvider, INonceService nonceService = null)
        {
            _settingsProvider = settingsProvider ?? new CspSettingsProvider();
            _nonceService = nonceService ?? new NonceService();
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

                // Build and inject CSP header with nonce support
                var cspHeaderValue = cspSettings.BuildCspHeader(_nonceService);
                if (string.IsNullOrWhiteSpace(cspHeaderValue))
                {
                    Log.Warn("CSP: Generated CSP header is empty.", this);
                    return;
                }

                InjectCspHeader(HttpContext.Current, cspHeaderValue);
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
            var httpContext = HttpContext.Current;
            if (httpContext?.Response == null)
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

            // Skip when in Experience Editor or Content Editor mode (if configured)
            var skipDuringEditing = Sitecore.Configuration.Settings.GetBoolSetting("CSP.SkipDuringEditing", true);
            if (skipDuringEditing && IsContentEditingMode(httpContext))
            {
                return true;
            }

            // Skip for static resources if needed
            var requestPath = httpContext.Request.Path.ToLowerInvariant();
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
        /// Determines if the current request is in content editing mode
        /// </summary>
        private bool IsContentEditingMode(HttpContext httpContext)
        {
            if (httpContext?.Request == null)
            {
                return false;
            }

            // Check for Experience Editor mode
            if (Sitecore.Context.PageMode.IsExperienceEditor || 
                Sitecore.Context.PageMode.IsExperienceEditorEditing)
            {
                return true;
            }

            // Check for Preview mode
            if (Sitecore.Context.PageMode.IsPreview)
            {
                return true;
            }

            // Check for Debug mode
            if (Sitecore.Context.PageMode.IsDebugging)
            {
                return true;
            }

            // Check query string parameters that indicate editing mode
            var queryString = httpContext.Request.QueryString;
            if (queryString["sc_mode"] != null || 
                queryString["sc_edit"] != null ||
                queryString["sc_debug"] != null ||
                queryString["sc_trace"] != null)
            {
                return true;
            }

            // Check for Experience Editor specific query parameters
            if (queryString["sc_ee"] != null || 
                queryString["sc_itemid"] != null ||
                queryString["sc_lang"] != null ||
                queryString["sc_site"] != null)
            {
                return true;
            }

            // Check request path for editing-related URLs
            var requestPath = httpContext.Request.Path.ToLowerInvariant();
            if (requestPath.Contains("/sitecore/") ||
                requestPath.Contains("/-/speak/") ||
                requestPath.Contains("/~/media/") ||
                requestPath.Contains("/-/media/"))
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
