#!/bin/bash
set -e

echo "Aguardando PostgreSQL ficar disponivel..."

# Aguardar PostgreSQL ficar pronto (timeout de 30 segundos)
timeout=30
while [ $timeout -gt 0 ]; do
  if timeout 2 bash -c "</dev/tcp/postgres/5432"; then
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