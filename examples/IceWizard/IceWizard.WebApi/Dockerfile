FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["IceWizard.WebApi/IceWizard.WebApi.csproj", "IceWizard.WebApi/"]
COPY ["IceWizard.Protocol/IceWizard.Protocol.csproj", "IceWizard.Protocol/"]
RUN dotnet restore "IceWizard.WebApi/IceWizard.WebApi.csproj"
COPY . .
WORKDIR "/src/IceWizard.WebApi"
RUN dotnet build "IceWizard.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IceWizard.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IceWizard.WebApi.dll"]