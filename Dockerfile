FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY BotApi/ ./BotApi/
COPY Bot.Core/ ./Bot.Core/
COPY Bot.Data/ ./Bot.Data/
RUN ls
RUN dotnet restore "BotApi/BotApi.csproj"
RUN dotnet build "BotApi/BotApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BotApi/BotApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BotApi.dll"]