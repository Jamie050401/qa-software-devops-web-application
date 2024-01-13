FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /Application

COPY ./Application/bin/Release/net8.0/publish ./

RUN dotnet restore

RUN dotnet publish -c Release -o build

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /Application
COPY --from=build-env /Application/build .
ENTRYPOINT ["dotnet", "DotNet.Docker.dll"]
