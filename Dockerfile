# Etapa de build: compila y publica el backend.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Wilson Futbol 5/Wilson Futbol 5.csproj", "Wilson Futbol 5/"]
RUN dotnet restore "Wilson Futbol 5/Wilson Futbol 5.csproj"

COPY . .
RUN dotnet publish "Wilson Futbol 5/Wilson Futbol 5.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa final: imagen liviana solo con runtime.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Wilson Futbol 5.dll"]
