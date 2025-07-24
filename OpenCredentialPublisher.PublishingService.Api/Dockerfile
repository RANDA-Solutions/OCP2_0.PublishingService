#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk:3.1-buster AS build

ARG Nuget_OCPPassword

WORKDIR /src

COPY ./nuget.config .
COPY ["OpenCredentialPublisher.PublishingService.Api/OpenCredentialPublisher.PublishingService.Api.csproj", "OpenCredentialPublisher.PublishingService.Api/"]
COPY ["OpenCredentialPublisher.PublishingService.Data/OpenCredentialPublisher.PublishingService.Data.csproj", "OpenCredentialPublisher.PublishingService.Data/"]
COPY ["OpenCredentialPublisher.PublishingService.Shared/OpenCredentialPublisher.PublishingService.Shared.csproj", "OpenCredentialPublisher.PublishingService.Shared/"]
COPY ["OpenCredentialPublisher.PublishingService.Services/OpenCredentialPublisher.PublishingService.Services.csproj", "OpenCredentialPublisher.PublishingService.Services/"]
COPY ["OpenCredentialPublisher.Credentials.Drawing/OpenCredentialPublisher.Credentials.Drawing.csproj", "OpenCredentialPublisher.Credentials.Drawing/"]
RUN dotnet restore "OpenCredentialPublisher.PublishingService.Api/OpenCredentialPublisher.PublishingService.Api.csproj"
COPY . .
WORKDIR "/src/OpenCredentialPublisher.PublishingService.Api"
RUN dotnet publish "OpenCredentialPublisher.PublishingService.Api.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:3.1-buster-slim
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OpenCredentialPublisher.PublishingService.Api.dll"]