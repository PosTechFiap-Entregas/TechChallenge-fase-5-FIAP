# 📘 Manual de Instalação, Configuração e Uso - FiapX

## 📋 Índice

1. [Pré-requisitos](#pré-requisitos)
2. [Configuração Inicial](#configuração-inicial)
3. [Instalação com Docker](#instalação-com-docker)
4. [Configuração de Variáveis de Ambiente](#configuração-de-variáveis-de-ambiente)
5. [Verificação de Infraestrutura](#verificação-de-infraestrutura)
6. [Uso do Sistema](#uso-do-sistema)
7. [Observabilidade e Monitoramento](#observabilidade-e-monitoramento)
8. [Troubleshooting](#troubleshooting)

---

## 🔧 Pré-requisitos

### Software Necessário

| Software | Versão Mínima | Download | Obrigatório |
|----------|---------------|----------|-------------|
| **Docker Desktop** | 20.10+ | [Download](https://www.docker.com/products/docker-desktop) | ✅ Sim |
| **Docker Compose** | 2.0+ | Incluído no Docker Desktop | ✅ Sim |
| **Git** | 2.30+ | [Download](https://git-scm.com/) | ✅ Sim |
| **.NET 8 SDK** | 8.0+ | [Download](https://dotnet.microsoft.com/download/dotnet/8.0) | ⚠️ Opcional* |
| **PowerShell** | 7.0+ | [Download](https://github.com/PowerShell/PowerShell) | ⚠️ Windows |

\* Necessário apenas se for executar fora do Docker

### Requisitos de Sistema

- **CPU**: 4 cores (recomendado 8 cores para melhor performance)
- **RAM**: 8 GB (recomendado 16 GB)
- **Disco**: 20 GB livres
- **SO**: Windows 10/11, macOS 11+, Ubuntu 20.04+

---

## ⚙️ Configuração Inicial

### 1️⃣ Clone do Repositório

```bash
# Clone o repositório
git clone https://github.com/wesleygyn/TechChallenge-fase-5-FIAP.git

# Entre na pasta do projeto
cd TechChallenge-fase-5-FIAP

# Verifique que está na branch main
git branch
```

### 2️⃣ Estrutura de Pastas

Verifique que a estrutura está correta:

```bash
# Linux/macOS
tree -L 2

# Windows (PowerShell)
Get-ChildItem -Recurse -Depth 2
```

**Estrutura esperada:**
```
TechChallenge-fase-5-FIAP/
├── docker/              # Dockerfiles
├── docs/                # Documentação
├── src/                 # Código-fonte
├── tests/               # Testes
├── .env.example         # Exemplo de variáveis
├── docker-compose.yml   # Orquestração
└── README.md
```

---

## 🔐 Configuração de Variáveis de Ambiente

### 1️⃣ Criar Arquivo .env

```bash
# Copiar arquivo de exemplo
cp .env.example .env

# Editar arquivo
# Windows: notepad .env
# Linux/macOS: nano .env
```

### 2️⃣ Configurações Obrigatórias

Edite o arquivo `.env` com as seguintes configurações:

```env
# ========================================
# CONFIGURAÇÕES OBRIGATÓRIAS
# ========================================

# PostgreSQL Database
POSTGRES_DB=fiapx_db
POSTGRES_USER=fiapx_user
POSTGRES_PASSWORD=SuaSenhaSegura123!  # ⚠️ ALTERE ESTA SENHA!

# Connection String PostgreSQL
ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=fiapx_db;Username=fiapx_user;Password=SuaSenhaSegura123!

# RabbitMQ
RABBITMQ_DEFAULT_USER=fiapx
RABBITMQ_DEFAULT_PASS=RabbitSenha456!  # ⚠️ ALTERE ESTA SENHA!

# RabbitMQ Connection String
MessageBroker__Host=rabbitmq
MessageBroker__Username=fiapx
MessageBroker__Password=RabbitSenha456!

# JWT Authentication
JWT__Secret=SuaChaveSecretaSuperSeguraDeNoMinimo32Caracteres123!  # ⚠️ ALTERE!
JWT__Issuer=FiapX
JWT__Audience=FiapXUsers
JWT__ExpirationMinutes=60

# Redis Cache
Redis__ConnectionString=redis:6379

# Storage (File System)
Storage__BasePath=/storage
Storage__UploadsFolder=uploads
Storage__OutputsFolder=outputs
Storage__TempFolder=temp
Storage__MaxFileSizeBytes=2147483648  # 2GB

# FFmpeg
FFmpeg__FrameRate=1
FFmpeg__OutputFormat=jpg
FFmpeg__Quality=high

# ========================================
# CONFIGURAÇÕES OPCIONAIS
# ========================================

# Telegram Notifications (OPCIONAL)
Telegram__Enabled=false
Telegram__BotToken=SEU_BOT_TOKEN_AQUI  # Só necessário se Enabled=true
Telegram__ChatId=SEU_CHAT_ID_AQUI      # Só necessário se Enabled=true

# Seq Logs
Seq__ServerUrl=http://seq:5341

# Serilog
Serilog__MinimumLevel__Default=Information
Serilog__MinimumLevel__Override__Microsoft=Warning
Serilog__MinimumLevel__Override__System=Warning
```

### 3️⃣ Configurar Secrets (Opcional - PowerShell)

Se você tem o arquivo `setup-secrets.ps1`:

```powershell
# Windows PowerShell (como Administrador)
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# Execute o script
.\setup-secrets.ps1
```

**⚠️ ATENÇÃO:** Nunca commite o arquivo `.env` com senhas reais!

---

## 🐳 Instalação com Docker

### 1️⃣ Verificar Docker

```bash
# Verificar versão do Docker
docker --version
# Esperado: Docker version 20.10+ ou superior

# Verificar Docker Compose
docker-compose --version
# Esperado: Docker Compose version 2.0+ ou superior

# Verificar se Docker está rodando
docker ps
# Se aparecer erro, inicie o Docker Desktop
```

### 2️⃣ Build das Imagens

```bash
# Build de todas as imagens (primeira vez)
docker-compose build

# Acompanhar o progresso
# Isso pode demorar 5-10 minutos na primeira vez
```

### 3️⃣ Iniciar Containers

```bash
# Iniciar todos os serviços em background
docker-compose up -d

# Ver logs em tempo real (opcional)
docker-compose logs -f

# Parar de ver logs: Ctrl+C
```

### 4️⃣ Verificar Status dos Containers

```bash
# Listar containers em execução
docker-compose ps

# Saída esperada:
# NAME                    STATUS              PORTS
# fiapx-api               Up 30 seconds       0.0.0.0:8080->8080/tcp
# fiapx-worker            Up 30 seconds
# postgres                Up 30 seconds       0.0.0.0:5432->5432/tcp
# redis                   Up 30 seconds       0.0.0.0:6379->6379/tcp
# rabbitmq                Up 30 seconds       0.0.0.0:5672->5672/tcp, 0.0.0.0:15672->15672/tcp
# seq                     Up 30 seconds       0.0.0.0:8081->80/tcp
```

### 5️⃣ Aguardar Health Checks

```bash
# Aguardar 30-60 segundos para os health checks

# Verificar health da API
curl http://localhost:8080/health

# Saída esperada:
# {"status":"Healthy","totalDuration":"00:00:00.1234567"}
```

---

## ✅ Verificação de Infraestrutura

### 1️⃣ PostgreSQL

```bash
# Conectar ao PostgreSQL
docker-compose exec postgres psql -U fiapx_user -d fiapx_db

# Verificar tabelas
\dt

# Sair
\q

# Saída esperada: tabelas Users e Videos
```

### 2️⃣ Redis

```bash
# Conectar ao Redis
docker-compose exec redis redis-cli

# Testar conexão
PING
# Esperado: PONG

# Ver estatísticas
INFO stats

# Sair
exit
```

### 3️⃣ RabbitMQ

```bash
# Acessar Management UI
# URL: http://localhost:15672
# User: fiapx
# Password: (sua senha do .env)

# Via CLI
docker-compose exec rabbitmq rabbitmqctl status

# Listar filas
docker-compose exec rabbitmq rabbitmqctl list_queues
```

### 4️⃣ Seq (Logs)

```bash
# Acessar Seq UI
# URL: http://localhost:8081

# Verificar se logs estão chegando
# Deve aparecer eventos de inicialização da API e Worker
```

### 5️⃣ API REST

```bash
# Swagger UI
# URL: http://localhost:8080/swagger

# Health Check
curl http://localhost:8080/health

# Saída esperada:
{
  "status": "Healthy",
  "checks": {
    "postgresql": "Healthy",
    "redis": "Healthy",
    "rabbitmq": "Healthy"
  }
}
```

---

## 🎯 Uso do Sistema

### 1️⃣ Registro de Usuário

**Endpoint:** `POST http://localhost:8080/api/auth/register`

```bash
curl -X POST http://localhost:8080/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "name": "João Silva",
    "email": "joao@example.com",
    "password": "Senha@123",
    "confirmPassword": "Senha@123"
  }'
```

**Response (201 Created):**
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "name": "João Silva",
  "email": "joao@example.com",
  "createdAt": "2024-01-30T10:00:00Z"
}
```

### 2️⃣ Login

**Endpoint:** `POST http://localhost:8080/api/auth/login`

```bash
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "joao@example.com",
    "password": "Senha@123"
  }'
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-01-30T11:00:00Z",
  "user": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "name": "João Silva",
    "email": "joao@example.com"
  }
}
```

**⚠️ Salve o token!** Você usará em todas as próximas requisições.

### 3️⃣ Upload de Vídeo

**Endpoint:** `POST http://localhost:8080/api/videos/upload`

```bash
# Substitua YOUR_JWT_TOKEN e YOUR_USER_ID
curl -X POST http://localhost:8080/api/videos/upload \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -F "file=@/path/to/video.mp4" \
  -F "userId=YOUR_USER_ID"
```

**Response (202 Accepted):**
```json
{
  "id": "abc-123",
  "fileName": "video.mp4",
  "status": "Queued",
  "uploadedAt": "2024-01-30T10:05:00Z",
  "message": "Vídeo recebido e enfileirado para processamento"
}
```

### 4️⃣ Consultar Status

**Endpoint:** `GET http://localhost:8080/api/videos/{id}/status`

```bash
curl -X GET http://localhost:8080/api/videos/abc-123/status \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**Possíveis Status:**
- `Uploaded` - Vídeo enviado, aguardando
- `Queued` - Na fila para processamento
- `Processing` - Sendo processado pelo Worker
- `Completed` - Processamento concluído, pronto para download
- `Failed` - Erro no processamento

**Response (200 OK - Completed):**
```json
{
  "id": "abc-123",
  "fileName": "video.mp4",
  "status": "Completed",
  "frameCount": 300,
  "fileSizeBytes": 52428800,
  "uploadedAt": "2024-01-30T10:05:00Z",
  "processedAt": "2024-01-30T10:10:23Z",
  "processingDuration": "00:05:23",
  "zipPath": "/storage/outputs/abc-123_frames.zip"
}
```

### 5️⃣ Download do ZIP

**Endpoint:** `GET http://localhost:8080/api/videos/{id}/download`

```bash
# Download direto
curl -X GET http://localhost:8080/api/videos/abc-123/download \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  --output video_frames.zip

# Ou abra no navegador:
# http://localhost:8080/api/videos/abc-123/download
# (adicione o token no header Authorization)
```

### 6️⃣ Listar Vídeos do Usuário

**Endpoint:** `GET http://localhost:8080/api/videos`

```bash
curl -X GET http://localhost:8080/api/videos \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**Response (200 OK):**
```json
[
  {
    "id": "abc-123",
    "fileName": "video.mp4",
    "status": "Completed",
    "uploadedAt": "2024-01-30T10:05:00Z"
  },
  {
    "id": "def-456",
    "fileName": "outro_video.mp4",
    "status": "Processing",
    "uploadedAt": "2024-01-30T10:15:00Z"
  }
]
```

---

## 📊 Observabilidade e Monitoramento

### 1️⃣ Seq - Logs Centralizados

**URL:** http://localhost:8081

**Como usar:**

1. Abra http://localhost:8081 no navegador
2. Sem senha necessária (ambiente local)
3. **Queries úteis:**

```sql
-- Ver apenas erros
@Level = 'Error'

-- Ver logs de processamento de vídeo
SourceContext like '%VideoProcessing%'

-- Ver logs de um usuário específico
UserId = 'abc-123'

-- Ver logs de hoje com erro
@Timestamp > DateTime.Now.AddDays(-1) and @Level = 'Error'

-- Ver duração de processamento
SourceContext like '%VideoProcessing%' and ProcessingDuration is not null
order by ProcessingDuration desc
```

4. **Filtros rápidos:**
   - Clique em qualquer propriedade para filtrar
   - Use `Ctrl+K` para busca rápida
   - Favorites → Salve queries frequentes

### 2️⃣ RabbitMQ Management

**URL:** http://localhost:15672  
**User:** `fiapx` (configurado no .env)  
**Password:** (sua senha do .env)

**Como monitorar:**

1. **Aba "Queues":**
   - Ver tamanho da fila `video-uploaded`
   - Taxa de mensagens (mensagens/segundo)
   - Ver mensagens pendentes

2. **Aba "Connections":**
   - Ver se API e Worker estão conectados
   - Ver quantos consumers ativos

3. **Métricas importantes:**
   - `Ready` - Mensagens aguardando processamento
   - `Unacked` - Mensagens sendo processadas
   - `Total` - Total de mensagens

4. **Comandos úteis:**

```bash
# Ver filas
docker-compose exec rabbitmq rabbitmqctl list_queues name messages consumers

# Ver conexões
docker-compose exec rabbitmq rabbitmqctl list_connections

# Purgar fila (CUIDADO!)
docker-compose exec rabbitmq rabbitmqctl purge_queue video-uploaded
```

### 3️⃣ Prometheus (Futuro - v2.0)

**⚠️ Ainda não implementado na v1.0**

Quando implementado, estará em: http://localhost:9090

**Métricas planejadas:**
- `total_videos_uploaded` - Total de uploads
- `total_videos_processed` - Total processados
- `processing_duration_seconds` - Histograma de duração
- `total_processing_failures` - Total de falhas

### 4️⃣ Grafana (Futuro - v2.0)

**⚠️ Ainda não implementado na v1.0**

Quando implementado, estará em: http://localhost:3000

**Dashboards planejados:**
- Overview do sistema
- Processamento de vídeos
- Infraestrutura (CPU, RAM, Disco)

### 5️⃣ Logs em Tempo Real

```bash
# Logs da API
docker-compose logs -f api

# Logs do Worker
docker-compose logs -f worker

# Logs de todos os serviços
docker-compose logs -f

# Filtrar por erro
docker-compose logs | grep ERROR

# Últimas 100 linhas
docker-compose logs --tail=100 worker
```

### 6️⃣ Métricas de Containers

```bash
# Ver uso de CPU/RAM de todos os containers
docker stats

# Ver uso de um container específico
docker stats fiapx-api

# Saída:
# CONTAINER ID   NAME         CPU %     MEM USAGE / LIMIT     MEM %
# abc123         fiapx-api    2.5%      256MB / 512MB         50%
# def456         fiapx-worker 15.3%     1.2GB / 2GB           60%
```

---

## 🔧 Troubleshooting

### Problema 1: Containers não iniciam

**Sintomas:**
```bash
docker-compose ps
# Mostra containers em estado "Exit" ou "Restarting"
```

**Soluções:**

```bash
# 1. Ver logs de erro
docker-compose logs api
docker-compose logs worker

# 2. Verificar portas em uso
# Windows
netstat -ano | findstr :8080
netstat -ano | findstr :5432

# Linux/macOS
lsof -i :8080
lsof -i :5432

# 3. Parar todos os containers e recriar
docker-compose down
docker-compose up -d --force-recreate

# 4. Limpar volumes (CUIDADO: perde dados!)
docker-compose down -v
docker-compose up -d
```

### Problema 2: Erro de conexão com PostgreSQL

**Sintomas:**
```
Npgsql.NpgsqlException: Connection refused
```

**Soluções:**

```bash
# 1. Verificar se PostgreSQL está rodando
docker-compose ps postgres

# 2. Testar conexão manual
docker-compose exec postgres psql -U fiapx_user -d fiapx_db

# 3. Verificar variáveis de ambiente
docker-compose exec api printenv | grep POSTGRES

# 4. Recriar banco
docker-compose restart postgres
```

### Problema 3: Vídeo fica em "Processing" indefinidamente

**Sintomas:**
- Status nunca muda de `Processing`
- Worker não loga nada

**Soluções:**

```bash
# 1. Ver logs do Worker
docker-compose logs worker | tail -100

# 2. Verificar fila do RabbitMQ
# http://localhost:15672

# 3. Ver se FFmpeg está instalado no container
docker-compose exec worker which ffmpeg

# 4. Tentar reprocessar manualmente
# (Isso requer acesso direto ao banco)

# 5. Reiniciar Worker
docker-compose restart worker
```

### Problema 4: "Unauthorized" ao fazer requests

**Sintomas:**
```json
{
  "status": 401,
  "title": "Unauthorized"
}
```

**Soluções:**

1. **Verificar se o token está correto:**
```bash
# Token JWT tem 3 partes separadas por ponto
# Exemplo: eyJhbGci.eyJzdWI.SflKxwRJ

# Verificar se não expirou (60 minutos padrão)
```

2. **Fazer login novamente:**
```bash
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"joao@example.com","password":"Senha@123"}'
```

3. **Verificar header:**
```bash
# Correto:
Authorization: Bearer eyJhbGci...

# Errado:
Authorization: eyJhbGci...  # Falta "Bearer "
```

### Problema 5: Download retorna 404

**Sintomas:**
```
GET /api/videos/abc-123/download → 404 Not Found
```

**Soluções:**

1. **Verificar se vídeo existe:**
```bash
curl -X GET http://localhost:8080/api/videos/abc-123/status \
  -H "Authorization: Bearer YOUR_TOKEN"
```

2. **Verificar se status é "Completed":**
```json
{
  "status": "Completed"  // ✅ OK para download
  // "status": "Processing"  // ❌ Ainda não pode baixar
}
```

3. **Verificar se arquivo ZIP existe:**
```bash
docker-compose exec api ls -la /storage/outputs/
```

### Problema 6: Seq não mostra logs

**Sintomas:**
- Seq UI vazio
- Nenhum log aparece

**Soluções:**

```bash
# 1. Verificar se Seq está rodando
docker-compose ps seq

# 2. Ver logs do Seq
docker-compose logs seq

# 3. Verificar conexão da API com Seq
docker-compose exec api printenv | grep Seq

# 4. Reiniciar Seq
docker-compose restart seq

# 5. Testar manualmente
docker-compose exec api curl http://seq:5341/api
```

### Problema 7: RabbitMQ Management UI inacessível

**Sintomas:**
- http://localhost:15672 não abre
- Connection refused

**Soluções:**

```bash
# 1. Verificar se RabbitMQ está rodando
docker-compose ps rabbitmq

# 2. Verificar porta
docker-compose exec rabbitmq netstat -tlnp | grep 15672

# 3. Verificar se Management Plugin está habilitado
docker-compose exec rabbitmq rabbitmq-plugins list

# 4. Habilitar plugin (se necessário)
docker-compose exec rabbitmq rabbitmq-plugins enable rabbitmq_management

# 5. Reiniciar RabbitMQ
docker-compose restart rabbitmq
```

---

## 🆘 Comandos Úteis de Manutenção

### Parar Sistema

```bash
# Parar todos os containers
docker-compose stop

# Parar e remover containers
docker-compose down

# Parar, remover containers e volumes (PERDE DADOS!)
docker-compose down -v
```

### Reiniciar Sistema

```bash
# Reiniciar tudo
docker-compose restart

# Reiniciar um serviço específico
docker-compose restart api
docker-compose restart worker
```

### Limpar Sistema

```bash
# Limpar containers parados
docker container prune -f

# Limpar imagens não usadas
docker image prune -a -f

# Limpar volumes não usados (CUIDADO!)
docker volume prune -f

# Limpar tudo (MUITO CUIDADO!)
docker system prune -a --volumes -f
```

### Ver Recursos

```bash
# Uso de disco
docker system df

# Containers em execução
docker ps

# Todos os containers
docker ps -a

# Volumes
docker volume ls
```

---

## 📚 Próximos Passos

Após configurar tudo:

1. ✅ Teste o fluxo completo (registro → login → upload → status → download)
2. ✅ Explore o Seq para ver logs estruturados
3. ✅ Monitore filas no RabbitMQ
4. ✅ Teste diferentes formatos de vídeo
5. ✅ Configure notificações Telegram (opcional)
6. ✅ Leia a documentação em `docs/`

---

## 🎓 Recursos Adicionais

- [📐 Documento de Arquitetura](docs/architecture-document.md)
- [🎭 Event Storming](docs/event-storming.md)
- [📊 Apresentação Executiva](docs/APRESENTACAO.md)
- [🏛️ Diagramas C4](docs/diagramas-c4.md)
- [💾 Database Scripts](docs/database-scripts.sql)

---

**Manual desenvolvido para FIAP - TechChallenge Fase 5** 📘
