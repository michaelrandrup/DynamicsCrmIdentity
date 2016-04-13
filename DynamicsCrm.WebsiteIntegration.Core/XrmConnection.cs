using Microsoft.Xrm.Client;

namespace DynamicsCrm.WebsiteIntegration.Core
{
    // Url=https://contoso.crm.dynamics.com; Username=jsmith@live-int.com; Password=passcode; DeviceID=contoso-ba9f6b7b2e6d; DevicePassword=passcode
    public static class XrmConnection
    {


        private static CrmConnection _Connection = null;
        public static CrmConnection Connection
        {
            get
            {
                if (_Connection == null)
                {
                    InitializeConnection(null);
                }
                return _Connection;
            }
            set
            {
                _Connection = value;
            }
        }

        private static void InitializeConnection(string Name)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[Name ?? ConnectionName].ConnectionString;
            _Connection = CrmConnection.Parse(connectionString);
        }



        #region Public Properties

        private static string _ConnectionName = "CrmServices";
        public static string ConnectionName
        {
            get { return _ConnectionName; }
            set { _ConnectionName = value; }
        }


        #endregion

    }
}
