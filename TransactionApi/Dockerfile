FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /source

COPY . .

# Restore as distinct layers
RUN dotnet restore "./TransactionApi.csproj" --disable-parallel

# Build and publish a release
RUN dotnet publish "./TransactionApi.csproj" -c release -o /app --no-restore

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app ./
ENTRYPOINT ["dotnet", "TransactionApi.dll"]
