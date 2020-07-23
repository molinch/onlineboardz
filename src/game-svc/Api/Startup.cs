using Api.Domain;
using Api.Exceptions;
using Api.Extensions;
using Api.Persistence;
using Api.SignalR;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;
using MongoDB.Driver;
using NSwag;
using NSwag.AspNetCore;
using NSwag.Generation.Processors.Security;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MongoDB.Thin;

namespace Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _isDevelopment = env.IsDevelopment();
        }

        private readonly IConfiguration _configuration;

        private readonly bool _isDevelopment;

        private string IdentityServerBaseUri => _configuration["IdentityServerBaseUri"];


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            if (_isDevelopment)
            {
                Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
            }

            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    // at the moment always serialize nulls since there's a bug in .NET5 (it would omit serializing value types having default value)
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
                });
            services.AddAuthorization();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Authority = IdentityServerBaseUri;
                options.Audience = "game-api";

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // If the request is for game hub...
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            path.StartsWithSegments("/hubs/game"))
                        {
                            // Read the token out of the query string
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins(IdentityServerBaseUri, _configuration["FrontendBaseUri"], _configuration["GameServiceBaseUri"])
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            // Add security definition and scopes to document
            services.AddOpenApiDocument(document =>
            {
                document.AddSecurity(JwtBearerDefaults.AuthenticationScheme, Enumerable.Empty<string>(), new OpenApiSecurityScheme
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
                            AuthorizationUrl = $"{IdentityServerBaseUri}connect/authorize",
                            TokenUrl = $"{IdentityServerBaseUri}connect/token",
                        }
                    }
                });

                document.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor(JwtBearerDefaults.AuthenticationScheme));
            });

            services.AddHttpContextAccessor();
            services.AddMediatR(typeof(Startup));

            var connectionString = _configuration["MongoConnectionString"];
            services.AddMongo(connectionString, _configuration["GameDatabaseName"]);

            services.AddSignalR();
            services.AddOptions<GameOptions>().Bind(_configuration.GetSection("Game"), options => options.BindNonPublicProperties = true);
            services.AddAutoMapper(typeof(Startup));
            services.AddFeatureManagement();

            // Online boardz DI setup
            services.AddSingleton<PlayerIdentity>();
            services.AddSingleton<GameAssert>();
            services.AddSingleton<IUniqueRandomRangeCreator, UniqueRandomRangeCreator>();
            services.Decorate<IUniqueRandomRangeCreator, PrefilledUniqueRandomRangeCreator>();
            services.AddSingleton<IGameRepository, GameRepository>();
            services.AddSingleton<ITicTacToeRepository, TicTacToeRepository>();
            services.AddSingleton<IGameFactory, GameFactory>();
            services.AddSingleton<GameService>();
            services.AddSingleton<IGameHubSender, GameHubSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors(); // must be between UseRouting and UseAuthorization

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseExceptionHandler(new ExceptionHandlerOptions
            {
                ExceptionHandler = new ExceptionHandler().Invoke
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<GameHub>("/hubs/game");
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

            InitializeMongoDb(app);
        }

        private void InitializeMongoDb(IApplicationBuilder app)
        {
            var db = app.ApplicationServices.GetService<IMongoDatabase>()!;

            db.Collection<Game>().Indexes
                .Add(g => g.GameType)
                .Add(g => g.Status)
                .Add(g => g.Players, p => p.Id);

            db.Collection<Player>().Indexes
                .Add(p => p.Games, g => g.Id);
        }
    }
}
