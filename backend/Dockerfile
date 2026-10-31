# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY SignalChat.Backend/SignalChat.Backend.csproj SignalChat.Backend/
RUN dotnet restore SignalChat.Backend/SignalChat.Backend.csproj

COPY SignalChat.Backend/ SignalChat.Backend/
RUN dotnet publish SignalChat.Backend/SignalChat.Backend.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

# wwwroot/images is written to at runtime (avatar/chat image uploads) — give the non-root app user ownership
RUN mkdir -p /app/wwwroot/images && chown -R $APP_UID:$APP_UID /app/wwwroot
USER $APP_UID

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SignalChat.Backend.dll"]
