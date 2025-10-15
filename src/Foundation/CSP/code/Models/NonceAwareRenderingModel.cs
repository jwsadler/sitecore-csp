using System;
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

        protected NonceAwareRenderingModel()
        {
            _nonceService = new NonceService();
        }

        // Constructor for dependency injection
        protected NonceAwareRenderingModel(INonceService nonceService)
        {
            _nonceService = nonceService ?? new NonceService();
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
        /// Gets the nonce service for generating and managing nonce tokens
        /// </summary>
        protected INonceService NonceService => _nonceService;

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
        /// Creates a simple script tag with nonce attribute for inline JavaScript
        /// Use this with your existing script injection service
        /// </summary>
        /// <param name="scriptContent">The JavaScript content</param>
        /// <param name="additionalAttributes">Additional attributes as key-value pairs (e.g., "type=text/javascript")</param>
        /// <returns>Complete script tag with nonce</returns>
        public string CreateNonceScript(string scriptContent, string additionalAttributes = null)
        {
            if (string.IsNullOrWhiteSpace(scriptContent))
            {
                return string.Empty;
            }

            var nonce = CurrentNonce;
            var attributes = string.IsNullOrWhiteSpace(additionalAttributes) ? "" : $" {additionalAttributes}";
            
            return $"<script nonce=\"{nonce}\"{attributes}>{scriptContent}</script>";
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
