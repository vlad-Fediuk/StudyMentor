FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["study-mentor-api/study-mentor-api.csproj", "study-mentor-api/"]
RUN dotnet restore "study-mentor-api/study-mentor-api.csproj"
COPY study-mentor-api/ study-mentor-api/
WORKDIR /src/study-mentor-api
RUN dotnet publish "study-mentor-api.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "study-mentor-api.dll"]
