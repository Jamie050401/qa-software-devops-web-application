FROM debian:stable-slim AS base-env
RUN \
   apt-get update && \
   apt-get install -y wget && \
   wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
   dpkg -i packages-microsoft-prod.deb && \
   rm packages-microsoft-prod.deb && \
   apt-get update && \
   apt-get install -y aspnetcore-runtime-8.0
WORKDIR /app
ENV \
    DOTNET_EnableDiagnostics=0 \
    DOTNET_GENERATE_ASPNET_CERTIFICATE=false \
    ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://*:5000
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /src
COPY ./Application .
RUN \
    dotnet restore "/src/Application.csproj" && \
    dotnet build "/src/Application.csproj" -c Release -o /src/build

FROM build-env AS publish-env
RUN dotnet publish "/src/Application.csproj" -c Release -o /src/publish

FROM base-env AS final-env
WORKDIR /app
COPY --from=publish-env /src/publish .

ENTRYPOINT ["dotnet", "Application.dll"]
