using Sitecore.Data.Items;

namespace Foundation.CSP.Models
{
    /// <summary>
    /// Interface for retrieving CSP settings from Sitecore
    /// </summary>
    public interface ICspSettingsProvider
    {
        /// <summary>
        /// Gets the CSP settings item from Sitecore
        /// </summary>
        /// <returns>The CSP settings item, or null if not found</returns>
        Item GetCspSettingsItem();

        /// <summary>
        /// Gets the complete CSP settings model
        /// </summary>
        /// <returns>The CSP settings model, or null if disabled or not found</returns>
        ICSPSettings GetCspSettings();

        /// <summary>
        /// Determines if CMS-based CSP is enabled
        /// </summary>
        /// <returns>True if enabled, false otherwise</returns>
        bool IsCspEnabled();
    }
}
