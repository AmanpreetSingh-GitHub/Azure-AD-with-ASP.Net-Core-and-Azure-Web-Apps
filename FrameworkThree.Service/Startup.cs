using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FrameworkThree.Domain;
using Microsoft.EntityFrameworkCore;
using FrameworkThree.Repository.Interface;
using FrameworkThree.Repository;
using FrameworkThree.BusinessLayer.Interface;
using FrameworkThree.BusinessLayer;

namespace FrameworkThree.Service
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            var connection = @"Server=(localdb)\MSSQLLocalDb;Database=FrameworkThree;Trusted_Connection=True;";
            services.AddDbContext<FrameworkThreeContext>(options => options.UseSqlServer(connection));
            services.AddSingleton<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IGenericRepository<Student>, GenericRepository<Student>>();

            services.AddTransient<IStudentLogic, StudentLogic>();

            services.AddMvc();

            services.AddAuthentication();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                Authority = Configuration["AzureActiveDirectory:AuthenticationAuthority"],
                Audience = Configuration["AzureActiveDirectory:AppIDURI"],
                AutomaticAuthenticate = true,
                AutomaticChallenge = true
            });

            app.UseMvc();
        }
    }
}
