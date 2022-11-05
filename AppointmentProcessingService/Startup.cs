
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PatientAppointment.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppointmentProcessingService.Models;
using Microsoft.EntityFrameworkCore;
using AppointmentProcessingService.Service;

namespace AppointmentProcessingService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddDbContext<ApplicationDbContext>(options =>options.UseSqlServer(Configuration.GetConnectionString("Connection")));
            RegisterServices(services);
        }

        #region Event Consumer

        private void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IServiceBusConsumer, ServiceBusConsumer>();
            services.AddHostedService<ServiceBusWorkerService>();
            services.AddHostedService<SessionQueueBackgroundService>();
        }


        private List<string> GetEventOrQueueNames()
        {
            return new List<string>
            {
                //typeof(SampleDemo1Event).FullName,
                //typeof(SampleDemo2Event).FullName
                "testqueue"
            };
        }

        #endregion Event Consumer

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
