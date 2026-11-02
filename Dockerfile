FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["MusicDatabase.Api/MusicDatabase.Api.csproj", "MusicDatabase.Api/"]
COPY ["MusicDatabase.Core/MusicDatabase.Core.csproj", "MusicDatabase.Core/"]
COPY ["MusicDatabase.Data/MusicDatabase.Data.csproj", "MusicDatabase.Data/"]
COPY ["MusicDatabase.Contracts/MusicDatabase.Contracts.csproj", "MusicDatabase.Contracts/"]
COPY ["MusicDatabase.Domain/MusicDatabase.Domain.csproj", "MusicDatabase.Domain/"]
COPY ["MusicDatabase.Common/MusicDatabase.Common.csproj", "MusicDatabase.Common/"]

RUN dotnet restore "MusicDatabase.Api/MusicDatabase.Api.csproj"

COPY . .
WORKDIR /src/MusicDatabase.Api
RUN dotnet publish "MusicDatabase.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "MusicDatabase.Api.dll"]
