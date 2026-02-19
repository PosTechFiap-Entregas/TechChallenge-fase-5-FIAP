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
- ✅ Arquitetura escalável
- ✅ Docker Compose completo
- ✅ CI/CD automatizado
- ✅ Testes unitários

---

## 🏗️ Arquitetura

### Clean Architecture + DDD

```
src/
├── FiapX.API/              # Apresentação (REST API)
├── FiapX.Application/      # Casos de Uso + Events
├── FiapX.Domain/           # Entidades + Regras de Negócio
├── FiapX.Infrastructure/   # Persistência + Serviços Externos
├── FiapX.Shared/           # DTOs + Responses
└── FiapX.Worker/           # Background Processing
```

### 🔄 Fluxo de Processamento

```
Cliente → API → RabbitMQ → Worker → FFmpeg → Storage → Cliente
          ↓                   ↓
       PostgreSQL          Telegram
```

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
| **Notificações** | Telegram Bot API |
| **Containerização** | Docker + Docker Compose |
| **CI/CD** | GitHub Actions |
| **Testes** | xUnit + Moq |

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
```

📖 **[Guia completo do Docker →](./README-DOCKER.md)**

---

### 💻 Opção 2: Visual Studio (Desenvolvimento)

```bash
# 1. Configurar User Secrets
cd src/FiapX.API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5433;Database=fiapx_db;Username=user;Password=pass"
# ... (configurar demais secrets)

cd ../FiapX.Worker
dotnet user-secrets init
# ... (repetir configurações)

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
```

Estrutura de testes:
- ✅ **Unit Tests**: UseCases, Services, Domain
- ✅ **Integration Tests**: API Endpoints, Database
- ✅ **Consumer Tests**: Event Handlers

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

### Health Check
```http
GET /health
```

📖 **Documentação completa:** `/swagger` quando a API estiver rodando

---

## 🔄 CI/CD Pipeline

Pipeline automatizado com GitHub Actions:

1. **Build & Test** - Compila e testa o código
2. **Docker Build** - Valida Dockerfiles
3. **Code Quality** - Análise estática

📖 **[Detalhes do CI/CD →](./README-CI-CD.md)**

---

## 📂 Estrutura do Projeto

```
TechChallenge-fase-5-FIAP/
├── .github/
│   └── workflows/           # GitHub Actions
├── docker/                  # (deprecated - usar src/*/Dockerfile)
├── docs/
│   ├── architecture.md      # Arquitetura detalhada
│   └── database-scripts.sql # Scripts SQL
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

### Seq Logging (Opcional)

Descomentar o serviço `seq` no `docker-compose.yml` para habilitar dashboard de logs.

---

## 📊 Monitoramento

- **Health Checks**: `/health` (API)
- **RabbitMQ Management**: `http://localhost:15672`
- **Seq Logs**: `http://localhost:5341` (se habilitado)

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
- [ ] Testes completos (90%+ cobertura)
- [ ] Deploy em cloud (Azure/AWS)
- [ ] Monitoramento com Application Insights

---

**Desenvolvido com 💙 para FIAP**
