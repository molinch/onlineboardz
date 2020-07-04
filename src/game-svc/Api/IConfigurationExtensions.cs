using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace Api
{
    public static class IConfigurationExtensions
    {
        public static bool IsRunningInContainer(this IConfiguration configuration)
            => configuration["DOTNET_RUNNING_IN_CONTAINER"] == "true";
        public static int GetKestrelPort(this IConfiguration configuration)
            => configuration.GetValue<int>("KestrelPort");
        public static string GetAzureAppConfigurationUri(this IConfiguration configuration)
            => configuration["AzureAppConfigurationUri"];
        public static bool IsUsingAzureAppConfiguration(this IConfiguration configuration)
            => !string.IsNullOrEmpty(configuration.GetAzureAppConfigurationUri());

        public static string GetAzureManagedIdentity(this IConfiguration configuration)
            => configuration["AzureManagedIdentityClientId"];

        public static X509Certificate2 GetSigningCertificate(this IConfiguration configuration)
        {
            if (configuration["tls-cert"] == null)
            {
                throw new Exception("Missing certificate");
            }

            var pfxBytes = Convert.FromBase64String(configuration["tls-cert"]);
            return new X509Certificate2(pfxBytes);
        }

        private static Regex FindSubstitutions = new Regex(@"(?<=\{)[^}{]*(?=\})", RegexOptions.Compiled);
        public static string GetSubstituted(this IConfiguration configuration, string key)
        {
            var value = configuration[key];
            return ApplySubstitution(configuration, value);
        }

        public static string ApplySubstitution(this IConfiguration configuration, string value)
        {
            var match = FindSubstitutions.Match(value);
            foreach (var capture in match.Captures.Cast<Capture>())
            {
                value = value.Replace("{" + capture.Value + "}", configuration[capture.Value]);
            }
            return value;
        }
    }
}
