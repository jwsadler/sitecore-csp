using Glass.Mapper.Sc;
using RRA.Foundation.GlassMapper.Enums;
using Sitecore.Data;

namespace Foundation.CSP.Services
{
    /// <summary>
    /// Factory interface for creating ISitecoreService instances
    /// </summary>
    public interface ISitecoreServiceFactory
    {
        /// <summary>
        /// Creates a new ISitecoreService instance
        /// </summary>
        /// <param name="db">The context database to use</param>
        /// <param name="sitecoreDb">Optional specific database instance</param>
        /// <returns>ISitecoreService instance</returns>
        ISitecoreService CreateInstance(ContextDb db = ContextDb.NotSet, Database sitecoreDb = null);

        /// <summary>
        /// Creates a new ISitecoreService instance with specific database
        /// </summary>
        /// <param name="sitecoreDb">The database to use</param>
        /// <returns>ISitecoreService instance</returns>
        ISitecoreService CreateInstance(Database sitecoreDb);
    }
}
