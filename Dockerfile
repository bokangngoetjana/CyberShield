# -------------------------
# Build stage
# -------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["CyberShield.csproj", "./"]
RUN dotnet restore "CyberShield.csproj"

COPY . .

RUN dotnet publish "CyberShield.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false


# -------------------------
# Runtime stage
# -------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:10000
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 10000

ENTRYPOINT ["dotnet", "CyberShield.dll"]