using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Glass.Mapper.Sc;
using RRA.Foundation.DI;
using RRA.Foundation.GlassMapper.Enums;
using Sitecore;
using Sitecore.Data;

namespace Foundation.CSP.Services
{
    /// <summary>
    /// Service Factory for IoC injection of Glass Mapper Sitecore Service
    /// </summary>
    [Service(typeof(ISitecoreServiceFactory), Lifetime = Lifetime.Scoped)]
    public class SitecoreServiceFactory : ISitecoreServiceFactory
    {
        public ISitecoreService CreateInstance(ContextDb db = ContextDb.NotSet, Database sitecoreDb = null)
        {
            try
            {
                var systemSites = Constants.Sites.ToList();
                if (db == ContextDb.Custom && sitecoreDb == null)
                {
                    db = ContextDb.NotSet;
                }

                if (sitecoreDb != null)
                {
                    db = ContextDb.Custom;
                }

                switch (db)
                {
                    case ContextDb.Master:
                        return new SitecoreService(Constants.Databases.Masterdb);
                    case ContextDb.Web:
                        return new SitecoreService(Constants.Databases.Webdb);
                    case ContextDb.Custom:
                        return new SitecoreService(sitecoreDb);
                    default:
                        if (Context.Site == null)
                            return new SitecoreService(Database.GetDatabase(Constants.Databases.Webdb));

                        if (systemSites.Contains(Context.Site.Name) || Context.PageMode.IsExperienceEditor || Context.PageMode.IsPreview)
                            return new SitecoreService(Database.GetDatabase(Constants.Databases.Masterdb));

                        return new SitecoreService(Database.GetDatabase(Context.Site.Database == null ? Constants.Databases.Masterdb : Context.Site.Database.Name));
                }
            }
            catch (Exception)
            {
                if (Context.Database != null)
                {
                    throw;
                }

                return null;
            }
        }

        public ISitecoreService CreateInstance(Database sitecoreDb)
        {
            return CreateInstance(ContextDb.Custom, sitecoreDb);
        }
    }
}
