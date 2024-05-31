FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /source

# Copy csproj and restore as distinct layers
COPY ./Server/Server.csproj ./Server/Server.csproj
RUN cd Server && dotnet restore

# Copy everything else and build app
COPY . .
WORKDIR /source/Server
RUN dotnet publish -c release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app ./
ENTRYPOINT ["dotnet", "Server.dll"]