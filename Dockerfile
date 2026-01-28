# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS builder
WORKDIR /src

# Copy solution và csproj
COPY *.sln ./
COPY NextShopV2.Api/*.csproj ./NextShopV2.Api/
COPY NextShopV2.Application/*.csproj ./NextShopV2.Application/
COPY NextShopV2.Domain/*.csproj ./NextShopV2.Domain/
COPY NextShopV2.Infrastructure/*.csproj ./NextShopV2.Infrastructure/
COPY NextShopV2.Shared/*.csproj ./NextShopV2.Shared/

# Restore dependencies
RUN dotnet restore NextShopV2.Api/NextShopV2.Api.csproj

# Copy toàn bộ mã nguồn và publish
COPY . .
WORKDIR /src/NextShopV2.Api
RUN dotnet publish NextShopV2.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=builder /app/publish .

# Copy entrypoint script and listen on the configured port
COPY scripts/entrypoint.sh /app/entrypoint.sh
RUN chmod +x /app/entrypoint.sh

ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
EXPOSE ${PORT:-8080}

ENTRYPOINT ["/app/entrypoint.sh"]
