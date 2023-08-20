FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["host/Template.Host.csproj", "host/"]
COPY ["protocol/Template.Protocol.csproj", "protocol/"]
RUN dotnet restore "host/Template.Host.csproj"
COPY . .
WORKDIR "/src/host"
RUN dotnet build "Template.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Template.Host.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Template.Host.dll"]