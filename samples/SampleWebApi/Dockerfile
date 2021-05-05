FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Orchestrator.Service.Dispatcher/Orchestrator.Service.Dispatcher.csproj", "Orchestrator.Service.Dispatcher/"]
COPY ["Orchestrator.Service.Dispatcher/NuGet.Config", "Orchestrator.Service.Dispatcher/"]
COPY ["Orchestrator.Service.Camunda/Orchestrator.Service.Camunda.csproj", "Orchestrator.Service.Camunda/"]
RUN dotnet restore "Orchestrator.Service.Dispatcher/Orchestrator.Service.Dispatcher.csproj"
COPY . .
WORKDIR "/src/Orchestrator.Service.Dispatcher"
RUN dotnet build "Orchestrator.Service.Dispatcher.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Orchestrator.Service.Dispatcher.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Orchestrator.Service.Dispatcher.dll"]