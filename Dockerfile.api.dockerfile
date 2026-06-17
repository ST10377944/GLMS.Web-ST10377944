FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files
COPY ContractManagement.API/*.csproj ./ContractManagement.API/
COPY GLMS.Web/*.csproj ./GLMS.Web/

# Restore dependencies
RUN dotnet restore ./ContractManagement.API/ContractManagement.API.csproj

# Copy all source code
COPY . .

# Publish API
WORKDIR /src/ContractManagement.API
RUN dotnet publish -c Release -o /app/publish --no-restore

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ContractManagement.API.dll"]