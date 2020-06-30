using Api.Domain;
using Api.Exceptions;
using Api.Extensions;
using Api.Persistence;
using Api.SignalR;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using MongoDB.Entities;
using NSwag;
using NSwag.AspNetCore;
using NSwag.Generation.Processors.Security;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        private string IdentityServerUri => _configuration.GetValue<string>("IdentityServerUri");


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
                        options.JsonSerializerOptions.IgnoreNullValues = true;
                    });
            services.AddAuthorization();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Authority = IdentityServerUri;
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
                    var allowedCorsOrigins = _configuration.GetSection("AllowedCorsOrigins").AsEnumerable()
                        .Select(p => p.Value)
                        .Where(v => v != null)
                        .ToArray();
                    policy.WithOrigins(allowedCorsOrigins)
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
                            AuthorizationUrl = $"{IdentityServerUri}connect/authorize",
                            TokenUrl = $"{IdentityServerUri}connect/token",
                        }
                    }
                });

                document.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor(JwtBearerDefaults.AuthenticationScheme));
            });

            services.AddHttpContextAccessor();
            services.AddMediatR(typeof(Startup));

            // to see if we really need to keep that dependency
            // yet we don't use advance functionalities from it
            services.AddMongoDBEntities(
                MongoClientSettings.FromConnectionString(_configuration.GetValue<string>("MongoConnectionString")),
                _configuration.GetValue<string>("GameDatabaseName")
            );

            services.AddSignalR();
            services.AddOptions<GameOptions>().Bind(_configuration.GetSection("Game"), options => options.BindNonPublicProperties = true);

            // Online boardz DI setup
            services.AddSingleton<PlayerIdentity>();
            services.AddSingleton<GameAssert>();
            services.AddSingleton<IUniqueRandomRangeCreator, UniqueRandomRangeCreator>();
            services.Decorate<IUniqueRandomRangeCreator, PrefilledUniqueRandomRangeCreator>();
            services.AddSingleton<IGameRepository, GameRepository>();
            services.AddSingleton<ITicTacToeRepository, TicTacToeRepository>();
            services.AddSingleton<IGameFactory, GameFactory>();
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
            var db = app.ApplicationServices.GetService<DB>();
            db.Index<Game>()
                .Key(g => g.Status, KeyType.Ascending)
                .Create();
            db.Index<Game>()
                .Key(g => g.GameType, KeyType.Ascending)
                .Create();
            db.Collection<Game>().Indexes.CreateOne(new CreateIndexModel<Game>(
                new IndexKeysDefinitionBuilder<Game>().Ascending(new StringFieldDefinition<Game>("Players.ID"))
            ));

            db.Collection<Player>().Indexes.CreateOne(new CreateIndexModel<Player>(
                new IndexKeysDefinitionBuilder<Player>().Ascending(new StringFieldDefinition<Player>("Games.ID"))
            ));
        }
    }
}
