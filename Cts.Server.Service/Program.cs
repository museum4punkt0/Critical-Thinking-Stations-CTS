using Gemelo.Components.Cts.WebApiHost;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Gemelo.Applications.Cts.Server.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CtsWebApiHost.MediaDirectoryPath = @"C:\+Daten\Cts\Media";
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    IConfiguration configuration = hostContext.Configuration;
                    services.Configure<ServiceSettings>(configuration.GetSection(nameof(ServiceSettings)));
                    ServiceSettings serviceSettings = 
                        configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
                    CtsWebApiHost.MediaDirectoryPath = serviceSettings.MediaDirectoryPath;
                    CtsWebApiHost.SqlServerOptions = serviceSettings.SqlServerOptions;
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel()
                        .UseUrls(CtsWebApiHost.DefaultUrl)
                        .UseStartup<CtsWebApiStartup>();
                });
        //.ConfigureServices((hostContext, services) =>
        //{
        //    services.AddHostedService<Worker>();
        //});
    }
}
