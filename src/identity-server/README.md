During development the secrets should never be in the source code!
Instead use:
```
dotnet user-secrets set "Authentication:Google:ClientId" "<client-id>"
dotnet user-secrets set "Authentication:Google:ClientSecret" "<client-secret>"
```
See: https://docs.microsoft.com/fr-fr/aspnet/core/security/authentication/social/google-logins?view=aspnetcore-3.1