FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["webapi/Template.WebApi.csproj", "webapi/"]
COPY ["protocol/Template.Protocol.csproj", "protocol/"]
RUN dotnet restore "webapi/Template.WebApi.csproj"
COPY . .
WORKDIR "/src/webapi"
RUN dotnet build "Template.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Template.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Template.WebApi.dll"]