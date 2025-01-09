FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Configura para usar a porta 80
ENV ASPNETCORE_URLS=http://+:80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . ./
RUN dotnet restore "Api/Api.csproj"
RUN dotnet build "Api/Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Api/Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
#Desabilita o Check de TLS

ENTRYPOINT ["dotnet", "Api.dll"]