using Gemelo.Components.Cts.Database.Databases;

namespace Gemelo.Applications.Cts.Server.Service
{
    public class ServiceSettings
    {
        public SqlServerOptions SqlServerOptions { get; set; } = new SqlServerOptions();

        public string MediaDirectoryPath { get; set; }
    }
}
