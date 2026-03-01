FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TaxIntegration.Api/TaxIntegration.Api.csproj", "TaxIntegration.Api/"]
RUN dotnet restore "TaxIntegration.Api/TaxIntegration.Api.csproj"
COPY . .
WORKDIR "/src/TaxIntegration.Api"
RUN dotnet build "./TaxIntegration.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TaxIntegration.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaxIntegration.Api.dll"]
