using Api.Extensions;
using Api.Persistence;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSwag;
using NSwag.AspNetCore;
using NSwag.Generation.Processors.Security;
using System.Collections.Generic;
using System.Linq;

namespace Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        private string IdentityServerUri => Configuration.GetValue<string>("IdentityServerUri");

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.Authority = IdentityServerUri;
                options.RequireHttpsMetadata = false;
                options.Audience = "game-api";
            });

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    var allowedCorsOrigins = Configuration.GetSection("AllowedCorsOrigins").AsEnumerable()
                        .Select(p => p.Value)
                        .Where(v => v != null)
                        .ToArray();
                    policy.WithOrigins(allowedCorsOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            // Add security definition and scopes to document
            services.AddOpenApiDocument(document =>
            {
                document.AddSecurity("bearer", Enumerable.Empty<string>(), new OpenApiSecurityScheme
                {
                    Type = OpenApiSecuritySchemeType.OAuth2,
                    Flow = OpenApiOAuth2Flow.AccessCode,
                    Flows = new OpenApiOAuthFlows()
                    {
                        AuthorizationCode = new OpenApiOAuthFlow()
                        {
                            Scopes = new Dictionary<string, string>
                            {
                                { "openid", "OpenId" },
                                { "profile", "Profile" },
                                { "game-api", "Access game api" }
                            },
                            AuthorizationUrl = $"{IdentityServerUri}connect/authorize",
                            TokenUrl = $"{IdentityServerUri}connect/token",
                        }
                        /*
                        Implicit = new OpenApiOAuthFlow()
                        {
                            Scopes = new Dictionary<string, string>
                            {
                                { "openid", "OpenId" },
                                { "profile", "Profile" },
                                { "game-api", "Access game api" }
                            },
                            AuthorizationUrl = $"{IdentityServerUri}connect/authorize",
                            TokenUrl = $"{IdentityServerUri}connect/token"
                        }*/
                    }
                });

                document.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("bearer"));
            });

            services.AddSingleton<DatabaseSettings>();
            services.AddSingleton<IGameRepository, GameRepository>();

            services.AddHttpContextAccessor();
            services.AddMediatR(typeof(Startup));

            services.AddSingleton<PlayerIdentity>();
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
            app.UseCors("default");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseOpenApi();
            app.UseSwaggerUi3(settings =>
            {
                settings.OAuth2Client = new OAuth2ClientSettings
                {
                    ClientId = "game-service-swagger",
                    AppName = "Swagger UI for Game service",
                    UsePkceWithAuthorizationCodeGrant = true,
                };
            });
        }
    }
}
