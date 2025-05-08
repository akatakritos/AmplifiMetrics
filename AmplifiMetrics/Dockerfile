FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/runtime-deps:10.0-preview-noble-chiseled AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:10.0-preview-noble-aot AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AmplifiMetrics/AmplifiMetrics.csproj", "AmplifiMetrics/"]
RUN dotnet restore "AmplifiMetrics/AmplifiMetrics.csproj"
COPY . .
WORKDIR "/src/AmplifiMetrics"
RUN dotnet publish "./AmplifiMetrics.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["./AmplifiMetrics"]
