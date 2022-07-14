using Gemelo.Components.Cts.Database.Databases;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Gemelo.Components.Cts.WebApiHost
{
    public class CtsWebApiStartup
    {
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                using var context = scope.ServiceProvider.GetService<CtsDatabaseContext>();
                context.Database.Migrate();
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<CtsDatabaseContext>(options =>
            {
                string connectionString = CtsWebApiHost.GetConnectionString();
                options.UseSqlServer(connectionString);
            });
            services.AddControllers();
        }
    }
}
