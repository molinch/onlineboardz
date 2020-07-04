using BoardIdentityServer.Persistence;
using IdentityServer4;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace BoardIdentityServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            CurrentEnvironment = env;
        }

        private IConfiguration Configuration { get; }
        private IWebHostEnvironment CurrentEnvironment { get; }
        private string ConnectionString => Configuration["PostgresConnectionString"].Replace("PostgresPassword", Configuration["PostgresPassword"]);

        private string[] AllowedCorsOrigins => Configuration.GetSection("AllowedCorsOrigins").AsEnumerable()
                        .Select(p => p.Value)
                        .Where(v => v != null)
                        .ToArray();

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins(AllowedCorsOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            services.AddMvcCore()
                .SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Latest);

            // NEEDED?
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            
            var identityServerBuilder = services
                .AddIdentityServer(options =>
                {
                    options.UserInteraction.LoginUrl = Configuration.GetValue<string>("UI:Login");
                    options.UserInteraction.LogoutUrl = Configuration.GetValue<string>("UI:Logout");
                    options.UserInteraction.ErrorUrl = Configuration.GetValue<string>("UI:Error");
                })
                // this adds the config data from DB (clients, resources)
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                        builder.UseNpgsql(ConnectionString,
                            sql => sql.MigrationsAssembly(typeof(Startup).Assembly.FullName));
                })
                // this adds the operational data from DB (codes, tokens, consents)
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                        builder.UseNpgsql(ConnectionString,
                            sql => sql.MigrationsAssembly(typeof(Startup).Assembly.FullName));

                    // this enables automatic token cleanup. this is optional.
                    options.EnableTokenCleanup = true;
                    options.TokenCleanupInterval = 30;
                });

            if (Configuration.IsUsingAzureAppConfiguration())
            {
                identityServerBuilder.AddSigningCredential(Configuration.GetSigningCertificate());
            }
            else if (CurrentEnvironment.IsDevelopment())
            {
                identityServerBuilder.AddDeveloperSigningCredential();
            }
            else
            {
                throw new Exception("Missing signing certificate");
                // see: https://damienbod.com/2020/02/10/create-certificates-for-identityserver4-signing-using-net-core/
                // see: https://tatvog.wordpress.com/2018/06/05/identityserver4-addsigningcredential-using-certificate-stored-in-azure-key-vault/
            }

            services.AddTransient<IReturnUrlParser, ReturnUrlParser>();
            services.AddSingleton<IProfileService, ProfileService>();

            services
                .AddAuthentication()
                .AddGoogle(Provider.Google, options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.Scope.Add("profile");

                    var googleAuthNSection = Configuration.GetSection("Authentication:Google");
                    options.ClientId = googleAuthNSection["ClientId"];
                    options.ClientSecret = googleAuthNSection["ClientSecret"];

                    options.ClaimActions.MapJsonKey("picture", "picture", "url");
                    options.ClaimActions.MapJsonKey("locale", "locale", "string");
                    options.SaveTokens = true;
                })
                .AddFacebook(Provider.Facebook, options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                    var googleAuthNSection = Configuration.GetSection("Authentication:Facebook");
                    options.ClientId = googleAuthNSection["ClientId"];
                    options.ClientSecret = googleAuthNSection["ClientSecret"];

                    options.ClaimActions.MapJsonKey("picture", "picture", "url");
                    options.ClaimActions.MapJsonKey("locale", "locale", "string");
                    options.SaveTokens = true;
                });

            services.AddControllers();
            services.AddDbContext<UserDbContext>(options => options.UseNpgsql(ConnectionString));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                ResetDatabase(app);
                app.UseDeveloperExceptionPage();
            }

            InitializeDatabase(app);

            app
                .UseCors()
                .UseRouting()
                .UseIdentityServer()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
        }

        private void ResetDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Clients.RemoveRange(context.Clients.ToList());
                context.IdentityResources.RemoveRange(context.IdentityResources.ToList());
                context.ApiResources.RemoveRange(context.ApiResources.ToList());
                context.SaveChanges();
            }
        }

        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                // Our models
                serviceScope.ServiceProvider.GetRequiredService<UserDbContext>().Database.Migrate();

                // For IdentityServer4
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();
                context.SaveChanges();

                if (!context.Clients.Any())
                {
                    var clients = new List<Client>();
                    Configuration.GetSection("IdentityServer:Clients").Bind(clients);
                    foreach (var client in clients)
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in new IdentityResource[]{
                        new IdentityResources.OpenId(),
                        new IdentityResources.Profile(),
                    })
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiResources.Any())
                {
                    var resources = new List<ApiResource>();
                    Configuration.GetSection("IdentityServer:ApiResources").Bind(resources);
                    foreach (var resource in resources)
                    {
                        context.ApiResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }
            }
        }
    }
}
