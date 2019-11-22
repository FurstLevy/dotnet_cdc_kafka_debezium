FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
WORKDIR /build

COPY ["src/Dotnet.CDC.Api/Dotnet.CDC.Api.csproj", "src/Dotnet.CDC.Api/"]
RUN dotnet restore "src/Dotnet.CDC.Api/Dotnet.CDC.Api.csproj"
COPY . .
WORKDIR /build/src/Dotnet.CDC.Api

FROM build AS publish
RUN dotnet publish "Dotnet.CDC.Api.csproj" -c Release -o /publish

FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 AS app
EXPOSE 80
WORKDIR /app
COPY --from=publish /publish .
ENTRYPOINT ["dotnet", "Dotnet.CDC.Api.dll"]
