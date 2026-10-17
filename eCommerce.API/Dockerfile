# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /src

# Copy project files first to maximize restore-layer caching.
COPY ["eCommerce.API/eCommerce.API.csproj", "eCommerce.API/"]
COPY ["eCommerce.Application/eCommerce.Application.csproj", "eCommerce.Application/"]
COPY ["eCommerce.Infrastructure/eCommerce.Infrastructure.csproj", "eCommerce.Infrastructure/"]
COPY ["eCommerce.Domain/eCommerce.Domain.csproj", "eCommerce.Domain/"]

RUN dotnet restore "eCommerce.API/eCommerce.API.csproj"

FROM restore AS build
COPY . .
WORKDIR /src/eCommerce.API
RUN dotnet build "eCommerce.API.csproj" -c Release --no-restore

FROM build AS publish
RUN dotnet publish "eCommerce.API.csproj" -c Release -o /app/publish --no-build --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Default container port; can be overridden by ASPNETCORE_URLS at runtime.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=publish /app/publish .

# copy entrypoint script
COPY eCommerce.API/docker-entrypoint.sh /app/docker-entrypoint.sh
RUN chmod +x /app/docker-entrypoint.sh

ENTRYPOINT ["/app/docker-entrypoint.sh"]

