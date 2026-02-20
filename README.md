# 🎓 TechChallenge Fase 5 - FIAP

Sistema de processamento de vídeos com extração de frames utilizando arquitetura distribuída e escalável.

[![CI/CD Pipeline](https://github.com/wesleygyn/TechChallenge-fase-5-FIAP/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/wesleygyn/TechChallenge-fase-5-FIAP/actions)

---

## 📋 Sobre o Projeto

Sistema que permite upload de múltiplos vídeos simultaneamente, processamento assíncrono via worker com RabbitMQ, extração de frames (1 fps) usando FFmpeg, e download do resultado em arquivo ZIP.

### 🎯 Objetivos Técnicos

- ✅ Processar múltiplos vídeos simultaneamente
- ✅ Não perder requisições (filas + persistência)
- ✅ Autenticação JWT
- ✅ Listagem de vídeos por usuário
- ✅ Notificações (Telegram)
- ✅ Arquitetura escalável e resiliente
- ✅ Docker Compose completo
- ✅ CI/CD automatizado
- ✅ Testes unitários
- ✅ Observabilidade

---

## 🏗️ Arquitetura

### Visão Geral

O sistema FiapX segue os princípios de **Clean Architecture** e **Domain-Driven Design (DDD)**, com separação clara de responsabilidades em camadas.

```
src/
├── FiapX.API/              # 🌐 Apresentação (REST API)
├── FiapX.Application/      # 💼 Aplicação (Use Cases)
├── FiapX.Domain/           # 🎯 Domínio (Entidades + Regras)
├── FiapX.Infrastructure/   # 🔧 Infraestrutura (BD, Filas, Storage)
├── FiapX.Shared/           # 📦 Compartilhado (DTOs, Responses)
└── FiapX.Worker/           # ⚙️ Background Processing
```

### 📊 Diagramas C4

Documentação visual completa da arquitetura seguindo o modelo C4:

| Nível | Descrição | Link |
|-------|-----------|------|
| **Nível 1** | Contexto do Sistema | **[Ver Diagrama →](./docs/diagrams/c4-level1-context.md)** |
| **Nível 2** | Containers (API, Worker, DBs) | **[Ver Diagrama →](./docs/diagrams/c4-level2-containers.md)** |
| **Nível 3** | Componentes da API | **[Ver Diagrama →](./docs/diagrams/c4-level3-components.md)** |
| **Sequência** | Fluxo de Processamento | **[Ver Diagrama →](./docs/diagrams/sequence-video-processing.md)** |

### 🔄 Fluxo de Processamento

```
Cliente → API → RabbitMQ → Worker → FFmpeg → Storage → Cliente
          ↓                   ↓
       PostgreSQL          Telegram
```

📖 **[Documentação completa da arquitetura →](./docs/architecture.md)**

---

## 🚀 Stack Tecnológica

| Camada | Tecnologia |
|--------|------------|
| **Framework** | .NET 8 + ASP.NET Core |
| **Banco de Dados** | PostgreSQL 16 + EF Core |
| **Cache** | Redis 7 |
| **Mensageria** | RabbitMQ 3 + MassTransit |
| **Processamento** | FFmpeg |
| **Autenticação** | JWT |
| **Logging** | Serilog + Seq |
| **Métricas** | Prometheus + Grafana |
| **Notificações** | Telegram Bot API |
| **Containerização** | Docker + Docker Compose |
| **CI/CD** | GitHub Actions |
| **Testes** | xUnit + Moq + FluentAssertions |

---

## 📦 Como Executar

### 🐳 Opção 1: Docker (Recomendado)

```bash
# 1. Clonar repositório
git clone https://github.com/wesleygyn/TechChallenge-fase-5-FIAP.git
cd TechChallenge-fase-5-FIAP

# 2. Configurar variáveis de ambiente
cp .env.example .env
# Editar .env com suas credenciais

# 3. Subir containers
docker-compose up --build -d

# 4. Acessar
# API: http://localhost:8080
# Swagger: http://localhost:8080/swagger
# RabbitMQ: http://localhost:15672
# Prometheus: http://localhost:9090
# Grafana: http://localhost:3000
```

📖 **[Guia completo do Docker →](./README-DOCKER.md)**

---

### 💻 Opção 2: Visual Studio (Desenvolvimento)

```bash
# 1. Configurar User Secrets
cd src/FiapX.API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5433;..."
# ... (configurar demais secrets)

# 2. Subir apenas a infraestrutura no Docker
docker-compose up postgres redis rabbitmq -d

# 3. Rodar API e Worker no Visual Studio (F5)
```

📖 **[Guia de User Secrets →](./docs/USER-SECRETS.md)**

---

## 🔐 Segurança

### Desenvolvimento Local
- ✅ User Secrets (.NET) - credenciais fora do código
- ✅ `.env` no `.gitignore`
- ✅ Nenhuma credencial hardcoded

### Docker / Produção
- ✅ Variáveis de ambiente via arquivo `.env`
- ✅ `.env.example` como template (sem credenciais)
- ✅ Secrets gerenciados via Docker Compose

### CI/CD
- ✅ GitHub Secrets para credenciais
- ✅ Build automatizado
- ✅ Testes antes do deploy

**⚠️ Conformidade LGPD:** Todas as credenciais e dados sensíveis são gerenciados de forma segura, seguindo as melhores práticas da indústria.

---

## 🧪 Testes

```bash
# Rodar todos os testes
dotnet test

# Com cobertura
dotnet test /p:CollectCoverage=true

# Relatório de cobertura
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

**Cobertura:** > 70%

Estrutura de testes:
- ✅ **Unit Tests**: UseCases, Services, Domain
- ✅ **Integration Tests**: Consumer, Repositories
- ✅ **Mocks**: Moq + FluentAssertions

---

## 📡 API Endpoints

### Autenticação
```http
POST /api/auth/register
POST /api/auth/login
```

### Vídeos
```http
POST   /api/videos/upload          # Upload de vídeo
GET    /api/videos                 # Listar meus vídeos
GET    /api/videos/{id}/status     # Status do processamento
GET    /api/videos/{id}/download   # Download do ZIP
```

### Monitoramento
```http
GET /health                        # Health check
GET /metrics                       # Métricas Prometheus
```

📖 **Documentação completa:** `/swagger` quando a API estiver rodando

---

## 🔄 CI/CD Pipeline

Pipeline automatizado com GitHub Actions:

1. **Build & Test** - Compila e testa o código
2. **Code Coverage** - Gera relatório de cobertura
3. **Docker Build** - Valida Dockerfiles
4. **Code Quality** - Análise estática

📖 **[Detalhes do CI/CD →](./README-CI-CD.md)**

---

## 📊 Observabilidade

### Métricas Disponíveis

- **API:**
  - Request rate (req/s)
  - Response time (ms)
  - Error rate (%)
  - Active connections

- **Worker:**
  - Events processed/sec
  - Processing time
  - Queue size
  - Failed jobs

### Dashboards

- **Prometheus:** `http://localhost:9090`
- **Grafana:** `http://localhost:3000` (admin/admin)

---

## 🛡️ Resiliência

### Dead Letter Queue (DLQ)
- Mensagens com falha após 3 tentativas vão para DLQ
- Logs detalhados de erros
- Possibilidade de reprocessamento manual

### Retry Policy
- 3 tentativas com backoff exponencial (1s, 4s, 16s)
- Circuit breaker para evitar sobrecarga
- Idempotência garantida (não processa 2x)

### Health Checks
- PostgreSQL, Redis, RabbitMQ
- Auto-restart em caso de falha

---

## 📂 Estrutura do Projeto

```
TechChallenge-fase-5-FIAP/
├── .github/
│   └── workflows/           # GitHub Actions
├── docs/
│   ├── diagrams/           # Diagramas C4 + Sequence
│   ├── architecture.md     # Arquitetura detalhada
│   └── database-scripts.sql
├── src/
│   ├── FiapX.API/          # API REST
│   ├── FiapX.Application/  # Use Cases
│   ├── FiapX.Domain/       # Entidades
│   ├── FiapX.Infrastructure/ # Infraestrutura
│   ├── FiapX.Shared/       # DTOs
│   └── FiapX.Worker/       # Background Worker
├── tests/
│   ├── FiapX.API.Tests/
│   ├── FiapX.Application.Tests/
│   ├── FiapX.Infrastructure.Tests/
│   └── FiapX.Worker.Tests/
├── .env.example            # Template de configuração
├── docker-compose.yml      # Orquestração de containers
└── README.md              # Este arquivo
```

---

## 🛠️ Configuração Avançada

### Telegram Bot (Opcional)

1. Criar bot com [@BotFather](https://t.me/BotFather)
2. Obter token e chat ID
3. Configurar no `.env`:
```env
TELEGRAM_ENABLED=true
TELEGRAM_BOT_TOKEN=seu-token
TELEGRAM_CHAT_ID=seu-chat-id
```

### Prometheus + Grafana

Métricas e dashboards já configurados no `docker-compose.yml`.

---

## 🤝 Contribuindo

Este é um projeto acadêmico da FIAP. Para sugestões:

1. Fork o projeto
2. Crie uma branch (`git checkout -b feature/MinhaFeature`)
3. Commit (`git commit -m 'Add: MinhaFeature'`)
4. Push (`git push origin feature/MinhaFeature`)
5. Abra um Pull Request

---

## 📝 Licença

Projeto acadêmico - FIAP Pós-Graduação em Arquitetura de Software

---

## 👥 Autores

- **Wesley Silva** - [GitHub](https://github.com/wesleygyn)

**Orientação:** FIAP - Fase 5 - Arquitetura de Soluções

---

## 📚 Documentação Adicional

- [Diagramas C4 (Níveis 1-3)](./docs/diagrams/)
- [Diagrama de Sequência](./docs/diagrams/sequence-video-processing.md)
- [Arquitetura Detalhada](./docs/architecture.md)
- [Configuração Docker](./README-DOCKER.md)
- [CI/CD Pipeline](./README-CI-CD.md)
- [Scripts de Banco de Dados](./docs/database-scripts.sql)

---

## 🎯 Roadmap

- [x] Autenticação JWT
- [x] Upload de vídeos
- [x] Processamento assíncrono
- [x] Notificações Telegram
- [x] Docker Compose
- [x] CI/CD Pipeline
- [x] User Secrets
- [x] Diagramas C4
- [x] Dead Letter Queue + Retry
- [x] Observabilidade (Prometheus + Grafana)
- [x] Testes unitários (>70% cobertura)
- [ ] Deploy em cloud (Azure/AWS)

---

**Desenvolvido com 💙 para FIAP - Nota 10 com louvor! 🎓**
