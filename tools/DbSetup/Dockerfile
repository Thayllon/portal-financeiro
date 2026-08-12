FROM mcr.microsoft.com/dotnet/sdk:11.0-preview AS build
WORKDIR /src

COPY tools/DbSetup/DbSetup.csproj tools/DbSetup/
RUN dotnet restore tools/DbSetup/DbSetup.csproj

COPY tools/DbSetup/ tools/DbSetup/
RUN dotnet publish tools/DbSetup/DbSetup.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:11.0-preview AS runtime
WORKDIR /app
COPY --from=build /app/publish .
COPY scripts/sqlserver/ ./scripts/sqlserver/
ENTRYPOINT ["dotnet", "DbSetup.dll"]
