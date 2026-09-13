FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copiar tudo (a pasta backend inteira será o contexto de build)
COPY . ./

# Restaurar e compilar a aplicação
WORKDIR /app/src/WebApolice.Api
RUN dotnet restore
RUN dotnet publish -c Release -o /app/out

# Build da imagem final
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "WebApolice.Api.dll"]
