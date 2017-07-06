using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using FrameworkThree.Web.Utils;
using Microsoft.AspNetCore.Authentication;

namespace FrameworkThree.Web
{
    public class Startup
    {
        public static string GraphURI;
        public static string AuthenticationAuthority;
        public static string ServiceAppIDURI;
        public static string ApplicationID;
        public static string ApplicationKey;
        public static string AfterLogoutRedirectURI;

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
            services.AddMvc();

            services.AddSession();

            services.AddAuthentication(sharedOptions => sharedOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);
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

            app.UseSession();

            GraphURI = Configuration["AzureActiveDirectory:GraphURI"];
            AuthenticationAuthority = Configuration["AzureActiveDirectory:AuthenticationAuthority"];
            ServiceAppIDURI = Configuration["AzureActiveDirectory:ServiceAppIDURI"];
            ApplicationID = Configuration["AzureActiveDirectory:ApplicationID"];
            ApplicationKey = Configuration["AzureActiveDirectory:ApplicationKey"];
            AfterLogoutRedirectURI = Configuration["AzureActiveDirectory:AfterLogoutRedirectURI"];

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
            {
                Authority = AuthenticationAuthority,
                ClientId = ApplicationID,                
                ResponseType = OpenIdConnectResponseType.CodeIdToken,
                GetClaimsFromUserInfoEndpoint = false,
                PostLogoutRedirectUri = AfterLogoutRedirectURI,

                Events = new OpenIdConnectEvents
                {
                    OnRemoteFailure = OnRemoteFailureHandler,
                    OnAuthorizationCodeReceived = OnAuthorizationCodeReceivedHandler,
                }
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private async Task OnAuthorizationCodeReceivedHandler(AuthorizationCodeReceivedContext context)
        {
            Uri redirectURI = new Uri(context.Properties.Items[OpenIdConnectDefaults.RedirectUriForCodePropertiesKey]);
            string azureADUserObjectID = (context.Ticket.Principal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;

            ClientCredential clientCredential = new ClientCredential(ApplicationID, ApplicationKey);
            AuthenticationContext authenticationContext = new AuthenticationContext(AuthenticationAuthority, new NaiveSessionCache(azureADUserObjectID, context.HttpContext.Session));
            AuthenticationResult authenticationResult = await authenticationContext.AcquireTokenByAuthorizationCodeAsync(context.ProtocolMessage.Code, redirectURI, clientCredential, GraphURI);

            context.HandleCodeRedemption(authenticationResult.AccessToken, authenticationResult.IdToken);
        }

        private Task OnRemoteFailureHandler(FailureContext context)
        {
            context.HandleResponse();

            context.Response.Redirect("/Home/Error?message=" + context.Failure.Message);

            return Task.FromResult(0);
        }
    }
}
