using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Models;
using Microsoft.AspNetCore.Routing.Constraints;

namespace MvcMovie
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings1.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: false)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            string currentMood = Configuration["Mood"];

            IConfigurationSection coloursSection = Configuration.GetSection("Colours");
            IEnumerable<IConfigurationSection> coloursSectionMembers = coloursSection.GetChildren();
            List<string> colours = (from c in coloursSectionMembers select c.Value).ToList();

            IConfigurationSection languagesSection = Configuration.GetSection("Languages");
            IEnumerable<IConfigurationSection> languagesSectionMembers = languagesSection.GetChildren();
            Dictionary<string, List<string>> platformLanguages = new Dictionary<string, List<string>>();
            foreach (var platform in languagesSectionMembers)
            {
                List<string> langs = (from p in platform.GetChildren() select p.Value).ToList();
                platformLanguages[platform.Key] = langs;
            }
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();          

            services.AddDbContext<MvcMovieContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("MvcMovieContext")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute("Default", "{controller}/{action}/{id?}",
                    defaults: new { controller = "Home", action = "Index" }
                    , constraints: new { id = new IntRouteConstraint() });

                routes.MapRoute("Route", "routing/{controller}/{action}");
            });
        }
    }
}
