# ─── Build Stage ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar arquivos de projeto e restaurar dependências
COPY ["src/FiapX.Worker/FiapX.Worker.csproj", "FiapX.Worker/"]
COPY ["src/FiapX.Application/FiapX.Application.csproj", "FiapX.Application/"]
COPY ["src/FiapX.Domain/FiapX.Domain.csproj", "FiapX.Domain/"]
COPY ["src/FiapX.Infrastructure/FiapX.Infrastructure.csproj", "FiapX.Infrastructure/"]
COPY ["src/FiapX.Shared/FiapX.Shared.csproj", "FiapX.Shared/"]

RUN dotnet restore "FiapX.Worker/FiapX.Worker.csproj"

# Copiar código fonte e compilar
COPY src/ .
WORKDIR "/src/FiapX.Worker"
RUN dotnet build "FiapX.Worker.csproj" -c Release -o /app/build

# ─── Publish Stage ─────────────────────────────────────────────────────────
FROM build AS publish
RUN dotnet publish "FiapX.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ─── Runtime Stage ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Instalar FFmpeg
RUN apt-get update && \
    apt-get install -y ffmpeg && \
    rm -rf /var/lib/apt/lists/*

# Criar diretórios para storage
RUN mkdir -p /app/storage/uploads /app/storage/outputs /app/storage/temp

# Copiar arquivos publicados
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "FiapX.Worker.dll"]
