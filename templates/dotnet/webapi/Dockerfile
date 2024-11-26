FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN cd webapi && dotnet publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /src/webapi/bin/Release/net9.0/publish .
ENTRYPOINT dotnet Template.WebApi.dll