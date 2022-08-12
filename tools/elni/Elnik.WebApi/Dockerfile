FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Elnik.WebApi/Elnik.WebApi.csproj", "Elnik.WebApi/"]
COPY ["Elnik.Protocol/Elnik.Protocol.csproj", "Elnik.Protocol/"]
RUN dotnet restore "Elnik.WebApi/Elnik.WebApi.csproj"
COPY . .
WORKDIR "/src/Elnik.WebApi"
RUN dotnet build "Elnik.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Elnik.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ARG KIBANA_URL=http://localhost:5601
ENTRYPOINT dotnet Elnik.WebApi.dll --KibanaUrl $KIBANA_URL