FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /source

# Copy csproj and restore as distinct layers
COPY Mailout.csproj ./Mailout/Mailout.csproj
RUN cd Mailout && dotnet restore

# Copy everything else and build app
COPY . ./Mailout/
WORKDIR /source/Mailout
RUN dotnet publish -c release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app ./
ENTRYPOINT ["dotnet", "Mailout.dll"]