# Build stage (SDK, ARM64)
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine-arm64v8 AS build
WORKDIR /src

# copy csproj and restore
COPY ["EconomicEventsWorker/EconomicEventsWorker.csproj", "EconomicEventsWorker/"]
RUN dotnet restore "./EconomicEventsWorker/EconomicEventsWorker.csproj"

# copy everything else and build
COPY . . 
WORKDIR "/src/EconomicEventsWorker"
RUN dotnet publish "./EconomicEventsWorker.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage (tiny)
FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine-arm64v8 AS final
WORKDIR /app

# Install ICU runtime for .NET globalization
RUN apk add --no-cache icu-libs

# Copy app
COPY --from=build /app/publish .

# Optional: force UTF-8 locale (sometimes needed on Alpine)
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=0
ENV LANG=en_US.UTF-8
ENV LC_ALL=en_US.UTF-8

ENTRYPOINT ["dotnet", "EconomicEventsWorker.dll"]
