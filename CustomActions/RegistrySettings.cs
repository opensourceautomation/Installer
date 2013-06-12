using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSAInstallCustomActions
{
    class RegistrySettings
    {
        private string dbPassword;
        public string DbPassword
        {
            get
            {
                return dbPassword;
            }
            set
            {
                dbPassword = value;
            }
        }

        public string DbUsername
        {
            get;
            set;
        }

        public string DbPort
        {
            get;
            set;
        }

        public string DbConnection
        {
            get;
            set;
        }

        public string WcfServer
        {
            get;
            set;
        }

        public void LoadCurrentValues()
        {
            OSAE.ModifyRegistry registry = new OSAE.ModifyRegistry();
            registry.SubKey = @"SOFTWARE\OSAE\DBSETTINGS";

            DbPassword = registry.Read("DBPASSWORD");
            DbUsername = registry.Read("DBUSERNAME");
            DbPort = registry.Read("DBPORT");
            DbConnection = registry.Read("DBCONNECTION");

            OSAE.ModifyRegistry baseRegistry = new OSAE.ModifyRegistry();
            baseRegistry.SubKey = @"SOFTWARE\OSAE";
            WcfServer = baseRegistry.Read("WcfServer");
        }

        public bool RequiredPresent()
        {
            if (string.IsNullOrEmpty(DbPassword) ||
               string.IsNullOrEmpty(DbUsername) ||
               string.IsNullOrEmpty(DbPort) ||
               string.IsNullOrEmpty(DbConnection))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
