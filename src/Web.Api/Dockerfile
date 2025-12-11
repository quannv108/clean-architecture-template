# syntax=docker/dockerfile:1
# ReadyToRun (R2R) compilation for faster startup while maintaining full EF Core/Scrutor compatibility

ARG BUILD_CONFIGURATION=Release
ARG DOTNET_VERSION=10.0

# =============================================================================
# Base stage - Runtime image with security hardening
# =============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION}-alpine AS base

RUN apk upgrade --no-cache && \
    apk add --no-cache icu-libs

RUN addgroup -g 1000 appuser && \
    adduser -u 1000 -G appuser -s /bin/sh -D appuser

WORKDIR /app

# Port 8080: Production behind reverse proxy (ALB, nginx) with SSL termination
# Port 8081: Standalone HTTPS with certificates
# ForwardedHeaders middleware ensures HSTS works correctly behind proxies
EXPOSE 8080
EXPOSE 8081

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_HTTPS_PORTS=8081

# =============================================================================
# Restore stage - Separate for better layer caching
# =============================================================================
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION}-alpine AS restore

WORKDIR /src

# Copy project files first for better layer caching
COPY ["Directory.Packages.props", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["src/Web.Api/Web.Api.csproj", "src/Web.Api/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/SharedKernel/SharedKernel.csproj", "src/SharedKernel/"]
COPY ["src/ServiceDefaults/ServiceDefaults.csproj", "src/ServiceDefaults/"]

# CRITICAL: PublishReadyToRun=true during restore downloads R2R runtime packages for linux-musl-x64
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore "src/Web.Api/Web.Api.csproj" \
    -r linux-musl-x64 \
    /p:PublishReadyToRun=true

# =============================================================================
# Build stage - Compile the application
# =============================================================================
FROM restore AS build

ARG BUILD_CONFIGURATION

COPY . .

WORKDIR /src/src/Web.Api

RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet build "Web.Api.csproj" \
    -c ${BUILD_CONFIGURATION} \
    -r linux-musl-x64 \
    -o /app/build \
    --no-restore \
    /p:TreatWarningsAsErrors=true

# =============================================================================
# Publish stage - Create optimized runtime artifacts
# =============================================================================
FROM build AS publish

ARG BUILD_CONFIGURATION

# Uses aspnet runtime (not Native AOT) - required for EF Core/Scrutor/Reflection
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish "Web.Api.csproj" \
    -c ${BUILD_CONFIGURATION} \
    -r linux-musl-x64 \
    --self-contained false \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false \
    /p:PublishReadyToRun=true \
    /p:PublishReadyToRunShowWarnings=true

# =============================================================================
# Final stage - Production-ready image
# =============================================================================
FROM base AS final

LABEL maintainer="QuanNv108" \
      org.opencontainers.image.title="Clean Architecture Web API" \
      org.opencontainers.image.description=".NET Clean Architecture API with CQRS, DDD, and Vertical Slice Architecture" \
      org.opencontainers.image.version="1.0.0" \
      org.opencontainers.image.vendor="QuanNv108"

WORKDIR /app

COPY --from=publish --chown=appuser:appuser /app/publish .

USER appuser

ENTRYPOINT ["dotnet", "Web.Api.dll"]
