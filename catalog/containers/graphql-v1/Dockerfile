FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /source

# Copy csproj and restore as distinct layers
COPY Catalog.csproj ./Catalog/Catalog.csproj
RUN cd Catalog && dotnet restore

# Copy everything else and build app
COPY . ./Catalog/
WORKDIR /source/Catalog
RUN dotnet publish -c release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app ./
ENTRYPOINT ["dotnet", "Catalog.dll"]