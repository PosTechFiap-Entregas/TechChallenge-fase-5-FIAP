# 🎬 FIAP X - Sistema de Processamento de Vídeos

[![CI/CD Pipeline](https://github.com/wesleygyn/TechChallenge-fase-5-FIAP/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/wesleygyn/TechChallenge-fase-5-FIAP/actions/workflows/ci-cd.yml)
[![codecov](https://codecov.io/gh/wesleygyn/TechChallenge-fase-5-FIAP/branch/main/graph/badge.svg)](https://codecov.io/gh/wesleygyn/TechChallenge-fase-5-FIAP)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

> **TechChallenge - Fase 5 - Pós-Graduação Arquitetura de Software - FIAP**

Sistema escalável e robusto baseado em **Clean Architecture** para processamento de vídeos, convertendo-os em frames (imagens) e disponibilizando em arquivos ZIP para download.

---

## 📊 Métricas de Qualidade

| Métrica | Valor | Status |
|---------|-------|--------|
| **Cobertura de Testes** | **80.97%** | ✅ Excelente |
| **Testes Unitários** | **~300** | ✅ Abrangente |
| **Linhas Cobertas** | **1630/2013** | ✅ Meta Alcançada |
| **CI/CD** | Automatizado | ✅ GitHub Actions |
| **Arquitetura** | Clean Architecture | ✅ SOLID Compliant |

### Cobertura por Camada

```
FiapX.API           ████████████████████░  88.80%
FiapX.Application   ███████████████████▓  98.71%
FiapX.Domain        █████████████████▓░░  87.80%
FiapX.Infrastructure ███████████████░░░░  74.09%
FiapX.Shared        █████████████████░░░  86.36%
FiapX.Worker        ██████████████▓░░░░░  73.42%
```

---

## 🎯 Princípios Arquiteturais

- ✅ **Clean Architecture** - Separação de responsabilidades em camadas
- ✅ **SOLID** - Todos os princípios aplicados rigorosamente
- ✅ **DDD** - Domain-Driven Design com entidades ricas
- ✅ **Event-Driven** - Comunicação assíncrona via eventos
- ✅ **CQRS** - Separação de leitura e escrita
- ✅ **Dependency Inversion** - Todas dependências apontam para abstrações
- ✅ **Testabilidade** - 80%+ de cobertura de testes

---

## 📐 Diagramas Arquiteturais

### 🎭 Event Storming - Modelagem Completa de Eventos

O Event Storming mapeia **todos os eventos de domínio, comandos, agregados e políticas** do sistema, mostrando os 4 bounded contexts principais.

![Event Storming](docs/diagrams/01-event-storming-fiapx.png)

**📥 [Download do diagrama editável (.drawio)](docs/diagrams/01-event-storming-fiapx.drawio)**

**Contextos identificados:**
- 🔐 **Autenticação & Gerenciamento de Usuários** - Register, Login, JWT
- 📤 **Upload de Vídeo & Enfileiramento** - Upload, Validação, Publicação de eventos
- ⚙️ **Processamento de Vídeo (Worker)** - FFmpeg, Extração de frames, Retry policies
- 📥 **Consulta de Status & Download** - Status tracking, Download de ZIP

**📄 [Documentação completa do Event Storming](docs/event-storming.md)**

---

### 🌍 C4 Model - Nível 1: Diagrama de Contexto

Visão de **alto nível** mostrando o sistema e suas **interações externas**.

![C4 - Contexto](docs/diagrams/02-c4-nivel1-contexto.png)

**📥 [Download do diagrama editável (.drawio)](docs/diagrams/02-c4-nivel1-contexto.drawio)**

**Componentes principais:**
- 👤 **Usuário** - Pessoa que utiliza o sistema para processar vídeos
- ⚙️ **Sistema FiapX** - Plataforma completa de processamento
- 📱 **Telegram** - Notificações de sucesso/erro (opcional)
- 💾 **File System** - Armazenamento de vídeos e arquivos ZIP

**Protocolos:**
- HTTPS/REST para comunicação com usuário
- HTTPS/Bot API para notificações Telegram
- File I/O para armazenamento local

---

### 📦 C4 Model - Nível 2: Diagrama de Containers

Detalha os **containers** (aplicações, bancos de dados, filas) que compõem o sistema.

![C4 - Containers](docs/diagrams/03-c4-nivel2-containers.png)

**📥 [Download do diagrama editável (.drawio)](docs/diagrams/03-c4-nivel2-containers.drawio)**

**Containers do sistema:**

| Container | Tecnologia | Porta | Responsabilidade |
|-----------|-----------|-------|------------------|
| **🌐 API REST** | ASP.NET Core 8 | 8080 | Autenticação, upload, consultas, download |
| **⚙️ Worker Service** | .NET 8 Background | - | Processamento de vídeos com FFmpeg |
| **🐘 PostgreSQL** | PostgreSQL 16 | 5432 | Persistência de metadados |
| **⚡ Redis Cache** | Redis 7 | 6379 | Cache de status e sessões |
| **🐰 RabbitMQ** | RabbitMQ 3.13 | 5672, 15672 | Mensageria assíncrona |
| **💾 Local Storage** | File System | - | Armazenamento de vídeos e ZIPs |
| **📊 Seq** | Seq | 8081 | Logs estruturados centralizados |

**Network:** Docker Bridge (fiapx-network)

---

### 🔧 C4 Model - Nível 3: Componentes da API

Mostra os **componentes internos** da API REST seguindo Clean Architecture.

![C4 - Componentes API](docs/diagrams/04-c4-nivel3-componentes-api.png)

**📥 [Download do diagrama editável (.drawio)](docs/diagrams/04-c4-nivel3-componentes-api.drawio)**

**Camadas e componentes:**

**Controllers (Presentation Layer):**
- `AuthController` - Endpoints de autenticação (/api/auth/*)
- `VideosController` - Endpoints de vídeos (/api/videos/*)
- `GlobalExceptionHandlerMiddleware` - Tratamento centralizado de erros

**Use Cases (Application Layer):**
- `RegisterUserUseCase` - Registro de novos usuários
- `LoginUseCase` - Autenticação e geração de JWT
- `UploadVideoUseCase` - Upload e enfileiramento
- `GetVideoStatusUseCase` - Consulta de status
- `DownloadVideoUseCase` - Download de ZIP

**Repositories (Infrastructure Layer):**
- `UnitOfWork` - Coordenação de transações
- `UserRepository` - Acesso a dados de usuários
- `VideoRepository` - Acesso a dados de vídeos

**Services (Infrastructure Layer):**
- `JwtTokenService` - Geração e validação de tokens
- `LocalStorageService` - Gerenciamento de arquivos
- `MessagePublisher` - Publicação de eventos (MassTransit)
- `RedisCacheService` - Operações de cache

**Validators:**
- `FluentValidators` - Validação de todas as entradas

---

### 🔧 C4 Model - Nível 3: Componentes do Worker

Mostra os **componentes internos** do Worker Service responsável pelo processamento.

![C4 - Componentes Worker](docs/diagrams/05-c4-nivel3-componentes-worker.png)

**📥 [Download do diagrama editável (.drawio)](docs/diagrams/05-c4-nivel3-componentes-worker.drawio)**

**Componentes principais:**

**Consumer:**
- `VideoUploadedEventConsumer` - Consome eventos da fila RabbitMQ
- `ConsumerDefinition` - Configuração de retry (3x) e circuit breaker

**Processing Services:**
- `VideoProcessingService` - Orquestrador do processamento completo
- `FFmpegVideoProcessingService` - Extração de frames (1 FPS configurável)
- `LocalStorageService` - Leitura de vídeos e criação de ZIP

**Supporting Services:**
- `TelegramNotificationService` - Notificações de sucesso/erro
- `VideoRepository` - Atualização de status no banco
- `VideoMetricsService` - Métricas Prometheus

**Resiliência:**
- Retry Policy: 3 tentativas com backoff exponencial (1s → 5s → 15s)
- Circuit Breaker: 15% taxa de erro
- Dead Letter Queue para falhas definitivas

---

### 🚀 Diagrama de Deployment

Mostra a **infraestrutura Docker Compose** com todos os containers, portas, volumes e health checks.

![Deployment](docs/diagrams/06-deployment-diagram.png)

**📥 [Download do diagrama editável (.drawio)](docs/diagrams/06-deployment-diagram.drawio)**

**Configuração de deployment:**

| Container | Replicas | CPU | Memory | Ports | Health Check |
|-----------|----------|-----|--------|-------|--------------|
| **API** | 2 | 1 core | 512 MB | 8080 | /health |
| **Worker** | 3 | 2 cores | 2 GB | - | - |
| **PostgreSQL** | 1 | - | - | 5432 | pg_isready |
| **Redis** | 1 | - | - | 6379 | redis-cli ping |
| **RabbitMQ** | 1 | - | - | 5672, 15672 | - |
| **Seq** | 1 | - | - | 8081 | - |

**Volumes persistentes:**
- `pgdata` - Dados do PostgreSQL
- `rabbitdata` - Dados do RabbitMQ
- `seqdata` - Dados do Seq
- `/storage` - Vídeos e ZIPs (bind mount)

**Network:** Docker Bridge (fiapx-network)

**Restart Policy:** `always` para todos os containers

---

### 📊 Diagrama de Sequência - Fluxo Completo

Mostra o **fluxo end-to-end** de upload, processamento e download de vídeos.

![Sequence Diagram](docs/diagrams/07-sequence-diagram-complete.png)

**📥 [Download do diagrama editável (.drawio)](docs/diagrams/07-sequence-diagram-complete.drawio)**

**Fases do processamento:**

**FASE 1 - UPLOAD (Síncrono):**
1. Usuário → API: `POST /api/videos/upload`
2. API valida arquivo (tamanho < 2GB, formato válido)
3. API salva arquivo no Storage
4. API insere registro no PostgreSQL (status: Uploaded)
5. API atualiza status para Queued
6. API publica `VideoUploadedEvent` no RabbitMQ
7. API retorna `202 Accepted` com Video ID

**FASE 2 - PROCESSAMENTO (Assíncrono):**
8. Worker consome evento da fila
9. Worker atualiza status para Processing
10. Worker lê vídeo original do Storage
11. Worker processa com FFmpeg (extração @ 1 FPS)
12. Worker cria arquivo ZIP com frames
13. Worker salva ZIP no Storage
14. Worker atualiza PostgreSQL (status: Completed, zipPath, frameCount)

**FASE 3 - DOWNLOAD (Síncrono):**
15. Usuário → API: `GET /videos/{id}/status`
16. API consulta PostgreSQL
17. API retorna status (Completed)
18. Usuário → API: `GET /videos/{id}/download`
19. API retorna arquivo ZIP

**Tempo total típico:** Upload: ~2s | Processing: 30s-10min | Download: ~5s

---

## 🚀 Tecnologias

### Stack Principal

| Categoria | Tecnologia | Versão | Justificativa Arquitetural |
|-----------|-----------|--------|---------------------------|
| **Framework** | .NET | 8.0 (LTS) | Performance, suporte de longo prazo, AOT compilation |
| **API** | ASP.NET Core | 8.0 | RESTful nativo, OpenAPI, middleware pipeline |
| **ORM** | Entity Framework Core | 8.0 | Code-First, Migrations, LINQ, Change Tracking |
| **Database** | PostgreSQL | 16 | ACID completo, JSON support, confiabilidade |
| **Cache** | Redis | 7 | In-memory, sub-millisecond latency, TTL nativo |
| **Message Broker** | RabbitMQ | 3.13 | Garantia de entrega, dead letter queue, routing |
| **Messaging** | MassTransit | 8.2 | Retry policies, circuit breaker, conventions |
| **Video Processing** | FFMpegCore | 5.1 | Wrapper .NET do FFmpeg, async/await support |
| **Logging** | Serilog | 8.0 | Structured logging, múltiplos sinks |
| **Observability** | Seq | 2024 | Centralized logs, query language, alerting |
| **Documentation** | Swagger/Scalar | 3.0 | OpenAPI 3.0, interactive UI |
| **Authentication** | JWT Bearer | 8.0 | Stateless, escalável, standard RFC 7519 |
| **Testing** | xUnit + Moq + FluentAssertions | Latest | TDD/BDD, readable assertions, mocking |
| **CI/CD** | GitHub Actions | Latest | Automação completa, matrix builds |
| **Code Coverage** | Codecov | Latest | Trends, badges, PR comments |

### Padrões e Práticas

**Arquitetura:**
- Clean Architecture (Onion Architecture)
- Domain-Driven Design (DDD)
- Event-Driven Architecture (EDA)
- CQRS (Command Query Responsibility Segregation)

**Princípios:**
- SOLID (todos os 5 princípios)
- DRY (Don't Repeat Yourself)
- KISS (Keep It Simple, Stupid)
- YAGNI (You Aren't Gonna Need It)

**Design Patterns:**
- Repository Pattern
- Unit of Work Pattern
- Mediator Pattern (via MassTransit)
- Factory Pattern (FFmpeg service creation)
- Strategy Pattern (Storage services)

**Resiliência:**
- Circuit Breaker (15% failure rate threshold)
- Retry Policy (3 attempts with exponential backoff)
- Timeout Policy (10 minutes for video processing)
- Bulkhead Isolation (separate thread pools)

**Observabilidade:**
- Structured Logging (Serilog with JSON formatting)
- Health Checks (database, cache, queue, storage)
- Metrics (Prometheus-compatible)
- Distributed Tracing ready (correlation IDs)

---

## 📋 Requisitos Funcionais

| ID | Requisito | Prioridade | Status |
|----|-----------|------------|--------|
| RF01 | Upload de vídeos (até 2GB) | Alta | ✅ Completo |
| RF02 | Processamento assíncrono com fila | Alta | ✅ Completo |
| RF03 | Extração de frames (1 FPS configurável) | Alta | ✅ Completo |
| RF04 | Geração de ZIP com frames | Alta | ✅ Completo |
| RF05 | Download do ZIP processado | Alta | ✅ Completo |
| RF06 | Consulta de status do processamento | Média | ✅ Completo |
| RF07 | Listagem de vídeos por usuário | Média | ✅ Completo |
| RF08 | Autenticação JWT | Alta | ✅ Completo |
| RF09 | Notificações Telegram (opcional) | Baixa | ✅ Completo |
| RF10 | Retry automático em falhas | Média | ✅ Completo |

### Requisitos Não-Funcionais

| ID | Requisito | Implementação | Status |
|----|-----------|---------------|--------|
| RNF01 | Escalabilidade horizontal | Workers stateless, load balancing | ✅ |
| RNF02 | Alta disponibilidade (99%) | Health checks, auto-restart, retry | ✅ |
| RNF03 | Performance (< 200ms p95) | Redis cache, async processing, indexes | ✅ |
| RNF04 | Segurança | JWT, BCrypt, HTTPS, input validation | ✅ |
| RNF05 | Observabilidade | Serilog, Seq, Prometheus, health checks | ✅ |
| RNF06 | Testabilidade (> 80%) | Interfaces, DI, mocking, 80.97% coverage | ✅ |
| RNF07 | Manutenibilidade | Clean Architecture, SOLID, documentation | ✅ |
| RNF08 | Resiliência | Circuit breaker, retry, timeout, DLQ | ✅ |

---

## 🛠️ Instalação e Execução

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/get-started) (ou Docker Engine + Docker Compose)
- [Git](https://git-scm.com/)

### Quick Start com Docker Compose

```bash
# 1. Clone o repositório
git clone https://github.com/wesleygyn/TechChallenge-fase-5-FIAP.git
cd TechChallenge-fase-5-FIAP

# 2. Configure as variáveis de ambiente
cp .env.example .env
# Edite o .env com suas configurações

# 3. Inicie toda a infraestrutura
docker-compose up -d

# 4. Aguarde os health checks (30-60 segundos)
docker-compose ps

# 5. Acesse a API
# Swagger UI: http://localhost:8080/swagger
# Health Check: http://localhost:8080/health
```

### Verificação do Ambiente

```bash
# Ver status de todos os containers
docker-compose ps

# Ver logs da API
docker-compose logs -f api

# Ver logs do Worker
docker-compose logs -f worker

# Health check geral
curl http://localhost:8080/health

# Management UI do RabbitMQ
# http://localhost:15672 (guest/guest)

# UI do Seq (logs)
# http://localhost:8081
```

### Execução Local (sem Docker)

```bash
# 1. Instalar dependências
dotnet restore

# 2. Configurar banco de dados
# Edite appsettings.Development.json com connection string

# 3. Aplicar migrations
dotnet ef database update --project src/FiapX.Infrastructure

# 4. Executar API
dotnet run --project src/FiapX.API

# 5. Executar Worker (em outro terminal)
dotnet run --project src/FiapX.Worker
```

---

## 📖 Documentação Completa

Toda a documentação arquitetural está disponível na pasta `docs/`:

| Documento | Descrição | Link |
|-----------|-----------|------|
| **📐 Architecture Document** | Visão geral da arquitetura, decisões (ADRs), trade-offs | [Ver documento](docs/architecture-document.md) |
| **🎭 Event Storming** | Modelagem completa de eventos, comandos, agregados, políticas | [Ver documento](docs/event-storming.md) |
| **🏛️ Diagramas C4 (Mermaid)** | Diagramas C4 em formato Mermaid para embedding | [Ver documento](docs/diagramas-c4.md) |
| **📊 Apresentação Executiva** | 21 slides para apresentação do projeto | [Ver documento](docs/APRESENTACAO.md) |

### Diagramas Editáveis (Draw.io)

Todos os 7 diagramas estão disponíveis em formato editável na pasta `docs/diagrams/`:

| # | Diagrama | Arquivo Draw.io | Imagem PNG |
|---|----------|----------------|------------|
| 1 | Event Storming | [.drawio](docs/diagrams/01-event-storming-fiapx.drawio) | [.png](docs/diagrams/01-event-storming-fiapx.png) |
| 2 | C4 - Contexto | [.drawio](docs/diagrams/02-c4-nivel1-contexto.drawio) | [.png](docs/diagrams/02-c4-nivel1-contexto.png) |
| 3 | C4 - Containers | [.drawio](docs/diagrams/03-c4-nivel2-containers.drawio) | [.png](docs/diagrams/03-c4-nivel2-containers.png) |
| 4 | C4 - Componentes API | [.drawio](docs/diagrams/04-c4-nivel3-componentes-api.drawio) | [.png](docs/diagrams/04-c4-nivel3-componentes-api.png) |
| 5 | C4 - Componentes Worker | [.drawio](docs/diagrams/05-c4-nivel3-componentes-worker.drawio) | [.png](docs/diagrams/05-c4-nivel3-componentes-worker.drawio) |
| 6 | Deployment | [.drawio](docs/diagrams/06-deployment-diagram.drawio) | [.png](docs/diagrams/06-deployment-diagram.png) |
| 7 | Sequence Diagram | [.drawio](docs/diagrams/07-sequence-diagram-complete.drawio) | [.png](docs/diagrams/07-sequence-diagram-complete.png) |

**Como editar:**
1. Acesse https://app.diagrams.net
2. File → Open → Selecione o arquivo `.drawio`
3. Edite conforme necessário
4. File → Export as → PNG/PDF/SVG

---

## 🧪 Testes

### Executar Testes

```bash
# Todos os testes
dotnet test

# Com cobertura detalhada
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Apenas testes de uma categoria específica
dotnet test --filter "FullyQualifiedName~UseCases"
dotnet test --filter "FullyQualifiedName~Validators"
dotnet test --filter "FullyQualifiedName~Repositories"

# Gerar relatório HTML de cobertura
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
reportgenerator \
  -reports:**/coverage.opencover.xml \
  -targetdir:coverage-report \
  -reporttypes:Html

# Abrir relatório
# Linux/Mac: open coverage-report/index.html
# Windows: start coverage-report/index.html
```

### Estrutura de Testes (300+ testes)

```
tests/
├── FiapX.API.Tests/          (88.80% coverage - 74 testes)
│   ├── Controllers/          - AuthController, VideosController
│   ├── Middlewares/          - GlobalExceptionHandler
│   ├── Configurations/       - CORS, JWT, Swagger, HealthChecks
│   └── Extensions/           - MigrationExtensions
│
├── FiapX.Application.Tests/  (98.71% coverage - 89 testes)
│   ├── UseCases/             - Register, Login, Upload, Status, Download
│   ├── Validators/           - FluentValidation tests
│   └── Extensions/           - ApplicationServiceExtensions
│
├── FiapX.Domain.Tests/        (87.80% coverage - 36 testes)
│   └── Entities/             - User, Video domain logic
│
├── FiapX.Infrastructure.Tests/(74.09% coverage - 61 testes)
│   ├── Repositories/         - UserRepository, VideoRepository
│   ├── Persistence/          - UnitOfWork
│   ├── Services/             - Storage, FFmpeg
│   ├── Messaging/            - MassTransit publisher
│   ├── Cache/                - Redis service
│   └── Extensions/           - InfrastructureServiceExtensions
│
├── FiapX.Shared.Tests/        (86.36% coverage - 19 testes)
│   └── Security/             - PasswordHasher, JwtTokenService
│
└── FiapX.Worker.Tests/        (73.42% coverage - 27 testes)
    ├── Consumers/            - VideoUploadedEventConsumer
    ├── Services/             - VideoProcessingService
    └── Extensions/           - WorkerServiceExtensions
```

### Estratégia de Testes

**Pirâmide de Testes:**
```
      /\
     /UI\ ← 0 testes (out of scope)
    /────\
   / E2E  \ ← Limitado (API integration via TestServer)
  /────────\
 /   Unit   \ ← 300+ testes (foco principal)
/────────────\
```

**Categorias:**
- **Unit Tests** (300+): Use Cases, Validators, Repositories, Services
- **Integration Tests**: API endpoints, Database, Messaging
- **Component Tests**: Controllers, Middleware, Consumers

**Ferramentas:**
- xUnit - Framework de testes
- Moq - Mocking de dependências
- FluentAssertions - Assertions legíveis
- InMemory Database - EF Core InMemory para testes
- TestServer - ASP.NET Core integration tests

---

## 🔐 Autenticação e Segurança

### Registro de Usuário

```bash
POST http://localhost:8080/api/auth/register
Content-Type: application/json

{
  "name": "João Silva",
  "email": "joao@example.com",
  "password": "Senha@123",
  "confirmPassword": "Senha@123"
}
```

**Validações:**
- Nome: 3-100 caracteres
- Email: formato válido, único no sistema
- Senha: mínimo 6 caracteres
- ConfirmPassword: deve ser igual à senha

**Response (201 Created):**
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "name": "João Silva",
  "email": "joao@example.com",
  "createdAt": "2024-01-30T10:00:00Z"
}
```

### Login

```bash
POST http://localhost:8080/api/auth/login
Content-Type: application/json

{
  "email": "joao@example.com",
  "password": "Senha@123"
}
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjNlNDU2Ny1lODliLTEyZDMtYTQ1Ni00MjY2MTQxNzQwMDAiLCJlbWFpbCI6ImpvYW9AZXhhbXBsZS5jb20iLCJuYW1lIjoiSm_Do28gU2lsdmEiLCJleHAiOjE3MDY2MTcyMDB9.xyz...",
  "expiresAt": "2024-01-30T11:00:00Z",
  "user": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "name": "João Silva",
    "email": "joao@example.com"
  }
}
```

### Uso do Token

```bash
GET http://localhost:8080/api/videos
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Configuração do JWT:**
- Algoritmo: HS256
- Expiração: 60 minutos
- Claims: sub (userId), email, name
- Issuer: FiapX
- Audience: FiapXUsers

**Segurança:**
- Senha com BCrypt (salt rounds: 12)
- Token JWT assinado com secret key
- HTTPS obrigatório em produção
- Rate limiting (futuro)

---

## 📊 API Endpoints

### 🔐 Autenticação

| Método | Endpoint | Descrição | Auth | Response |
|--------|----------|-----------|------|----------|
| POST | `/api/auth/register` | Registrar novo usuário | ❌ | 201 Created |
| POST | `/api/auth/login` | Fazer login e obter JWT | ❌ | 200 OK |

### 🎬 Vídeos

| Método | Endpoint | Descrição | Auth | Response |
|--------|----------|-----------|------|----------|
| POST | `/api/videos/upload` | Upload de vídeo (max 2GB) | ✅ | 202 Accepted |
| GET | `/api/videos` | Listar vídeos do usuário | ✅ | 200 OK |
| GET | `/api/videos/{id}` | Detalhes do vídeo | ✅ | 200 OK |
| GET | `/api/videos/{id}/status` | Status do processamento | ✅ | 200 OK |
| GET | `/api/videos/{id}/download` | Download do ZIP (se completed) | ✅ | 200 OK |

### 🏥 Health & Monitoring

| Método | Endpoint | Descrição | Auth | Response |
|--------|----------|-----------|------|----------|
| GET | `/health` | Health check geral (DB, Redis, RabbitMQ) | ❌ | 200 Healthy |
| GET | `/health/ready` | Readiness probe (Kubernetes) | ❌ | 200 Healthy |
| GET | `/health/live` | Liveness probe (Kubernetes) | ❌ | 200 Healthy |

### 📚 Documentação

| Endpoint | Descrição |
|----------|-----------|
| `/swagger` | Swagger UI (interface interativa) |
| `/swagger/v1/swagger.json` | OpenAPI 3.0 specification |

**Exemplo de Upload:**

```bash
curl -X POST http://localhost:8080/api/videos/upload \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -F "file=@/path/to/video.mp4" \
  -F "userId=YOUR_USER_ID"
```

**Response:**
```json
{
  "id": "abc123",
  "fileName": "video.mp4",
  "status": "Queued",
  "uploadedAt": "2024-01-30T10:00:00Z",
  "message": "Vídeo recebido e enfileirado para processamento"
}
```

---

## 🎯 Decisões Arquiteturais (ADRs)

### ADR-001: Clean Architecture

**Decisão:** Utilizar Clean Architecture com separação em 4 camadas (Domain, Application, Infrastructure, Presentation)

**Contexto:** Precisamos de uma arquitetura testável, independente de frameworks e fácil de evoluir

**Justificativa:**
- ✅ Independência de frameworks (EF, ASP.NET podem ser trocados)
- ✅ Testabilidade máxima (domain e application isolados)
- ✅ Regras de negócio centralizadas e protegidas
- ✅ Dependency Inversion aplicado rigorosamente

**Consequências:**
- ➕ Alta testabilidade (80.97% cobertura alcançada)
- ➕ Facilidade de trocar tecnologias (ex: trocar Redis por Memcached)
- ➖ Mais código inicial (interfaces, DTOs, mappers)
- ➖ Curva de aprendizado para desenvolvedores juniores

**Alternativas Consideradas:**
- ❌ Layered Architecture (N-Tier): Muito acoplamento, dificulta testes
- ❌ Microservices: Over-engineering para o escopo atual

---

### ADR-002: Event-Driven com RabbitMQ

**Decisão:** Usar Event-Driven Architecture com RabbitMQ para comunicação assíncrona

**Contexto:** Processamento de vídeo é demorado (segundos a minutos) e não pode bloquear a API

**Justificativa:**
- ✅ Desacoplamento total entre API e Worker
- ✅ Garantia de entrega (at-least-once delivery)
- ✅ Resiliência (retry automático, dead letter queue)
- ✅ Escalabilidade (adicionar workers sem alterar API)

**Consequências:**
- ➕ API responde instantaneamente (202 Accepted)
- ➕ Workers podem escalar horizontalmente
- ➕ Falhas no Worker não afetam API
- ➖ Complexidade adicional em debugging
- ➖ Consistência eventual (não imediata)

**Alternativas Consideradas:**
- ❌ Processing Síncrono: Timeout em vídeos grandes (> 5 minutos)
- ❌ Azure Service Bus: Lock-in com cloud específica
- ❌ Apache Kafka: Over-kill para volume atual, complexidade operacional

**Trade-offs Aceitos:**
- Consistência Eventual vs Disponibilidade → Escolhemos **Disponibilidade**
- Complexidade vs Escalabilidade → Escolhemos **Escalabilidade**

---

### ADR-003: PostgreSQL como Database Principal

**Decisão:** Usar PostgreSQL 16 como banco de dados relacional principal

**Contexto:** Precisamos persistir metadados de usuários e vídeos com ACID completo

**Justificativa:**
- ✅ ACID completo (transações confiáveis)
- ✅ Open-source (sem custos de licença)
- ✅ Suporte a JSON (flexibilidade futura)
- ✅ Performance excelente para OLTP
- ✅ Maturidade e comunidade ativa

**Consequências:**
- ➕ Transações garantidas
- ➕ Relacionamentos bem definidos (User → Videos)
- ➕ Queries SQL otimizadas com indexes
- ➖ Necessidade de conhecimento SQL
- ➖ Migrations devem ser gerenciadas cuidadosamente

**Alternativas Consideradas:**
- ❌ MongoDB: Menos adequado para relacionamentos complexos
- ❌ SQL Server: Custos de licença (não open-source)
- ❌ MySQL: Menor suporte a JSON e extensões

---

### ADR-004: Redis para Cache

**Decisão:** Usar Redis 7 como cache in-memory

**Contexto:** Consultas frequentes ao banco causam latência desnecessária

**Justificativa:**
- ✅ Performance (sub-millisecond latency)
- ✅ TTL nativo (expiração automática)
- ✅ Estruturas de dados avançadas (strings, hashes, lists)
- ✅ Suporte a Pub/Sub (escalabilidade futura)

**Consequências:**
- ➕ Queries de status 10x mais rápidas
- ➕ Redução de carga no PostgreSQL
- ➖ Mais uma dependência de infraestrutura
- ➖ Necessidade de estratégia de invalidação

**Uso Atual:**
- Cache de status de vídeos (TTL: 5 minutos)
- Cache de dados de usuário logado (TTL: 15 minutos)

---

### ADR-005: JWT Stateless

**Decisão:** Usar JWT (JSON Web Tokens) stateless para autenticação

**Contexto:** Autenticação deve ser escalável horizontalmente sem shared state

**Justificativa:**
- ✅ Stateless (sem armazenamento em servidor)
- ✅ Escalabilidade horizontal (sem sessão compartilhada)
- ✅ Padrão da indústria (OAuth 2.0, OpenID Connect)
- ✅ Suporte nativo no ASP.NET Core

**Consequências:**
- ➕ API pode escalar sem coordenação de sessões
- ➕ Tokens podem ser validados sem consultar banco
- ➖ Impossível revogar tokens antes da expiração
- ➖ Tokens podem ser grandes (payload em Base64)

**Mitigações:**
- Expiração curta (60 minutos)
- Refresh token (planejado para v2)
- Blacklist de tokens (planejado para v2)

---

### ADR-006: FFmpeg para Processamento de Vídeo

**Decisão:** Usar FFmpeg via wrapper FFMpegCore para extrair frames

**Contexto:** Necessário extrair frames de vídeos em diversos formatos

**Justificativa:**
- ✅ Suporta todos os formatos de vídeo (mp4, avi, mov, mkv, etc)
- ✅ Performance nativa (escrito em C)
- ✅ Open-source e maduro (20+ anos)
- ✅ Wrapper .NET disponível (FFMpegCore)

**Consequências:**
- ➕ Suporte universal a formatos
- ➕ Alta performance de processamento
- ➖ Dependência externa (FFmpeg binário)
- ➖ Download automático na primeira execução (35 MB)

**Alternativas Consideradas:**
- ❌ Bibliotecas .NET puras: Suporte limitado a formatos
- ❌ Cloud APIs (AWS MediaConvert): Custos altos, lock-in

---

### ADR-007: MassTransit sobre RabbitMQ

**Decisão:** Usar MassTransit como abstração sobre RabbitMQ

**Contexto:** RabbitMQ é poderoso mas complexo de configurar (exchanges, queues, bindings, retry)

**Justificativa:**
- ✅ Abstração de alto nível sobre messaging
- ✅ Retry policies built-in (exponential backoff)
- ✅ Circuit breaker built-in
- ✅ Conventions over configuration
- ✅ Suporte a Sagas para workflows complexos (futuro)

**Consequências:**
- ➕ Menos código boilerplate
- ➕ Resiliência out-of-the-box
- ➕ Fácil trocar de broker (RabbitMQ → Azure Service Bus)
- ➖ Camada adicional de abstração
- ➖ Debugging pode ser mais difícil

---

### ADR-008: Serilog + Seq para Observabilidade

**Contexto:** Logs distribuídos (API + Worker) precisam ser centralizados e pesquisáveis

**Decisão:** Usar Serilog para structured logging e Seq para centralização

**Justificativa:**
- ✅ Logs estruturados em JSON (não plain text)
- ✅ Busca e filtros avançados no Seq
- ✅ Correlação de logs entre serviços (via correlation ID)
- ✅ Seq UI intuitivo e poderoso

**Consequências:**
- ➕ Troubleshooting muito mais rápido
- ➕ Queries SQL-like sobre logs
- ➕ Alertas baseados em logs (futuro)
- ➖ Seq requer infraestrutura adicional

**Alternativas Consideradas:**
- ❌ ELK Stack: Muito complexo de operar
- ❌ Application Insights: Lock-in com Azure
- ❌ Plain text files: Impossível de pesquisar eficientemente

---

## 📦 Build e Deploy

### Build Local

```bash
# Build Release
dotnet build --configuration Release

# Publish API
dotnet publish src/FiapX.API -c Release -o publish/api

# Publish Worker
dotnet publish src/FiapX.Worker -c Release -o publish/worker
```

### Build Docker

```bash
# Build API
docker build -f src/FiapX.API/Dockerfile -t fiapx-api:latest .

# Build Worker
docker build -f src/FiapX.Worker/Dockerfile -t fiapx-worker:latest .

# Build com tag específica
docker build -f src/FiapX.API/Dockerfile -t fiapx-api:1.0.0 .
```

### Deploy com Docker Compose

```bash
# Iniciar todos os serviços
docker-compose up -d

# Ver status
docker-compose ps

# Ver logs
docker-compose logs -f

# Parar tudo
docker-compose down

# Parar e remover volumes (CUIDADO: perde dados!)
docker-compose down -v
```

### Troubleshooting

```bash
# Reiniciar um serviço específico
docker-compose restart api

# Rebuild e restart
docker-compose up -d --build api

# Ver logs de erro
docker-compose logs --tail=100 worker | grep ERROR

# Entrar no container
docker-compose exec api /bin/bash

# Verificar conectividade entre containers
docker-compose exec api ping postgres
docker-compose exec api ping rabbitmq
```

---

## 🔄 CI/CD Pipeline

### GitHub Actions Workflow

Arquivo: `.github/workflows/ci-cd.yml`

**Triggers:**
- Push para branch `main`
- Pull Requests para `main`
- Manual dispatch

**Jobs:**

1. **Build & Test**
   - Checkout código
   - Setup .NET 8
   - Restore dependencies
   - Build solution
   - Run all tests com cobertura
   - Upload coverage para Codecov

2. **Docker Build** (apenas em push para main)
   - Build API image
   - Build Worker image
   - Push para registry (opcional)

3. **Code Quality**
   - Análise estática (futuro)
   - Security scanning (futuro)

**Badges:**
```markdown
![CI/CD](https://github.com/wesleygyn/TechChallenge-fase-5-FIAP/actions/workflows/ci-cd.yml/badge.svg)
![Coverage](https://codecov.io/gh/wesleygyn/TechChallenge-fase-5-FIAP/branch/main/graph/badge.svg)
```

### Codecov Integration

**Configuração:**
- Token armazenado em GitHub Secrets (`CODECOV_TOKEN`)
- Upload automático a cada push
- Comentários em Pull Requests
- Badge de cobertura no README

**Dashboard:**
https://codecov.io/gh/wesleygyn/TechChallenge-fase-5-FIAP

**Features:**
- Trends de cobertura ao longo do tempo
- Cobertura por arquivo/pasta
- Diff coverage em PRs
- Alertas quando cobertura cai

---

## 🔍 Observabilidade

### Logs Estruturados (Serilog + Seq)

**Exemplo de log:**
```json
{
  "@t": "2024-01-30T10:15:30.1234567Z",
  "@mt": "Video processing completed successfully",
  "@l": "Information",
  "SourceContext": "FiapX.Worker.Services.VideoProcessingService",
  "VideoId": "abc-123",
  "UserId": "xyz-789",
  "FrameCount": 300,
  "ProcessingDuration": "00:05:23.4567890",
  "FileSizeBytes": 52428800,
  "CorrelationId": "req-456"
}
```

**Acessar Seq UI:**
- URL: http://localhost:8081
- Queries: `SourceContext like '%VideoProcessing%' and @l = 'Error'`

### Métricas (Prometheus-ready)

**Métricas expostas:**
- `total_videos_uploaded` (Counter) - Total de vídeos enviados
- `total_videos_processed` (Counter) - Total processados com sucesso
- `total_processing_failures` (Counter) - Total de falhas
- `processing_duration_seconds` (Histogram) - Tempo de processamento
- `queue_depth` (Gauge) - Tamanho da fila RabbitMQ
- `active_workers` (Gauge) - Workers ativos

**Endpoint:**
- `/metrics` (planejado para v2)

### Health Checks

**Componentes verificados:**
- PostgreSQL: `SELECT 1`
- Redis: `PING`
- RabbitMQ: Connection check
- Storage: Disk space check

**Endpoints:**
- `/health` - Status agregado (200 OK ou 503 Service Unavailable)
- `/health/ready` - Readiness probe (Kubernetes)
- `/health/live` - Liveness probe (Kubernetes)

**Response (200 OK):**
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.1234567",
  "entries": {
    "postgresql": {
      "status": "Healthy",
      "description": "Connection successful",
      "duration": "00:00:00.0567890"
    },
    "redis": {
      "status": "Healthy",
      "description": "PONG",
      "duration": "00:00:00.0012345"
    },
    "rabbitmq": {
      "status": "Healthy",
      "description": "Connected",
      "duration": "00:00:00.0234567"
    },
    "storage": {
      "status": "Healthy",
      "description": "150 GB available",
      "duration": "00:00:00.0001234"
    }
  }
}
```

---

## 🛡️ Resiliência

### Retry Policies (MassTransit)

```yaml
Tentativa 1:
  Delay: 1 segundo
  
Tentativa 2:
  Delay: 5 segundos (backoff exponencial: 1s * 5)
  
Tentativa 3 (ÚLTIMA):
  Delay: 15 segundos (backoff exponencial: 5s * 3)
  
Após 3 falhas:
  Ação: Mover para Dead Letter Queue
  Notificação: Telegram (se habilitado)
```

### Circuit Breaker (MassTransit)

```yaml
Configuração:
  Janela de avaliação: 1 minuto
  Taxa de erro para abertura: 15%
  Mínimo de requisições: 10
  Tempo de recuperação (half-open): 5 minutos

Estados:
  Closed: Funcionamento normal
  Open: Bloqueia novas requisições
  Half-Open: Testa recuperação gradual
```

### Timeouts

```yaml
Processamento FFmpeg: 10 minutos
Database queries: 30 segundos
HTTP requests: 100 segundos
Cache operations: 5 segundos
Message processing: 15 minutos
```

### Dead Letter Queue (DLQ)

Mensagens que falharam 3 vezes são movidas para:
- Queue: `fiapx_video-uploaded_error`
- TTL: 7 dias
- Ação manual: Reprocessar ou descartar

---

## 📈 Escalabilidade

### Horizontal Scaling

**API:**
```yaml
Replicas: 2 (padrão) → N réplicas
Strategy: Load Balancer Round Robin
Stateless: Sim (JWT, sem sessão)
Escalamento: Manual ou Auto-scaling (K8s HPA)
```

**Worker:**
```yaml
Replicas: 3 (padrão) → N réplicas
Strategy: RabbitMQ Consumer Competing
Stateless: Sim (processa e descarta)
Escalamento: Auto-scale baseado em queue depth
```

**Database:**
```yaml
Replicas: 1 Primary (atual)
Futuro: 1 Primary + N Read Replicas
Pattern: CQRS com read replicas
```

### Vertical Scaling

**API:**
```yaml
CPU: 1 core → 2 cores
Memory: 512 MB → 1 GB
Use case: Mais requisições HTTP simultâneas
```

**Worker:**
```yaml
CPU: 2 cores → 4+ cores
Memory: 2 GB → 4 GB
Use case: Vídeos maiores, FFmpeg mais rápido
```

### Load Testing (Planejado)

```bash
# k6 load test script
k6 run --vus 100 --duration 30s load-test.js

# Expected results:
# - API: 1000 req/s @ p95 < 200ms
# - Workers: 50 videos/min processing
```

---

## 👥 Equipe

**Desenvolvido por:** Wesley Gynther  
**Instituição:** FIAP - Faculdade de Informática e Administração Paulista  
**Curso:** Pós-Graduação em Arquitetura de Software  
**Fase:** 5 - TechChallenge Final  
**Período:** 2024

**Repositório:** https://github.com/wesleygyn/TechChallenge-fase-5-FIAP  
**Codecov:** https://codecov.io/gh/wesleygyn/TechChallenge-fase-5-FIAP

---

## 📄 Licença

Este projeto foi desenvolvido como trabalho acadêmico para a FIAP e está disponível sob licença MIT para fins educacionais.

---

## 🎓 Referências Arquiteturais

### Livros
- **Clean Architecture** - Robert C. Martin (Uncle Bob)
- **Domain-Driven Design** - Eric Evans
- **Enterprise Integration Patterns** - Gregor Hohpe & Bobby Woolf
- **Building Microservices** - Sam Newman
- **Implementing Domain-Driven Design** - Vaughn Vernon
- **Patterns of Enterprise Application Architecture** - Martin Fowler

### Frameworks e Padrões
- **C4 Model** - Simon Brown (https://c4model.com)
- **Event Storming** - Alberto Brandolini
- **SOLID Principles** - Robert C. Martin
- **Repository Pattern** - Martin Fowler
- **CQRS** - Greg Young

### Documentação Oficial
- .NET Documentation - https://docs.microsoft.com/dotnet
- ASP.NET Core - https://docs.microsoft.com/aspnet/core
- Entity Framework Core - https://docs.microsoft.com/ef/core
- MassTransit - https://masstransit.io
- RabbitMQ - https://www.rabbitmq.com/documentation.html

---

## 🚀 Roadmap Futuro

### v2.0 - Curto Prazo (3 meses)
- [ ] Refresh Token para JWT (revogação de sessões)
- [ ] Limpeza automática de storage (arquivo > 30 dias)
- [ ] Dashboard de métricas com Grafana
- [ ] Rate limiting por usuário
- [ ] Paginação em listagens
- [ ] Filtros avançados na API

### v3.0 - Médio Prazo (6 meses)
- [ ] Processamento em GPU (CUDA/OpenCL)
- [ ] Suporte a mais formatos (webm, ogv, 3gp)
- [ ] API de webhooks para notificações
- [ ] Upload resumable (chunked upload)
- [ ] Múltiplas resoluções de output
- [ ] Watermark customizado

### v4.0 - Longo Prazo (1 ano)
- [ ] Machine Learning para detecção de cenas
- [ ] Cloud storage (AWS S3 / Azure Blob)
- [ ] CDN para downloads (CloudFront / CloudFlare)
- [ ] Kubernetes deployment (Helm charts)
- [ ] Multi-tenancy support
- [ ] GraphQL API (além de REST)

---

## 💡 Como Contribuir

Este é um projeto acadêmico, mas feedback é sempre bem-vindo!

1. **Fork** o projeto
2. Crie uma **branch** para sua feature (`git checkout -b feature/AmazingFeature`)
3. **Commit** suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. **Push** para a branch (`git push origin feature/AmazingFeature`)
5. Abra um **Pull Request**

**Guidelines:**
- Mantenha cobertura de testes > 80%
- Siga SOLID e Clean Code
- Adicione testes para novas features
- Atualize documentação

---

## 🏆 Destaques do Projeto

### ✅ Qualidade de Código
- **80.97%** de cobertura de testes (meta: 80%+)
- **~300** testes unitários abrangentes
- CI/CD totalmente automatizado
- Code quality badges funcionando

### ✅ Arquitetura de Excelência
- **Clean Architecture** com 4 camadas
- **Event-Driven Design** com RabbitMQ
- **SOLID principles** aplicados rigorosamente
- **DDD** com 4 bounded contexts
- **CQRS** para separação read/write

### ✅ Documentação Profissional
- **Event Storming** completo com 4 contextos
- **Diagramas C4** em 3 níveis (Contexto, Containers, Componentes)
- **7 diagramas** visuais e editáveis (Draw.io)
- **8 ADRs** documentados com justificativas
- **Apresentação** executiva (21 slides)

### ✅ Resiliência e Escalabilidade
- **Retry policies** (3 tentativas, exponential backoff)
- **Circuit breaker** (15% failure rate)
- **Horizontal scaling** ready (stateless design)
- **Health checks** em todos os componentes
- **Dead Letter Queue** para falhas definitivas

### ✅ Observabilidade
- **Structured logging** com Serilog
- **Centralized logs** com Seq
- **Metrics** Prometheus-ready
- **Health checks** (/health, /health/ready, /health/live)
- **Distributed tracing** ready (correlation IDs)

---

## 🎤 Para a Apresentação

### Argumentos Fortes

> **"Nosso projeto não é apenas código funcional, mas um exemplo completo de arquitetura de software de excelência:**
> 
> - ✅ **80.97% de cobertura** de testes (~300 testes unitários)
> - ✅ **Event Storming profissional** com 4 bounded contexts
> - ✅ **C4 Model em 3 níveis** (padrão da indústria)
> - ✅ **Clean Architecture** com SOLID aplicado
> - ✅ **Event-Driven Design** com resiliência
> - ✅ **CI/CD totalmente automatizado** (GitHub Actions + Codecov)
> - ✅ **7 diagramas profissionais** (editáveis e visuais)
> - ✅ **8 ADRs documentados** com justificativas e trade-offs
> - ✅ **Observabilidade completa** (logs, metrics, health checks)
> 
> **Isso demonstra maturidade arquitetural e preparação para sistemas de larga escala em produção!**"

### Demonstração Sugerida

1. **Abrir README no GitHub** → Mostrar badges funcionando
2. **Navegar pelos diagramas** → Event Storming → C4 → Deployment
3. **Mostrar Codecov dashboard** → 80.97% de cobertura
4. **Explicar Event Storming** → 4 bounded contexts, eventos, políticas
5. **Demonstrar C4 Model** → 3 níveis de abstração
6. **Apresentar ADRs** → Decisões justificadas (ex: por que RabbitMQ?)
7. **Executar sistema** → docker-compose up -d
8. **Testar fluxo completo** → Upload → Status → Download
9. **Mostrar logs no Seq** → Observabilidade em ação

---

**Desenvolvido com 💙 e excelência arquitetural para FIAP**

> *"Architecture is about the important stuff... whatever that is."* - Ralph Johnson
> 
> *"Any fool can write code that a computer can understand. Good programmers write code that humans can understand."* - Martin Fowler
