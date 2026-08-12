FROM mcr.microsoft.com/dotnet/sdk:11.0-preview AS build
WORKDIR /src

COPY PortalFinanceiro.API.slnx ./
COPY src/PortalFinanceiro.Core/PortalFinanceiro.Core.csproj src/PortalFinanceiro.Core/
COPY src/PortalFinanceiro.Infrastructure/PortalFinanceiro.Infrastructure.csproj src/PortalFinanceiro.Infrastructure/
COPY src/PortalFinanceiro.API/PortalFinanceiro.API.csproj src/PortalFinanceiro.API/
RUN dotnet restore

COPY . .
RUN dotnet build --no-restore -c Release

FROM build AS publish
RUN dotnet publish src/PortalFinanceiro.API/PortalFinanceiro.API.csproj -c Release --no-build -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:11.0-preview AS runtime
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "PortalFinanceiro.API.dll"]
