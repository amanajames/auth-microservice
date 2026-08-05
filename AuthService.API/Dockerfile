FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/AuthService.API/AuthService.API.csproj", "src/AuthService.API/"]
COPY ["src/AuthService.Application/AuthService.Application.csproj", "src/AuthService.Application/"]
COPY ["src/AuthService.Domain/AuthService.Domain.csproj", "src/AuthService.Domain/"]
COPY ["src/AuthService.Infrastructure/AuthService.Infrastructure.csproj", "src/AuthService.Infrastructure/"]

RUN dotnet restore "src/AuthService.API/AuthService.API.csproj"

COPY . .
WORKDIR "/src/src/AuthService.API"
RUN dotnet build "AuthService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AuthService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuthService.API.dll"]
