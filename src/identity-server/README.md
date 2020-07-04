# Authentication database
IdentityServer4 provides an integration with EFCore, as long as there is a supported provider for EFCore the database can be used. 
Hence for simplicity MongoDB isn't used here, we may revise this later.
Currently PostgreSQL is used.

# Running everything locally
## External authentication
If you want to use external authentication providers: Google, Facebook, Microsoft, Apple you need to provide the relevant client id/secret for each provider.

During development the secrets should never be in the source code!
Instead use dotnet user secrets:
```
dotnet user-secrets set "Authentication:Google:ClientId" "<client-id>"
dotnet user-secrets set "Authentication:Google:ClientSecret" "<client-secret>"

dotnet user-secrets set "Authentication:Facebook:ClientId" "<client-id>"
dotnet user-secrets set "Authentication:Facebook:ClientSecret" "<client-secret>"
```
It will put them outside of project folder so that they will never accidentally be committed.
See: https://docs.microsoft.com/fr-fr/aspnet/core/security/authentication/social/google-logins?view=aspnetcore-3.1

## Authentication database
The development connection string is in appsettings.Development.json.
The development connection string password should also be placed in dotnet secrets:
```
dotnet user-secrets set "PostgrePassword" "password"
```

## Development user secrets
So in the end they should look like:
```json
{
  "PostgresPassword": "xxx",
  "Authentication:Google:ClientSecret": "xxx",
  "Authentication:Google:ClientId": "xxx",
  "Authentication:Facebook:ClientSecret": "xxx",
  "Authentication:Facebook:ClientId": "xxx",
  "Kestrel:Certificates:Development:Password": "xxx"
}
```

# Running in Azure
This is easier, only one user secret is needed, the App Configuration connection string.
From Azure App Configuration everything else is fetched: key/values, secrets, certificates