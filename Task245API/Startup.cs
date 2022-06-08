using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WaspIntegration.Business.Services;
using WaspIntegration.Service.Interfaces;

namespace WaspAPI
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddSingleton<IFtpConfigManagerService, FtpConfigManagerService>();

            services.AddScoped<IFtpWriterService, FtpWriterService>();

            services.AddScoped<IFtpDownLoaderService, GetRemoteOrdersService>();

            services.AddScoped<IManifestService, ManifestService>();

            services.AddScoped<IOrderService, CreateOrdersService>();

            services.AddScoped<IMailWaspService, MailKitService>();

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