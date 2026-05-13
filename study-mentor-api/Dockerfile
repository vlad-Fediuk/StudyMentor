FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["study-mentor-api/study-mentor-api.csproj", "study-mentor-api/"]
RUN dotnet restore "study-mentor-api/study-mentor-api.csproj"
COPY . .
WORKDIR "/src/study-mentor-api"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "study-mentor-api.dll"]
