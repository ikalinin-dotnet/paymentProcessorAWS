FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Services/User/User.API/User.API.csproj", "src/Services/User/User.API/"]
COPY ["src/Services/User/User.Domain/User.Domain.csproj", "src/Services/User/User.Domain/"]
COPY ["src/Services/User/User.Infrastructure/User.Infrastructure.csproj", "src/Services/User/User.Infrastructure/"]
RUN dotnet restore "src/Services/User/User.API/User.API.csproj"

COPY . .
WORKDIR "/src/src/Services/User/User.API"
RUN dotnet build "User.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "User.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "User.API.dll"]