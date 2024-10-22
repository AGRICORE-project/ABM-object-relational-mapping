#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["AGRICORE-ABM-object-relational-mapping/AGRICORE-ABM-object-relational-mapping.csproj", "AGRICORE-ABM-object-relational-mapping/"]
COPY ["DB/DB.csproj", "DB/"]
RUN dotnet restore "AGRICORE-ABM-object-relational-mapping/AGRICORE-ABM-object-relational-mapping.csproj"
COPY . .
WORKDIR "/src/AGRICORE-ABM-object-relational-mapping"
RUN dotnet build "AGRICORE-ABM-object-relational-mapping.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AGRICORE-ABM-object-relational-mapping.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AGRICORE-ABM-object-relational-mapping.dll"]