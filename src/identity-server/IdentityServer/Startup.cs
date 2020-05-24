using IdentityServer4;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Linq;

namespace IdentityServer
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
        private string ConnectionString => Configuration.GetValue<string>("PostgreSQLConnectionString");

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.AddMvc();//.SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            var identityServerBuilder = services.AddIdentityServer()
                .AddProfileService<ProfileService>()
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

            if (CurrentEnvironment.IsDevelopment())
            {
                identityServerBuilder.AddDeveloperSigningCredential();
            }
            else
            {
                throw new System.Exception("missing signing certificate");
                // see: https://damienbod.com/2020/02/10/create-certificates-for-identityserver4-signing-using-net-core/
                // see: https://tatvog.wordpress.com/2018/06/05/identityserver4-addsigningcredential-using-certificate-stored-in-azure-key-vault/
            }

            services
                .AddAuthentication()
                .AddGoogle("Google", options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.Scope.Add("profile");
   
                    var googleAuthNSection = Configuration.GetSection("Authentication:Google");
                    options.ClientId = googleAuthNSection["ClientId"];
                    options.ClientSecret = googleAuthNSection["ClientSecret"];

                    options.ClaimActions.MapJsonKey("picture", "picture", "url");
                    options.ClaimActions.MapJsonKey("locale", "locale", "string");
                    options.SaveTokens = true;
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                InitializeDatabase(app);
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }

        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();
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
