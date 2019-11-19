using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using MqttHome.WebSockets;
using MqttHomeWeb.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace MqttHomeWeb
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var mvc = services.AddControllersWithViews();
            services.AddSingleton<WebsocketManager>();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(options => { 
                    
                    });

#if (DEBUG)
            mvc.AddRazorRuntimeCompilation();
#endif
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
                app.UseExceptionHandler("/Home/Error");
            }

            Program.RootFolderPath = env.ContentRootPath;

            app.UseStaticFiles(); // For the wwwroot folder

            var wellKnownFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/.well-known");

            // create .well-known if it doesnt exist
            Directory.CreateDirectory(wellKnownFolder);

            app.UseStaticFiles(new StaticFileOptions    //For the '.well-known' folder
            {
                FileProvider = new PhysicalFileProvider(wellKnownFolder),
                RequestPath = "/.well-known",
                ServeUnknownFileTypes = true,
            });

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseWebSockets(new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                ReceiveBufferSize = 4096
            });

            app.UseMiddleware<WebsocketMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=System}/{action=Index}/{id?}");
            });

            app.UseCookiePolicy(new CookiePolicyOptions {
                MinimumSameSitePolicy = SameSiteMode.None
            });
        }
    }
}
