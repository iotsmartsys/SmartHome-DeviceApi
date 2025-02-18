FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . ./
RUN dotnet restore "Automation.Services/AutomationService.csproj"
RUN dotnet build "Automation.Services/AutomationService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Automation.Services/AutomationService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
#Desabilita o Check de TLS

ENTRYPOINT ["dotnet", "AutomationService.dll"]
