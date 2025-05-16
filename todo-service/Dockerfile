FROM mcr.microsoft.com/dotnet/sdk:10.0.100-preview.4 AS build
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0.0-preview.4
WORKDIR /App
COPY --from=build /App/out .

ENTRYPOINT ["dotnet", "todo-service.dll"]