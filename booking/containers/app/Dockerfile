FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /source

# Copy csproj and restore as distinct layers
COPY Booking.csproj ./Booking/Booking.csproj
RUN cd Booking && dotnet restore

# Copy everything else and build app
COPY . ./Booking/
WORKDIR /source/Booking
RUN dotnet publish -c release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app ./
ENTRYPOINT ["dotnet", "Booking.dll"]