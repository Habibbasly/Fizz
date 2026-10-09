# ---- Build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restauration à part pour profiter du cache Docker tant que les .csproj ne changent pas.
COPY src/FizzBuzz.Domain/FizzBuzz.Domain.csproj src/FizzBuzz.Domain/
COPY src/FizzBuzz.Application/FizzBuzz.Application.csproj src/FizzBuzz.Application/
COPY src/FizzBuzz.Infrastructure/FizzBuzz.Infrastructure.csproj src/FizzBuzz.Infrastructure/
COPY src/FizzBuzz.Api/FizzBuzz.Api.csproj src/FizzBuzz.Api/
RUN dotnet restore src/FizzBuzz.Api/FizzBuzz.Api.csproj

COPY src/ src/
RUN dotnet publish src/FizzBuzz.Api/FizzBuzz.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

# ---- Runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_RUNNING_IN_CONTAINER=true

COPY --from=build /app/publish .

# Utilisateur non-root fourni par l'image Microsoft.
USER $APP_UID
EXPOSE 8080

ENTRYPOINT ["dotnet", "FizzBuzz.Api.dll"]
