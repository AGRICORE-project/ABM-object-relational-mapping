# -- Build Stage --
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /src

# Copy the source code files
COPY . .

# Restore NuGet packages for the projects
RUN dotnet restore

# Build the class library project
WORKDIR "/src/DB"
RUN dotnet build "DB.csproj" -c Release -o /app/build

# Build the web app project
WORKDIR "/src/AGRICORE-ABM-object-relational-mapping"
RUN dotnet build "AGRICORE-ABM-object-relational-mapping.csproj" -c Release -o /app/build

# Publish the web app
RUN dotnet publish "AGRICORE-ABM-object-relational-mapping.csproj" -c Release -o /app/publish


# -- Runtime Stage --
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime

WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app/publish .

# Expose the port that the app will run on
EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "AGRICORE-ABM-object-relational-mapping.dll"]