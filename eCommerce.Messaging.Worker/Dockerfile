# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /src
COPY ["eCommerce.Messaging.Worker/eCommerce.Messaging.Worker.csproj", "eCommerce.Messaging.Worker/"]
COPY ["eCommerce.Application/eCommerce.Application.csproj", "eCommerce.Application/"]
COPY ["eCommerce.Infrastructure/eCommerce.Infrastructure.csproj", "eCommerce.Infrastructure/"]
COPY ["eCommerce.Domain/eCommerce.Domain.csproj", "eCommerce.Domain/"]
RUN dotnet restore "eCommerce.Messaging.Worker/eCommerce.Messaging.Worker.csproj"

FROM restore AS build
COPY . .
WORKDIR /src/eCommerce.Messaging.Worker
RUN dotnet build "eCommerce.Messaging.Worker.csproj" -c Release --no-restore

FROM build AS publish
RUN dotnet publish "eCommerce.Messaging.Worker.csproj" -c Release -o /app/publish --no-build --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "eCommerce.Messaging.Worker.dll"]
