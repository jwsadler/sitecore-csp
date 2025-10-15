using System;
using Sitecore;
using Sitecore.Caching;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Foundation.CSP.Models
{
    /// <summary>
    /// Provides CSP settings from Sitecore with caching support
    /// </summary>
    public class CspSettingsProvider : ICspSettingsProvider
    {
        private const string CacheKeyPrefix = "CSP_Settings_";
        private const int CacheExpirationMinutes = 60;

        private readonly string _settingsPath;
        private readonly Database _database;

        public CspSettingsProvider()
        {
            _settingsPath = Settings.GetSetting("CSP.SettingsPath", "/sitecore/system/Settings/CSP");
            var databaseName = Settings.GetSetting("CSP.Database", "master");
            _database = Database.GetDatabase(databaseName);
        }

        /// <summary>
        /// Gets the CSP settings item from Sitecore
        /// </summary>
        public Item GetCspSettingsItem()
        {
            if (_database == null)
            {
                Log.Error("CSP: Database is not available", this);
                return null;
            }

            try
            {
                return _database.GetItem(_settingsPath);
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
            var settingsItem = GetCspSettingsItem();
            if (settingsItem == null)
            {
                return false;
            }

            return MainUtil.GetBool(settingsItem["Enabled"], false);
        }

        /// <summary>
        /// Gets the complete CSP settings model with caching
        /// </summary>
        public CspSettings GetCspSettings()
        {
            var cacheKey = $"{CacheKeyPrefix}{_database?.Name ?? "unknown"}";
            var cache = CacheManager.GetNamedInstance("CSP.Settings", StringUtil.ParseSizeString("10MB"));

            // Try to get from cache
            var cachedSettings = cache.GetValue(cacheKey) as CspSettings;
            if (cachedSettings != null)
            {
                return cachedSettings;
            }

            // Retrieve from Sitecore
            var settingsItem = GetCspSettingsItem();
            if (settingsItem == null)
            {
                Log.Warn("CSP: Settings item not found. CSP headers will not be applied.", this);
                return null;
            }

            var settings = MapItemToSettings(settingsItem);

            // Cache the settings
            if (settings != null && settings.Enabled)
            {
                cache.Add(cacheKey, settings, TimeSpan.FromMinutes(CacheExpirationMinutes));
            }

            return settings;
        }

        /// <summary>
        /// Maps a Sitecore item to CSP settings model
        /// </summary>
        private CspSettings MapItemToSettings(Item item)
        {
            if (item == null)
            {
                return null;
            }

            try
            {
                return new CspSettings
                {
                    Enabled = MainUtil.GetBool(item["Enabled"], false),
                    DefaultSrc = item["Default Src"],
                    ScriptSrc = item["Script Src"],
                    StyleSrc = item["Style Src"],
                    ImgSrc = item["Img Src"],
                    FontSrc = item["Font Src"],
                    ConnectSrc = item["Connect Src"],
                    FrameSrc = item["Frame Src"],
                    FrameAncestors = item["Frame Ancestors"],
                    ObjectSrc = item["Object Src"],
                    MediaSrc = item["Media Src"],
                    WorkerSrc = item["Worker Src"],
                    ManifestSrc = item["Manifest Src"],
                    BaseUri = item["Base Uri"],
                    FormAction = item["Form Action"],
                    ChildSrc = item["Child Src"],
                    UpgradeInsecureRequests = item["Upgrade Insecure Requests"],
                    BlockAllMixedContent = item["Block All Mixed Content"]
                };
            }
            catch (Exception ex)
            {
                Log.Error("CSP: Error mapping settings item to model", ex, this);
                return null;
            }
        }
    }
}

