#!/bin/bash
set -e

# Extrai o host da connection string ou usa variável dedicada
# Exemplo: "Host=localhost;Port=5432;..." → "localhost"
DB_HOST=${DB_HOST:-$(echo "${ConnectionStrings__DefaultConnection}" | grep -oP '(?<=Host=)[^;]+')}
DB_PORT=${DB_PORT:-5432}

echo "Aguardando PostgreSQL em $DB_HOST:$DB_PORT..."

timeout=60
while [ $timeout -gt 0 ]; do
  if timeout 2 bash -c "</dev/tcp/$DB_HOST/$DB_PORT" 2>/dev/null; then
    echo "PostgreSQL esta pronto!"
    break
  fi
  echo "PostgreSQL nao esta pronto ainda - aguardando... ($timeout)"
  sleep 2
  timeout=$((timeout-2))
done

if [ $timeout -le 0 ]; then
  echo "Timeout aguardando PostgreSQL!"
  exit 1
fi

echo "Iniciando FiapX API..."

# Iniciar a API (as migrations serão aplicadas automaticamente pelo EF Core)
exec dotnet FiapX.API.dll