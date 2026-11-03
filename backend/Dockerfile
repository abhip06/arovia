# Build and publish the API using the .NET SDK.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy project files first so package restore can be reused between source edits.
COPY ["src/HealthcareStore.Api/HealthcareStore.Api.csproj", "src/HealthcareStore.Api/"]
COPY ["src/HealthcareStore.Application/HealthcareStore.Application.csproj", "src/HealthcareStore.Application/"]
COPY ["src/HealthcareStore.Domain/HealthcareStore.Domain.csproj", "src/HealthcareStore.Domain/"]
COPY ["src/HealthcareStore.Infrastructure/HealthcareStore.Infrastructure.csproj", "src/HealthcareStore.Infrastructure/"]

RUN dotnet restore "src/HealthcareStore.Api/HealthcareStore.Api.csproj"

COPY . .

RUN dotnet publish "src/HealthcareStore.Api/HealthcareStore.Api.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

# Run the published app on the smaller ASP.NET runtime image as its built-in non-root user.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_NOLOGO=true

EXPOSE 8080

COPY --from=build /app/publish .

USER $APP_UID

ENTRYPOINT ["dotnet", "HealthcareStore.Api.dll"]
