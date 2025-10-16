using System;
using Foundation.CSP.Services;
using Glass.Mapper.Sc;
using RRA.Foundation.DI;
using Sitecore;
using Sitecore.Caching;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Text;

namespace Foundation.CSP.Models
{
    /// <summary>
    /// Provides CSP settings from Sitecore with Glass Mapper and caching support
    /// </summary>
    [Service(typeof(ICspSettingsProvider), Lifetime = Lifetime.Scoped)]
    public class CspSettingsProvider : ICspSettingsProvider
    {
        private const string CacheKeyPrefix = "CSP_Settings_";
        private const int CacheExpirationMinutes = 60;

        private readonly string _settingsPath;
        private readonly ISitecoreServiceFactory _sitecoreServiceFactory;

        public CspSettingsProvider(ISitecoreServiceFactory sitecoreServiceFactory)
        {
            _settingsPath = Settings.GetSetting("CSP.SettingsPath", "/sitecore/content/RRA/Data/Settings/CSP");
            _sitecoreServiceFactory = sitecoreServiceFactory ?? throw new ArgumentNullException(nameof(sitecoreServiceFactory));
        }

        /// <summary>
        /// Gets the CSP settings item from Sitecore
        /// </summary>
        public Item GetCspSettingsItem()
        {
            try
            {
                var sitecoreService = _sitecoreServiceFactory.CreateInstance();
                if (sitecoreService == null)
                {
                    Log.Error("CSP: SitecoreService is not available", this);
                    return null;
                }

                return sitecoreService.Database.GetItem(_settingsPath);
            }
            catch (Exception ex)
            {
                Log.Error($"CSP: Error retrieving settings item from path '{_settingsPath}'", ex, this);
                return null;
            }
        }

        /// <summary>
        /// Determines if CMS-based CSP is enabled
        /// </summary>
        public bool IsCspEnabled()
        {
            try
            {
                var sitecoreService = _sitecoreServiceFactory.CreateInstance();
                if (sitecoreService == null)
                {
                    return false;
                }

                var settings = sitecoreService.GetItem<ICSPSettings>(_settingsPath);
                return settings?.Enabled ?? false;
            }
            catch (Exception ex)
            {
                Log.Error("CSP: Error checking if CSP is enabled", ex, this);
                return false;
            }
        }

        /// <summary>
        /// Gets the complete CSP settings model with caching
        /// </summary>
        public CspSettings GetCspSettings()
        {
            try
            {
                var sitecoreService = _sitecoreServiceFactory.CreateInstance();
                if (sitecoreService == null)
                {
                    Log.Error("CSP: SitecoreService is not available", this);
                    return null;
                }

                var cacheKey = $"{CacheKeyPrefix}{sitecoreService.Database?.Name ?? "unknown"}";
                var cache = CacheManager.GetNamedInstance("CSP.Settings", StringUtil.ParseSizeString("10MB"), true);

                // Try to get from cache
                var cachedSettings = cache.GetValue(cacheKey) as CspSettings;
                if (cachedSettings != null)
                {
                    return cachedSettings;
                }

                // Retrieve from Sitecore using Glass Mapper
                var glassSettings = sitecoreService.GetItem<ICSPSettings>(_settingsPath);
                if (glassSettings == null)
                {
                    Log.Warn("CSP: Settings item not found. CSP headers will not be applied.", this);
                    return null;
                }

                // Convert Glass Mapper interface to concrete class
                var settings = new CspSettings
                {
                    Enabled = glassSettings.Enabled,
                    DefaultSrc = glassSettings.DefaultSrc,
                    ScriptSrc = glassSettings.ScriptSrc,
                    StyleSrc = glassSettings.StyleSrc,
                    ImgSrc = glassSettings.ImgSrc,
                    FontSrc = glassSettings.FontSrc,
                    ConnectSrc = glassSettings.ConnectSrc,
                    FrameSrc = glassSettings.FrameSrc,
                    FrameAncestors = glassSettings.FrameAncestors,
                    ObjectSrc = glassSettings.ObjectSrc,
                    MediaSrc = glassSettings.MediaSrc,
                    WorkerSrc = glassSettings.WorkerSrc,
                    ManifestSrc = glassSettings.ManifestSrc,
                    BaseUri = glassSettings.BaseUri,
                    FormAction = glassSettings.FormAction,
                    ChildSrc = glassSettings.ChildSrc,
                    UpgradeInsecureRequests = glassSettings.UpgradeInsecureRequests,
                    BlockAllMixedContent = glassSettings.BlockAllMixedContent,
                    ReportUri = glassSettings.ReportUri,
                    
                    // Google Analytics settings
                    EnableGoogleAnalytics = glassSettings.EnableGoogleAnalytics,
                    EnableGoogleSignals = glassSettings.EnableGoogleSignals,
                    GoogleTagManagerID = glassSettings.GoogleTagManagerID,
                    
                    // Nonce settings
                    EnableNonce = glassSettings.EnableNonce,

                    // Glass Mapper properties
                    InnerItem = glassSettings.InnerItem,
                    Language = glassSettings.Language,
                    Version = glassSettings.Version
                };

                // Cache the settings
                if (settings.Enabled)
                {
                    cache.Add(cacheKey, settings, TimeSpan.FromMinutes(CacheExpirationMinutes));
                }

                return settings;
            }
            catch (Exception ex)
            {
                Log.Error("CSP: Error retrieving CSP settings", ex, this);
                return null;
            }
        }
    }
}
