# 🐋 FiapX - Execução com Docker

## 📋 Pré-requisitos

- Docker Desktop instalado e rodando
- 8GB de RAM livre (recomendado)
- 20GB de espaço em disco

---

## 🚀 Como executar

### 1️⃣ Subir tudo com Docker Compose

Na raiz do projeto, execute:

```bash
docker-compose up --build
```

**O que vai acontecer:**
- ✅ Baixa/constrói imagens (primeira vez: ~5-10 minutos)
- ✅ Sobe PostgreSQL, Redis, RabbitMQ, Seq
- ✅ Aplica migrations automaticamente no banco
- ✅ Inicia API na porta 8080
- ✅ Inicia Worker processando vídeos

### 2️⃣ Aguardar inicialização

Acompanhe os logs:

```bash
docker-compose logs -f api
```

Quando ver isso, está pronto:
```
✅ Migrations aplicadas com sucesso!
🚀 Iniciando FiapX API...
[INF] FiapX API iniciada com sucesso.
```

### 3️⃣ Verificar Worker

```bash
docker-compose logs -f worker
```

Deve aparecer:
```
[INF] FiapX Worker iniciado e escutando a fila RabbitMQ...
```

---

## 🌐 Acessos

| Serviço | URL | Credenciais |
|---------|-----|-------------|
| **API** | http://localhost:8080 | - |
| **Swagger** | http://localhost:8080/swagger | - |
| **Seq (Logs)** | http://localhost:5341 | - |
| **RabbitMQ Management** | http://localhost:15672 | user: `fiapx`<br>pass: `fiapx_rabbit_2024` |
| **PostgreSQL** | localhost:5432 | db: `fiapx_db`<br>user: `fiapx_user`<br>pass: `fiapx_pass_2024` |

---

## 📝 Testando a API

### 1. Registrar usuário

```bash
curl -X POST http://localhost:8080/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test User",
    "email": "test@fiap.com.br",
    "password": "senha123",
    "confirmPassword": "senha123"
  }'
```

Copie o **token** da resposta.

### 2. Upload de vídeo

```bash
curl -X POST http://localhost:8080/api/videos/upload \
  -H "Authorization: Bearer SEU_TOKEN_AQUI" \
  -F "file=@caminho/do/seu/video.mp4"
```

Copie o **videoId** da resposta.

### 3. Verificar status

```bash
curl -X GET http://localhost:8080/api/videos/VIDEO_ID/status \
  -H "Authorization: Bearer SEU_TOKEN_AQUI"
```

### 4. Download do ZIP

Quando `status` = `"Completed"`:

```bash
curl -X GET http://localhost:8080/api/videos/VIDEO_ID/download \
  -H "Authorization: Bearer SEU_TOKEN_AQUI" \
  --output frames.zip
```

---

## 🔍 Comandos Úteis

### Ver logs de todos os serviços
```bash
docker-compose logs -f
```

### Ver logs de um serviço específico
```bash
docker-compose logs -f api
docker-compose logs -f worker
docker-compose logs -f postgres
```

### Parar tudo
```bash
docker-compose down
```

### Parar e remover volumes (apaga dados)
```bash
docker-compose down -v
```

### Rebuild de uma imagem específica
```bash
docker-compose build api
docker-compose build worker
```

### Acessar container
```bash
docker exec -it fiapx-api bash
docker exec -it fiapx-worker bash
docker exec -it fiapx-postgres psql -U fiapx_user -d fiapx_db
```

### Ver status dos containers
```bash
docker-compose ps
```

---

## 🐛 Troubleshooting

### API não inicia / erro de conexão com PostgreSQL

**Problema:** API tenta conectar antes do PostgreSQL estar pronto.

**Solução:** O entrypoint já tem retry automático. Aguarde uns 30 segundos.

Se persistir:
```bash
docker-compose restart api
```

### Worker não processa vídeos

**Problema:** RabbitMQ ainda não criou a fila.

**Solução:** 
1. Acesse http://localhost:15672 (user: `fiapx`, pass: `fiapx_rabbit_2024`)
2. Vá em "Queues"
3. Deve aparecer `video-processing-queue`

Se não aparecer:
```bash
docker-compose restart worker
```

### Erro "Port already in use"

**Problema:** Porta já está sendo usada (ex: PostgreSQL local rodando).

**Solução:** Pare o serviço local ou mude a porta no `docker-compose.yml`:
```yaml
postgres:
  ports:
    - "5433:5432"  # Mude de 5432 para 5433
```

### FFmpeg não encontrado no Worker

**Problema:** Imagem não instalou FFmpeg.

**Solução:** Rebuild do worker:
```bash
docker-compose build --no-cache worker
docker-compose up -d worker
```

### Logs não aparecem no Seq

**Problema:** Seq ainda não está pronto.

**Solução:** Aguarde 1 minuto e recarregue http://localhost:5341

---

## 🔄 Desenvolvimento

Para desenvolvimento local (sem Docker), use:

```bash
# Subir apenas infraestrutura
docker-compose up -d postgres redis rabbitmq seq

# Rodar API localmente
cd src/FiapX.API
dotnet run

# Rodar Worker localmente (outro terminal)
cd src/FiapX.Worker
dotnet run
```

Lembre-se de usar `appsettings.json` com conexões para `localhost` ao invés dos nomes dos containers.

---

## 📦 Volumes

Os dados persistem nos volumes do Docker:

- `postgres_data` - Banco de dados
- `redis_data` - Cache
- `rabbitmq_data` - Filas e mensagens
- `seq_data` - Logs
- `shared_storage` - Vídeos e frames (compartilhado entre API e Worker)

Para limpar tudo:
```bash
docker-compose down -v
```

---

## ✅ Checklist de Funcionamento

- [ ] `docker-compose up --build` executado sem erros
- [ ] Todos os containers healthy: `docker-compose ps`
- [ ] API responde em http://localhost:8080/swagger
- [ ] Seq mostra logs em http://localhost:5341
- [ ] RabbitMQ Management acessível em http://localhost:15672
- [ ] Registro de usuário funciona
- [ ] Upload de vídeo retorna `videoId`
- [ ] Worker processa vídeo (status muda para `Completed`)
- [ ] Download do ZIP funciona
- [ ] ZIP contém frames PNG extraídos

---

**Está tudo funcionando? Ótimo! Sistema 100% containerizado! 🎉**
