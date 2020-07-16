using System;
using AspNetCore.SignalR.HttpForwarder.Internal;
using AspNetCore.SignalR.HttpForwarder.TestApp.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;

namespace AspNetCore.SignalR.HttpForwarder.TestApp
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
            services.AddMvc();

            services.AddSignalR()
                .AddHttpForwarder();

            if (!String.IsNullOrWhiteSpace(Configuration["Nodes"]))
                services.AddTransient<IOtherNodesProvider, StaticNodesFromConfig>();

            services
                .AddHttpClient("Forwarder")
                .AddTransientHttpErrorPolicy(s => s.WaitAndRetryAsync(10, i => TimeSpan.FromSeconds(i)));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHttpForwarder();
                endpoints.MapHub<NotificationHub>("/hub/notifications");
                endpoints.MapHub<ChatroomHub>("/hub/chat");
                endpoints.MapControllers();
            });
        }
    }
}
