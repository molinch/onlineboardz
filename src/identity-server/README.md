# External authentication
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

# Authentication database
IdentityServer4 provides an integration with EFCore, as long as there is a supported provider for EFCore the database can be used. 
Hence for simplicity MongoDB isn't used here, we may revise this later.
Currently PostgreSQL is used.
The connection string should also be placed in dotnet secrets:
```
dotnet user-secrets set "PostgreSQLConnectionString" "Host=localhost; user id=identityserver-db-user; password=password; database=identityserver"
```