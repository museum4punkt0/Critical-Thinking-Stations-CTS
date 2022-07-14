using Gemelo.Components.Cts.Database.Databases;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;

namespace Gemelo.Components.Cts.WebApiHost
{
    public static class CtsWebApiHost
    {
        #region Konstanten

        public const string DefaultUrl = "http://*:8536";

        #endregion Konstanten

        private static readonly IWebHost s_WebHost;

        public static string MediaDirectoryPath { get; set; }

        public static SqlServerOptions SqlServerOptions { get; set; } = new SqlServerOptions();

        static CtsWebApiHost()
        {
            s_WebHost = new WebHostBuilder()
              .UseKestrel()
              .UseUrls(DefaultUrl)
              .UseStartup<CtsWebApiStartup>()
              .Build();
        }

        public static void Start()
        {
            Task.Run(() => s_WebHost.Run());
        }

        public static void Stop()
        {
            s_WebHost.StopAsync();
        }

        internal static string GetConnectionString()
        {
            return SqlServerOptions.GetConnectionString();
        }
    }
}
