# 🎬 FIAP X - Sistema de Processamento de Vídeos

**TechChallenge - Fase 5 - Pós-Graduação Arquitetura de Software**

Sistema escalável e robusto para processamento de vídeos, convertendo-os em frames (imagens) e disponibilizando em arquivos ZIP para download.

---

## 🏗️ Arquitetura

Sistema baseado em **Clean Architecture** com as seguintes camadas:

- **FiapX.API**: API REST (ASP.NET Core 8)
- **FiapX.Worker**: Processador de vídeos (Background Service)
- **FiapX.Application**: Casos de uso e lógica de negócio
- **FiapX.Domain**: Entidades e interfaces do domínio
- **FiapX.Infrastructure**: Persistência, mensageria e serviços externos
- **FiapX.Shared**: Utilitários compartilhados

### Diagrama de Arquitetura

```
┌─────────────┐      ┌──────────────┐      ┌─────────────┐
│   Cliente   │─────▶│   API REST   │─────▶│  RabbitMQ   │
│  (Upload)   │      │  (ASP.NET 8) │      │ (Mensageria)│
└─────────────┘      └──────────────┘      └─────────────┘
                            │                      │
                            ▼                      ▼
                     ┌──────────────┐      ┌─────────────┐
                     │  PostgreSQL  │      │   Worker    │
                     │ (Metadados)  │◀─────│  (FFmpeg)   │
                     └──────────────┘      └─────────────┘
                            │                      │
                            ▼                      ▼
                     ┌──────────────┐      ┌─────────────┐
                     │    Redis     │      │  Storage    │
                     │   (Cache)    │      │  (Arquivos) │
                     └──────────────┘      └─────────────┘
```

---

## 🚀 Tecnologias

| Categoria | Tecnologia |
|-----------|------------|
| **Framework** | .NET 8 (LTS) |
| **API** | ASP.NET Core Web API |
| **ORM** | Entity Framework Core 8 |
| **Database** | PostgreSQL 16 |
| **Cache** | Redis 7 |
| **Mensageria** | RabbitMQ 3.13 + MassTransit |
| **Processamento** | FFMpegCore |
| **Logs** | Serilog + Seq |
| **Docs** | Swagger/Scalar |
| **Notificações** | Telegram Bot API |
| **Containers** | Docker + Docker Compose |
| **Testes** | xUnit + FluentAssertions + Moq |

---

## 📋 Requisitos Funcionais

✅ Processar múltiplos vídeos simultaneamente  
✅ Não perder requisições em picos de carga  
✅ Sistema protegido por autenticação JWT  
✅ Listagem de status dos vídeos por usuário  
✅ Notificação em caso de erro (Telegram - opcional)  
✅ Persistência de dados  
✅ Arquitetura escalável  
✅ CI/CD com GitHub Actions  
✅ Testes automatizados  

---

## 🛠️ Instalação e Execução

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/get-started) e Docker Compose
- [Git](https://git-scm.com/)

### Passos

1. **Clone o repositório**

```bash
git clone https://github.com/seu-usuario/TechChallenge-Fase5.git
cd TechChallenge-Fase5
```

2. **Configure as variáveis de ambiente**

```bash
cp .env.example .env
# Edite o arquivo .env conforme necessário
```

3. **Inicie a infraestrutura com Docker Compose**

```bash
docker-compose up -d
```

4. **Execute as migrations**

```bash
cd src/FiapX.API
dotnet ef database update
```

5. **Acesse a aplicação**

- **API**: http://localhost:8080
- **Swagger**: http://localhost:8080/swagger
- **RabbitMQ Management**: http://localhost:15672 (user: fiapx, pass: fiapx_rabbit_2024)
- **Seq (Logs)**: http://localhost:8081

---

## 📖 Documentação

Consulte a pasta `docs/` para documentação detalhada:

- [Arquitetura do Sistema](docs/architecture.md)
- [Guia de Desenvolvimento](docs/development-guide.md)
- [Scripts de Banco de Dados](docs/database-scripts.sql)

---

## 🧪 Testes

```bash
# Executar todos os testes
dotnet test

# Executar testes com cobertura
dotnet test /p:CollectCoverage=true
```

---

## 📦 Build e Deploy

### Build Local

```bash
dotnet build TechChallenge-Fase5.sln --configuration Release
```

### Build Docker

```bash
# API
docker build -f docker/Dockerfile.api -t fiapx-api:latest .

# Worker
docker build -f docker/Dockerfile.worker -t fiapx-worker:latest .
```

---

## 🔐 Autenticação

O sistema utiliza JWT (JSON Web Tokens). Para obter um token:

**POST** `/api/auth/login`

```json
{
  "email": "user@example.com",
  "password": "senha123"
}
```

**Response:**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expiresAt": "2024-01-30T23:00:00Z"
}
```

Use o token no header: `Authorization: Bearer {token}`

---

## 📊 Endpoints Principais

### Autenticação
- `POST /api/auth/register` - Registrar usuário
- `POST /api/auth/login` - Fazer login

### Vídeos
- `POST /api/videos/upload` - Upload de vídeo
- `GET /api/videos` - Listar vídeos do usuário
- `GET /api/videos/{id}` - Detalhes do vídeo
- `GET /api/videos/{id}/status` - Status do processamento
- `GET /api/videos/{id}/download` - Download do ZIP

### Health Check
- `GET /health` - Status da aplicação

---

## 👥 Equipe

- **Seu Nome** - Desenvolvedor

---

## 📄 Licença

Este projeto foi desenvolvido como trabalho acadêmico para a FIAP.

---

## 🎯 Roadmap

- [x] Estrutura do projeto
- [x] Configuração Docker
- [ ] Implementação de autenticação
- [ ] Implementação de upload
- [ ] Integração com FFmpeg
- [ ] Sistema de notificações
- [ ] Testes automatizados
- [ ] CI/CD
- [ ] Documentação completa

---

**Desenvolvido com 💙 para FIAP**
