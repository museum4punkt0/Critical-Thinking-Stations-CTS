using Gemelo.Components.Common.Text;

namespace Gemelo.Components.Cts.Database.Databases
{
    public class SqlServerOptions
    {
        #region Konstanten

        private const string DefaultDatabaseName = "CtsDatabase";

        private const string ConstConnectionStringFormatUseLocalDB
            = "Server=(localdb)\\mssqllocaldb;Database={0};Trusted_Connection=True;MultipleActiveResultSets=true";
        private const string ConstConnectionStringFormatWithUser
            = "Server={0};Database={1};User Id={2};Password={3};";
        private const string ConstConnectionStringFormatWithIntegratedSecurity
            = "Server={0};Database={1};Integrated Security=True";

        #endregion Konstanten

        #region Felder und Eigenschaften

        public string Server { get; set; }

        public string DatabaseName { get; set; } = DefaultDatabaseName;

        public string Username { get; set; } = null;

        public string Password { get; set; } = string.Empty;

        #endregion Felder und Eigenschaften

        public string GetConnectionString()
        {
            if (Server.IsNullOrEmpty())
            {
                return string.Format(ConstConnectionStringFormatUseLocalDB, DatabaseName);
            }
            else if (Username.IsNullOrEmpty())
            {
                return string.Format(ConstConnectionStringFormatWithIntegratedSecurity, Server, DatabaseName);
            }
            else return string.Format(ConstConnectionStringFormatWithUser, Server, DatabaseName, Username, Password);
        }
    }
}
