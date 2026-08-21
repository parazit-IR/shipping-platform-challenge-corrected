# =========================
# Build
# =========================

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY . .

RUN API_PROJ=$(find . -name "ShippingPlatform.Api.csproj" -print -quit) \
    && echo "API project: $API_PROJ" \
    && dotnet restore "$API_PROJ"

RUN API_PROJ=$(find . -name "ShippingPlatform.Api.csproj" -print -quit) \
    && dotnet publish "$API_PROJ" \
       -c Release \
       -o /out/api \
       --no-restore


# =========================
# Create EF migration bundle
# =========================

FROM build AS migration-build

RUN dotnet tool install --global dotnet-ef --version 10.0.11

ENV PATH="${PATH}:/root/.dotnet/tools"

RUN INFRA_PROJ=$(find . -name "ShippingPlatform.Infrastructure.csproj" -print -quit) \
    && API_PROJ=$(find . -name "ShippingPlatform.Api.csproj" -print -quit) \
    && INFRA_DIR=$(dirname "$INFRA_PROJ") \
    && API_DIR=$(dirname "$API_PROJ") \
    && echo "Infrastructure: $INFRA_DIR" \
    && echo "Startup: $API_DIR" \
    && dotnet ef migrations bundle \
       --project "$INFRA_DIR" \
       --startup-project "$API_DIR" \
       --context Context \
       --output /out/migrate


# =========================
# Migration runner
# =========================

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS migrations

WORKDIR /app

COPY --from=migration-build /out/migrate ./migrate

RUN chmod +x ./migrate

ENTRYPOINT ["./migrate"]


# =========================
# API runtime
# =========================

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS api

WORKDIR /app

COPY --from=build /out/api .

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "ShippingPlatform.Api.dll"]