# ─── Build Stage ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar arquivos de projeto e restaurar dependências
COPY ["src/FiapX.API/FiapX.API.csproj", "FiapX.API/"]
COPY ["src/FiapX.Application/FiapX.Application.csproj", "FiapX.Application/"]
COPY ["src/FiapX.Domain/FiapX.Domain.csproj", "FiapX.Domain/"]
COPY ["src/FiapX.Infrastructure/FiapX.Infrastructure.csproj", "FiapX.Infrastructure/"]
COPY ["src/FiapX.Shared/FiapX.Shared.csproj", "FiapX.Shared/"]

RUN dotnet restore "FiapX.API/FiapX.API.csproj"

# Copiar código fonte e compilar
COPY src/ .
WORKDIR "/src/FiapX.API"
RUN dotnet build "FiapX.API.csproj" -c Release -o /app/build

# ─── Publish Stage ─────────────────────────────────────────────────────────
FROM build AS publish
RUN dotnet publish "FiapX.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ─── Runtime Stage ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Criar diretórios para storage
RUN mkdir -p /app/storage/uploads /app/storage/outputs /app/storage/temp

# Copiar arquivos publicados
COPY --from=publish /app/publish .

# Copiar script de entrypoint (nome correto: entrypoint.sh)
COPY src/FiapX.API/entrypoint.sh /app/entrypoint.sh
RUN chmod +x /app/entrypoint.sh

# Expor porta
EXPOSE 8080

# Variáveis de ambiente
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["/app/entrypoint.sh"]