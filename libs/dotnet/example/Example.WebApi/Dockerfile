FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["example/Example.WebApi/Example.WebApi.csproj", "example/Example.WebApi/"]
COPY ["example/Example.Protocol/Example.Protocol.csproj", "example/Example.Protocol/"]
COPY ["responses/Nist.Responses.csproj", "responses/"]
COPY ["queries/Nist.Queries/Nist.Queries.csproj", "queries/Nist.Queries/"]
COPY ["logging/Nist.Logs.csproj", "logging/"]
COPY ["errors/Nist.Errors.csproj", "errors/"]
RUN dotnet restore "example/Example.WebApi/Example.WebApi.csproj"
COPY . .
WORKDIR "/src/example/Example.WebApi"
RUN dotnet build "Example.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Example.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Example.WebApi.dll"]