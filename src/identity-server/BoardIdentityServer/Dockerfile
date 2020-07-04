FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_ENVIRONMENT="Production"
ENV ASPNETCORE_URLS="https://+:5000"

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["BoardIdentityServer/BoardIdentityServer.csproj", "BoardIdentityServer/"]
RUN dotnet restore "BoardIdentityServer/BoardIdentityServer.csproj"
COPY . .
WORKDIR "/src/BoardIdentityServer"
RUN dotnet build "BoardIdentityServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BoardIdentityServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BoardIdentityServer.dll"]