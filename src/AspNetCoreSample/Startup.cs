using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace AspNetCoreSample
{
    public class Startup
    {
        TokenValidationParameters tokenValidationParameters;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
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

            var tokenSecretKey = Encoding.UTF8.GetBytes("w4t4l8sC8xa6f5n4S6P7sGTBN8Urgb0D");

            tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                RequireSignedTokens = true,
                IssuerSigningKey = new SymmetricSecurityKey(tokenSecretKey),                
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                RequireExpirationTime = true,
                ClockSkew = new TimeSpan(0, 5, 0),
                ValidateActor = false
            };

            AuthenticateUI(app);
            AuthenticateAPI(app);

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        public void AuthenticateAPI(IApplicationBuilder app)
        {
            // IsAPI method returns TRUE when a request route is started with "/api".
            // For those routes, we'll use JWT Authorization:
            app.UseWhen(context => IsAPI(context), builder =>
            {
                builder.UseJwtBearerAuthentication(new JwtBearerOptions
                {
                    AutomaticAuthenticate = true,
                    TokenValidationParameters = tokenValidationParameters
                });
            });
        }

        public void AuthenticateUI(IApplicationBuilder app)
        {
            // For non-API routes, we'll use Cookie Authorization, as an example.
            app.UseWhen(context => !IsAPI(context), builder =>
            {
                builder.UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AuthenticationScheme = "Auth",
                    LoginPath = new PathString("/auth/login"),
                    AutomaticAuthenticate = true,
                    AutomaticChallenge = true
                });
            });
        }

        private bool IsAPI(HttpContext context)
        {
            return context.Request.Path.Value.StartsWith("/api/") && !context.Request.Path.Value.StartsWith("/api/token");
        }
    }
}
