# 🏗️ Arquitetura do Sistema - FIAP X

## Visão Geral

O sistema FIAP X foi projetado seguindo os princípios de **Clean Architecture** e **Domain-Driven Design (DDD)**, garantindo:

- ✅ Separação clara de responsabilidades
- ✅ Independência de frameworks
- ✅ Testabilidade
- ✅ Escalabilidade horizontal
- ✅ Manutenibilidade

---

## 🎯 Padrões Arquiteturais Utilizados

### 1. Clean Architecture

```
┌─────────────────────────────────────────────┐
│              Presentation Layer             │
│         (FiapX.API / FiapX.Worker)          │
├─────────────────────────────────────────────┤
│            Application Layer                │
│   (Use Cases, DTOs, Validators, Mappers)   │
├─────────────────────────────────────────────┤
│              Domain Layer                   │
│    (Entities, Value Objects, Interfaces)   │
├─────────────────────────────────────────────┤
│          Infrastructure Layer               │
│  (EF Core, RabbitMQ, Redis, File Storage)  │
└─────────────────────────────────────────────┘
```

**Dependências:**
- Presentation → Application → Domain
- Infrastructure → Application → Domain
- **Domain não depende de ninguém** (núcleo puro)

---

### 2. CQRS Simplificado

Separação de responsabilidades entre comandos (escrita) e queries (leitura):

**Commands (Escrita):**
- `UploadVideoCommand`
- `ProcessVideoCommand`
- `RegisterUserCommand`

**Queries (Leitura):**
- `GetVideosByUserQuery`
- `GetVideoStatusQuery`
- `GetVideoDetailsQuery`

**Benefícios:**
- Otimizações específicas para leitura vs. escrita
- Cachear queries sem afetar commands
- Escalabilidade independente

---

### 3. Repository Pattern

Abstração do acesso a dados:

```csharp
// Domain/Interfaces
public interface IVideoRepository
{
    Task<Video> GetByIdAsync(Guid id);
    Task<IEnumerable<Video>> GetByUserIdAsync(Guid userId);
    Task AddAsync(Video video);
    Task UpdateAsync(Video video);
}

// Infrastructure/Repositories
public class VideoRepository : IVideoRepository
{
    private readonly AppDbContext _context;
    // Implementação...
}
```

---

### 4. Unit of Work

Gerenciamento de transações:

```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}
```

---

## 🔄 Fluxo de Processamento de Vídeo

### Fluxo Completo

```
1. Cliente faz upload
        ↓
2. API valida e salva vídeo
        ↓
3. API publica mensagem no RabbitMQ
        ↓
4. API retorna ID do job ao cliente
        ↓
5. Worker consome mensagem
        ↓
6. Worker processa vídeo com FFmpeg
        ↓
7. Worker cria arquivo ZIP
        ↓
8. Worker atualiza status no banco
        ↓
9. Worker envia notificação (se habilitado)
        ↓
10. Cliente consulta status/faz download
```

### Fluxo Detalhado com Componentes

```
┌──────────┐
│ Cliente  │
└────┬─────┘
     │ POST /api/videos/upload
     ▼
┌──────────────────────────────┐
│      API Controller          │
│  - Valida tamanho/formato    │
│  - Salva arquivo temporário  │
└────┬─────────────────────────┘
     │
     ▼
┌──────────────────────────────┐
│   UploadVideoUseCase         │
│  - Cria entidade Video       │
│  - Salva no banco            │
└────┬─────────────────────────┘
     │
     ▼
┌──────────────────────────────┐
│    Message Publisher         │
│  - Publica VideoUploadedEvent│
└────┬─────────────────────────┘
     │
     ▼
┌──────────────────────────────┐
│       RabbitMQ Queue         │
│   video-processing-queue     │
└────┬─────────────────────────┘
     │
     ▼
┌──────────────────────────────┐
│    Worker Consumer           │
│  - Consome mensagem          │
└────┬─────────────────────────┘
     │
     ▼
┌──────────────────────────────┐
│  ProcessVideoUseCase         │
│  - Extrai frames (FFmpeg)    │
│  - Cria ZIP                  │
│  - Atualiza status           │
└────┬─────────────────────────┘
     │
     ▼
┌──────────────────────────────┐
│   Notification Service       │
│  - Envia Telegram (opcional) │
└──────────────────────────────┘
```

---

## 🗄️ Modelo de Dados

### Entidades Principais

```csharp
// User
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<Video> Videos { get; set; }
}

// Video
public class Video
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string OriginalFileName { get; set; }
    public string StoragePath { get; set; }
    public long FileSizeBytes { get; set; }
    public VideoStatus Status { get; set; }
    public DateTime UploadedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ZipPath { get; set; }
    public int? FrameCount { get; set; }
    public string? ErrorMessage { get; set; }
    public User User { get; set; }
}

// VideoStatus (Enum)
public enum VideoStatus
{
    Uploaded = 1,
    Queued = 2,
    Processing = 3,
    Completed = 4,
    Failed = 5
}
```

### Diagrama ER

```
┌─────────────────┐         ┌─────────────────────┐
│      User       │1      N │       Video         │
├─────────────────┤◄────────┤─────────────────────┤
│ Id (PK)         │         │ Id (PK)             │
│ Email           │         │ UserId (FK)         │
│ PasswordHash    │         │ OriginalFileName    │
│ CreatedAt       │         │ StoragePath         │
└─────────────────┘         │ FileSizeBytes       │
                            │ Status              │
                            │ UploadedAt          │
                            │ ProcessedAt         │
                            │ ZipPath             │
                            │ FrameCount          │
                            │ ErrorMessage        │
                            └─────────────────────┘
```

---

## 🔐 Segurança

### Autenticação JWT

```
1. Usuário faz login → Recebe JWT token
2. Requisições subsequentes → Header: Authorization: Bearer {token}
3. Middleware valida token em cada request
4. Token expira após 60 minutos (configurável)
```

**Estrutura do Token:**
```json
{
  "sub": "user-id-guid",
  "email": "user@example.com",
  "jti": "token-id",
  "exp": 1735689600,
  "iss": "FiapX.API",
  "aud": "FiapX.Client"
}
```

### Hash de Senhas

- Algoritmo: **BCrypt**
- Work Factor: **12**
- Salt gerado automaticamente

---

## 📨 Mensageria (RabbitMQ + MassTransit)

### Filas e Exchanges

```
Exchange: video-processing-exchange (Topic)
    │
    ├─ Queue: video-processing-queue
    │   └─ Consumers: 3 workers (configurável)
    │
    └─ Queue: video-processing-queue_error (DLQ)
        └─ Mensagens com falha após retries
```

### Eventos

**VideoUploadedEvent:**
```csharp
public record VideoUploadedEvent
{
    public Guid VideoId { get; init; }
    public Guid UserId { get; init; }
    public string StoragePath { get; init; }
    public DateTime UploadedAt { get; init; }
}
```

**VideoProcessedEvent:**
```csharp
public record VideoProcessedEvent
{
    public Guid VideoId { get; init; }
    public bool Success { get; init; }
    public int? FrameCount { get; init; }
    public string? ZipPath { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTime ProcessedAt { get; init; }
}
```

### Retry Policy

```csharp
cfg.UseMessageRetry(r => 
{
    r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
});
```

- **Tentativa 1**: Imediato
- **Tentativa 2**: Aguarda 1s
- **Tentativa 3**: Aguarda 3s
- **Tentativa 4**: Vai para DLQ (Dead Letter Queue)

---

## 💾 Cache (Redis)

### Estratégia de Cache

**Cache de Status de Vídeo:**
- **Key Pattern**: `video:status:{videoId}`
- **TTL**: 5 minutos
- **Invalidação**: Quando status muda

**Cache de Lista de Vídeos:**
- **Key Pattern**: `user:videos:{userId}`
- **TTL**: 2 minutos
- **Invalidação**: Quando novo vídeo é adicionado

```csharp
// Exemplo de uso
var cacheKey = $"video:status:{videoId}";
var cachedStatus = await _cache.GetAsync<VideoStatus>(cacheKey);

if (cachedStatus == null)
{
    cachedStatus = await _repository.GetStatusAsync(videoId);
    await _cache.SetAsync(cacheKey, cachedStatus, TimeSpan.FromMinutes(5));
}
```

---

## 📊 Observabilidade

### Logs Estruturados (Serilog + Seq)

**Níveis de Log:**
- **Trace**: Detalhes técnicos (desenvolvimento)
- **Debug**: Informações de depuração
- **Information**: Fluxo normal da aplicação
- **Warning**: Situações inesperadas mas tratáveis
- **Error**: Erros que impedem operação específica
- **Fatal**: Erros críticos que param o sistema

**Exemplo:**
```csharp
_logger.LogInformation(
    "Video {VideoId} processado com sucesso. Frames: {FrameCount}", 
    videoId, 
    frameCount
);
```

### Health Checks

```
GET /health

Response:
{
  "status": "Healthy",
  "checks": {
    "database": "Healthy",
    "redis": "Healthy",
    "rabbitmq": "Healthy",
    "storage": "Healthy"
  },
  "duration": "00:00:00.0234567"
}
```

---

## ⚡ Escalabilidade

### Escala Horizontal

**API:**
- Load Balancer → Múltiplas instâncias da API
- Stateless (sessões em Redis/JWT)

**Workers:**
- 3+ instâncias processando simultaneamente
- Cada worker consome da mesma fila
- RabbitMQ distribui mensagens automaticamente

**Database:**
- Read Replicas (futuro)
- Connection Pooling

**Storage:**
- Filesytem → Migração futura para S3/Azure Blob

### Limites e Capacidade

| Componente | Limite Atual | Limite Recomendado |
|------------|--------------|-------------------|
| Upload simultâneo | 20 | 100 com load balancer |
| Tamanho de vídeo | 2GB | 2GB |
| Workers | 3 | 10+ em produção |
| Conexões DB | 100 | 500 |
| Tempo de processamento | ~10min (2GB) | - |

---

## 🚀 Deploy e CI/CD

### Pipeline GitHub Actions

```yaml
1. Build → Compila solução
2. Test → Executa testes unitários
3. Build Docker Images → API e Worker
4. Push to Registry → Docker Hub / ACR
5. Deploy → Kubernetes / Docker Swarm
```

### Ambientes

- **Development**: Docker Compose local
- **Staging**: Cluster Kubernetes (dev)
- **Production**: Cluster Kubernetes (prod)

---

## 🔧 Tecnologias e Justificativas

| Tecnologia | Justificativa |
|------------|---------------|
| **.NET 8** | LTS, performance, suporte de 3 anos |
| **PostgreSQL** | ACID, confiável, open-source |
| **Redis** | Cache rápido, sessões distribuídas |
| **RabbitMQ** | Mensageria robusta, retry automático |
| **MassTransit** | Abstração sobre RabbitMQ, patterns prontos |
| **FFMpegCore** | Wrapper .NET para FFmpeg |
| **Serilog** | Logs estruturados, múltiplos sinks |
| **Seq** | Análise de logs centralizada |
| **Docker** | Portabilidade, reprodutibilidade |

---

## 📈 Melhorias Futuras

- [ ] Implementar Circuit Breaker (Polly)
- [ ] Migrar storage para S3/Azure Blob
- [ ] Adicionar websockets para status real-time
- [ ] Implementar CQRS completo com Event Sourcing
- [ ] Adicionar API Gateway (Ocelot/YARP)
- [ ] Implementar rate limiting por usuário
- [ ] Adicionar compressão de vídeos
- [ ] Suporte a múltiplos formatos de saída

---

**Documento mantido pela equipe de arquitetura**  
**Última atualização: 30/01/2026**
