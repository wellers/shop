FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /source

# Copy csproj and restore as distinct layers
COPY Basket.csproj ./Basket/Basket.csproj
RUN cd Basket && dotnet restore

# Copy everything else and build app
COPY . ./Basket/
WORKDIR /source/Basket
RUN dotnet publish -c release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app ./
ENTRYPOINT ["dotnet", "Basket.dll"]