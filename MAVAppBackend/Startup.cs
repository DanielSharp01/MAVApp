using MAVAppBackend.EF;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;

namespace MAVAppBackend
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddDbContext<MAVAppContext>(options => options.UseMySql(Configuration.GetConnectionString("DefaultConnection")), optionsLifetime: ServiceLifetime.Singleton, contextLifetime: ServiceLifetime.Singleton);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            DI.ServiceProvider = app.ApplicationServices;
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();

            app.UseStatusCodePages(async context =>
            {
                context.HttpContext.Response.ContentType = "application/json";

                JObject response = new JObject
                {
                    ["status"] = context.HttpContext.Response.StatusCode
                };
                await context.HttpContext.Response.WriteAsync(response.ToString(Newtonsoft.Json.Formatting.Indented));
            });
        }
    }
}
