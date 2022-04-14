using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WaspIntegration.Business.Services;
using WaspIntegration.Service.Interfaces;

namespace WaspIntegration
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddSingleton<IFtpConfigManagerService, FtpConfigManagerService>();

            services.AddScoped<IFtpServerService, FtpServerService>();

            services.AddScoped<IManifestService, ManifestService>();

            services.AddScoped<IOrderService, CreateOrderService>();

            services.AddScoped<IMailService, MailService>();

            services.AddScoped<ICanceledOrdersService, CanceledOrdersService>();

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}