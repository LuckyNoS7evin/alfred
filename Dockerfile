FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
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