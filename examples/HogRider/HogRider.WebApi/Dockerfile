FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["HogRider.WebApi/HogRider.WebApi.csproj", "HogRider.WebApi/"]
COPY ["HogRider.Protocol/HogRider.Protocol.csproj", "HogRider.Protocol/"]
RUN dotnet restore "HogRider.WebApi/HogRider.WebApi.csproj"
COPY . .
WORKDIR "/src/HogRider.WebApi"
RUN dotnet build "HogRider.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HogRider.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HogRider.WebApi.dll"]