using Azure.Core;
using Azure.Identity;
using ConfigurationSubstitution;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Net;

namespace Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((ctx, builder) =>
                {
                    var settings = builder.Build();
                    if (settings.IsUsingAzureAppConfiguration())
                    {
                        var credentials = settings.IsRunningInContainer()
                            ? new ManagedIdentityCredential(settings.GetAzureManagedIdentity())
                            : (TokenCredential)new DefaultAzureCredential();
                        builder.AddAzureAppConfiguration(options =>
                        {
                            options.Connect(new Uri(settings.GetAzureAppConfigurationUri()), credentials)
                                .Select(keyFilter: "*", labelFilter: "all")
                                .Select(keyFilter: "*", labelFilter: "game-svc")
                                .UseFeatureFlags(o => o.Label = "game-svc")
                                .ConfigureKeyVault(kv =>
                                {
                                    kv.SetCredential(credentials);
                                });
                        });
                    }

                    builder.EnableSubstitutions();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel((ctx, options) =>
                    {
                        options.Listen(
                            new IPEndPoint(IPAddress.Any, ctx.Configuration.GetKestrelPort()),
                            listenOptions =>
                            {
                                listenOptions.KestrelServerOptions.AddServerHeader = false;
                                if (ctx.Configuration.IsUsingAzureAppConfiguration() && ctx.Configuration.IsRunningInContainer())
                                {
                                    // if we run in AKS then we should use the KeyVault certificate
                                    // if we try to use it locally it'll be weird as it won't match the domain
                                    listenOptions.UseHttps(ctx.Configuration.GetSigningCertificate());
                                }
                                else
                                {
                                    // will use ASP.NET Core developer certificate
                                    listenOptions.UseHttps();
                                }
                            });
                    });

                    webBuilder.UseStartup<Startup>();
                });
    }
}
