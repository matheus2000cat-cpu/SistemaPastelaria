FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY SistemaPastelaria.csproj ./
RUN dotnet restore SistemaPastelaria.csproj
COPY . ./
RUN dotnet publish SistemaPastelaria.csproj --configuration Release --no-restore --output /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=10000
ENV PORT=10000
EXPOSE 10000
COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "SistemaPastelaria.dll"]
