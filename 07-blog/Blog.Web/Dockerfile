# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia o repositório completo para manter as pastas de estudo 01-* a 06-*
COPY . .

WORKDIR /src/07-blog/Blog.Web
RUN dotnet publish "Blog.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Copia a aplicação compilada
COPY --from=build /app/publish .

# Copia os módulos de estudo (01-* a 06-*) para que o FileSystemPostRepository os leia em produção
COPY --from=build /src/01-dotnet-internals-and-performance /src/01-dotnet-internals-and-performance
COPY --from=build /src/02-architecture-and-ddd /src/02-architecture-and-ddd
COPY --from=build /src/03-microservices-and-resilience /src/03-microservices-and-resilience
COPY --from=build /src/04-messaging-and-event-driven /src/04-messaging-and-event-driven
COPY --from=build /src/05-cloud-containers-and-observability /src/05-cloud-containers-and-observability
COPY --from=build /src/06-react-frontend /src/06-react-frontend

# Previne estouro de inotify (limit 128) em ambientes Linux compartilhados como Render
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
ENV DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Blog.Web.dll"]
