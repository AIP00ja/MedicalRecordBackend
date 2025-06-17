# Use the .NET 6.0 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy the whole repo
COPY . .

# Navigate into your project folder
WORKDIR /src/backend

# Restore dependencies
RUN dotnet restore

# Publish the project
RUN dotnet publish -c Release -o /app/publish

# Use the ASP.NET runtime for hosting
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/publish .

# Start the app
ENTRYPOINT ["dotnet", "MedicalRecordBackend.dll"]
