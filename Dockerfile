FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["NHIGIA.Modern/NHIGIA.Modern.csproj", "NHIGIA.Modern/"]
RUN dotnet restore "NHIGIA.Modern/NHIGIA.Modern.csproj"
COPY NHIGIA.Modern/ NHIGIA.Modern/
WORKDIR /src/NHIGIA.Modern
RUN dotnet publish "NHIGIA.Modern.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NHIGIA.Modern.dll"]
