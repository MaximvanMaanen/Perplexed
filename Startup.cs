using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArgentPonyWarcraftClient.Extensions.DependencyInjection;
using guild_instinct.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace guild_instinct
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
            services.AddScoped<OAuthService>();
            services.AddScoped<InstinctWarcraftClientService>();
            
            services.AddCookieManager(options =>
            {
                // Allow cookie data to encrypt by default it allow encryption
                options.AllowEncryption = true;
                // Throw if not all chunks of a cookie are available on a request for re-assembly.
                options.ThrowForPartialCookies = true;
                // Set null if not allow to devide in chunks
                options.ChunkSize = null;
                // Default Cookie expire time if expire time set to null of cookie
                // Default time is 1 day to expire cookie 
                options.DefaultExpireTimeInDays = 1;
            });

            services.AddWarcraftClients(
                Configuration["BattleNet:ClientId"], 
                Configuration["BattleNet:ClientSecret"], 
                ArgentPonyWarcraftClient.Region.Europe, 
                ArgentPonyWarcraftClient.Locale.en_GB
                );
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
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
