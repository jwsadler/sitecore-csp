using System;
using System.Collections.Generic;
using Foundation.CSP.Services;
using Sitecore.Diagnostics;
using Sitecore.Mvc.Presentation;

namespace Foundation.CSP.Models
{
    /// <summary>
    /// Base rendering model class that provides nonce token access for CSP-compliant script injection
    /// Inherit from this class to automatically get nonce support in your rendering models
    /// </summary>
    public abstract class NonceAwareRenderingModel : RenderingModel
    {
        private readonly INonceService _nonceService;
        private readonly IScriptInjectionService _scriptInjectionService;

        protected NonceAwareRenderingModel()
        {
            _nonceService = new NonceService();
            _scriptInjectionService = new ScriptInjectionService(_nonceService);
        }

        // Constructor for dependency injection
        protected NonceAwareRenderingModel(INonceService nonceService, IScriptInjectionService scriptInjectionService)
        {
            _nonceService = nonceService ?? new NonceService();
            _scriptInjectionService = scriptInjectionService ?? new ScriptInjectionService(_nonceService);
        }

        /// <summary>
        /// Gets the current request's nonce token
        /// </summary>
        public string CurrentNonce
        {
            get
            {
                try
                {
                    return _nonceService.GetCurrentNonce();
                }
                catch (Exception ex)
                {
                    Log.Error("NonceAwareRenderingModel: Failed to get current nonce", ex, this);
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Gets the script injection service for nonce-aware script creation
        /// </summary>
        protected IScriptInjectionService ScriptInjectionService => _scriptInjectionService;

        /// <summary>
        /// Creates a script tag with nonce attribute for inline JavaScript
        /// </summary>
        /// <param name="scriptContent">The JavaScript content</param>
        /// <param name="additionalAttributes">Additional attributes for the script tag</param>
        /// <returns>Complete script tag with nonce</returns>
        public string CreateInlineScript(string scriptContent, Dictionary<string, string> additionalAttributes = null)
        {
            return _scriptInjectionService.CreateNonceScript(scriptContent, false, additionalAttributes);
        }

        /// <summary>
        /// Creates a script tag with nonce attribute for external JavaScript
        /// </summary>
        /// <param name="scriptSrc">The source URL of the external script</param>
        /// <param name="additionalAttributes">Additional attributes for the script tag</param>
        /// <returns>Complete script tag with nonce</returns>
        public string CreateExternalScript(string scriptSrc, Dictionary<string, string> additionalAttributes = null)
        {
            return _scriptInjectionService.CreateNonceScript(scriptSrc, true, additionalAttributes);
        }

        /// <summary>
        /// Gets the Google Tag Manager script with nonce support (if Google Analytics is enabled)
        /// </summary>
        /// <returns>Google Tag Manager script with nonce or empty string</returns>
        public string GetGoogleTagManagerScript()
        {
            try
            {
                var settingsProvider = new CspSettingsProvider();
                var cspSettings = settingsProvider.GetCspSettings();
                
                if (cspSettings?.EnableGoogleAnalytics == true)
                {
                    // Ensure the current nonce is set
                    cspSettings.CurrentNonce = CurrentNonce;
                    return cspSettings.GetGoogleTagManagerScript();
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                Log.Error("NonceAwareRenderingModel: Failed to get Google Tag Manager script", ex, this);
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the Google Tag Manager noscript fallback
        /// </summary>
        /// <returns>Google Tag Manager noscript tag or empty string</returns>
        public string GetGoogleTagManagerNoScript()
        {
            try
            {
                var settingsProvider = new CspSettingsProvider();
                var cspSettings = settingsProvider.GetCspSettings();
                
                if (cspSettings?.EnableGoogleAnalytics == true)
                {
                    return cspSettings.GetGoogleTagManagerNoScript();
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                Log.Error("NonceAwareRenderingModel: Failed to get Google Tag Manager noscript", ex, this);
                return string.Empty;
            }
        }

        /// <summary>
        /// Helper method to inject scripts at different locations with nonce support
        /// </summary>
        /// <param name="location">The injection location (head, body-top, body-bottom)</param>
        /// <param name="scriptContent">The JavaScript content to inject</param>
        /// <param name="includeScriptTags">Whether to wrap content in script tags</param>
        /// <returns>The script HTML with nonce attribute</returns>
        public string InjectScript(string location, string scriptContent, bool includeScriptTags = true)
        {
            if (string.IsNullOrWhiteSpace(scriptContent))
            {
                return string.Empty;
            }

            switch (location?.ToLowerInvariant())
            {
                case "head":
                    return _scriptInjectionService.InjectHeadScript(scriptContent, includeScriptTags);
                case "body-top":
                case "bodytop":
                    return _scriptInjectionService.InjectBodyTopScript(scriptContent, includeScriptTags);
                case "body-bottom":
                case "bodybottom":
                    return _scriptInjectionService.InjectBodyBottomScript(scriptContent, includeScriptTags);
                default:
                    Log.Warn($"NonceAwareRenderingModel: Unknown script injection location '{location}'. Using body-bottom as default.", this);
                    return _scriptInjectionService.InjectBodyBottomScript(scriptContent, includeScriptTags);
            }
        }

        /// <summary>
        /// Checks if nonce support is enabled in the current CSP settings
        /// </summary>
        /// <returns>True if nonce is enabled, false otherwise</returns>
        public bool IsNonceEnabled()
        {
            try
            {
                var settingsProvider = new CspSettingsProvider();
                var cspSettings = settingsProvider.GetCspSettings();
                return cspSettings?.EnableNonce == true;
            }
            catch (Exception ex)
            {
                Log.Error("NonceAwareRenderingModel: Failed to check if nonce is enabled", ex, this);
                return false;
            }
        }

        /// <summary>
        /// Checks if Google Analytics is enabled in the current CSP settings
        /// </summary>
        /// <returns>True if Google Analytics is enabled, false otherwise</returns>
        public bool IsGoogleAnalyticsEnabled()
        {
            try
            {
                var settingsProvider = new CspSettingsProvider();
                var cspSettings = settingsProvider.GetCspSettings();
                return cspSettings?.EnableGoogleAnalytics == true;
            }
            catch (Exception ex)
            {
                Log.Error("NonceAwareRenderingModel: Failed to check if Google Analytics is enabled", ex, this);
                return false;
            }
        }
    }
}

