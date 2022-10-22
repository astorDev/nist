FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Template.WebApi/Template.WebApi.csproj", "Template.WebApi/"]
COPY ["Template.Protocol/Template.Protocol.csproj", "Template.Protocol/"]
RUN dotnet restore "Template.WebApi/Template.WebApi.csproj"
COPY . .
WORKDIR "/src/Template.WebApi"
RUN dotnet build "Template.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Template.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Template.WebApi.dll"]