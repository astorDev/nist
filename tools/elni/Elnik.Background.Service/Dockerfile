FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Elnik.Background.Service/Elnik.Background.Service.csproj", "Elnik.Background.Service/"]
RUN dotnet restore "Elnik.Background.Service/Elnik.Background.Service.csproj"
COPY . .
WORKDIR "/src/Elnik.Background.Service"
RUN dotnet build "Elnik.Background.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Elnik.Background.Service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ARG ELNIK_URL=http://elnik-webapi
ENTRYPOINT dotnet Elnik.Background.Service.dll --elnikUrl=$ELNIK_URL