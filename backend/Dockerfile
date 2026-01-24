FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["ServiceHubPro.API/ServiceHubPro.API.csproj", "ServiceHubPro.API/"]
COPY ["ServiceHubPro.Application/ServiceHubPro.Application.csproj", "ServiceHubPro.Application/"]
COPY ["ServiceHubPro.Domain/ServiceHubPro.Domain.csproj", "ServiceHubPro.Domain/"]
COPY ["ServiceHubPro.Infrastructure/ServiceHubPro.Infrastructure.csproj", "ServiceHubPro.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "ServiceHubPro.API/ServiceHubPro.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/ServiceHubPro.API"
RUN dotnet build "ServiceHubPro.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ServiceHubPro.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ServiceHubPro.API.dll"]
