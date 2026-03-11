# 🏛️ Diagramas C4 - Sistema FiapX

## 📖 Sobre C4

O modelo C4 (Context, Containers, Components, Code) é uma técnica de diagramação criada por Simon Brown para visualizar arquitetura de software em diferentes níveis de abstração.

---

## 🌍 Nível 1: Diagrama de Contexto

Mostra o sistema como uma caixa preta e suas interações externas.

```mermaid
C4Context
    title Diagrama de Contexto - Sistema FiapX
    
    Person(user, "Usuário", "Pessoa que deseja processar vídeos em frames")
    
    System(fiapx, "Sistema FiapX", "Sistema de processamento de vídeos com extração de frames e geração de ZIP")
    
    System_Ext(telegram, "Telegram Bot API", "Serviço de mensagens para notificações")
    System_Ext(storage, "File System", "Sistema de arquivos local para armazenamento")
    
    Rel(user, fiapx, "Faz upload de vídeos, consulta status, baixa ZIP", "HTTPS/REST")
    Rel(fiapx, telegram, "Envia notificações de sucesso/erro", "HTTPS/Bot API")
    Rel(fiapx, storage, "Armazena vídeos originais e ZIPs gerados", "File I/O")
    
    UpdateRelStyle(user, fiapx, $offsetY="-40", $offsetX="-50")
    UpdateRelStyle(fiapx, telegram, $offsetY="-30")
    UpdateRelStyle(fiapx, storage, $offsetY="20")
```

### Exportar para Draw.io:
1. Copie o código Mermaid acima
2. Abra draw.io → Insert → Advanced → Mermaid
3. Cole o código

---

## 📦 Nível 2: Diagrama de Containers

Mostra os containers (aplicações, serviços, bancos de dados) que compõem o sistema.

```mermaid
C4Container
    title Diagrama de Containers - Sistema FiapX
    
    Person(user, "Usuário", "Pessoa que usa o sistema")
    
    Container_Boundary(fiapx, "Sistema FiapX") {
        Container(api, "API REST", ".NET 8 / ASP.NET Core", "Fornece endpoints REST para upload, consulta e download")
        Container(worker, "Worker Service", ".NET 8 / Background Service", "Processa vídeos em background consumindo fila")
        
        ContainerDb(postgres, "PostgreSQL", "PostgreSQL 16", "Armazena metadados de usuários e vídeos")
        ContainerDb(redis, "Redis Cache", "Redis 7", "Cache de sessões e dados frequentes")
        ContainerQueue(rabbitmq, "RabbitMQ", "RabbitMQ 3.13", "Fila de mensagens para processamento assíncrono")
        
        Container(storage, "Local Storage", "File System", "Armazena vídeos originais e ZIPs processados")
        Container(seq, "Seq", "Seq Logs", "Centralização e análise de logs")
    }
    
    System_Ext(telegram, "Telegram", "Bot API para notificações")
    
    Rel(user, api, "Usa", "HTTPS/REST")
    
    Rel(api, postgres, "Lê/Escreve", "TCP/Npgsql")
    Rel(api, redis, "Lê/Escreve", "TCP/StackExchange.Redis")
    Rel(api, rabbitmq, "Publica eventos", "AMQP/MassTransit")
    Rel(api, storage, "Salva uploads", "File I/O")
    Rel(api, seq, "Envia logs", "HTTP")
    
    Rel(worker, rabbitmq, "Consome eventos", "AMQP/MassTransit")
    Rel(worker, postgres, "Atualiza status", "TCP/Npgsql")
    Rel(worker, storage, "Lê vídeos / Salva ZIPs", "File I/O")
    Rel(worker, telegram, "Envia notificações", "HTTPS")
    Rel(worker, seq, "Envia logs", "HTTP")
    
    UpdateRelStyle(user, api, $offsetY="-60")
    UpdateRelStyle(api, rabbitmq, $offsetX="-50")
    UpdateRelStyle(worker, rabbitmq, $offsetX="50")
```

### Componentes Principais:

| Container | Tecnologia | Responsabilidade |
|-----------|-----------|------------------|
| **API REST** | ASP.NET Core 8 | Autenticação, upload, consultas, download |
| **Worker** | .NET Background Service | Processamento de vídeos com FFmpeg |
| **PostgreSQL** | PostgreSQL 16 | Persistência de metadados |
| **Redis** | Redis 7 | Cache para performance |
| **RabbitMQ** | RabbitMQ 3.13 | Mensageria assíncrona |
| **Storage** | File System | Armazenamento de arquivos |
| **Seq** | Seq | Observabilidade e logs |

---

## 🔧 Nível 3: Diagrama de Componentes - API

Mostra os componentes internos da API.

```mermaid
C4Component
    title Diagrama de Componentes - API REST
    
    Person(user, "Usuário")
    
    Container_Boundary(api, "API REST") {
        Component(authController, "AuthController", "Controller", "Endpoints de autenticação")
        Component(videoController, "VideosController", "Controller", "Endpoints de vídeos")
        
        Component(registerUseCase, "RegisterUserUseCase", "Use Case", "Lógica de registro")
        Component(loginUseCase, "LoginUseCase", "Use Case", "Lógica de login")
        Component(uploadUseCase, "UploadVideoUseCase", "Use Case", "Lógica de upload")
        Component(statusUseCase, "GetVideoStatusUseCase", "Use Case", "Lógica de consulta")
        Component(downloadUseCase, "DownloadVideoUseCase", "Use Case", "Lógica de download")
        
        Component(validators, "FluentValidators", "Validators", "Validação de requests")
        Component(middleware, "GlobalExceptionHandler", "Middleware", "Tratamento de erros")
        
        Component(unitOfWork, "UnitOfWork", "Repository Pattern", "Coordena repositórios")
        Component(userRepo, "UserRepository", "Repository", "Acesso a usuários")
        Component(videoRepo, "VideoRepository", "Repository", "Acesso a vídeos")
        
        Component(jwtService, "JwtTokenService", "Service", "Geração de tokens JWT")
        Component(storageService, "LocalStorageService", "Service", "Gerenciamento de arquivos")
        Component(messagePublisher, "MessagePublisher", "Service", "Publicação de eventos")
    }
    
    ContainerDb(postgres, "PostgreSQL")
    ContainerQueue(rabbitmq, "RabbitMQ")
    Container(storage, "Storage")
    
    Rel(user, authController, "POST /api/auth/*", "HTTPS")
    Rel(user, videoController, "POST/GET /api/videos/*", "HTTPS")
    
    Rel(authController, registerUseCase, "Usa")
    Rel(authController, loginUseCase, "Usa")
    Rel(videoController, uploadUseCase, "Usa")
    Rel(videoController, statusUseCase, "Usa")
    Rel(videoController, downloadUseCase, "Usa")
    
    Rel(registerUseCase, validators, "Valida")
    Rel(loginUseCase, validators, "Valida")
    Rel(uploadUseCase, validators, "Valida")
    
    Rel(registerUseCase, unitOfWork, "Usa")
    Rel(loginUseCase, unitOfWork, "Usa")
    Rel(uploadUseCase, unitOfWork, "Usa")
    Rel(statusUseCase, unitOfWork, "Usa")
    Rel(downloadUseCase, unitOfWork, "Usa")
    
    Rel(unitOfWork, userRepo, "Acessa")
    Rel(unitOfWork, videoRepo, "Acessa")
    
    Rel(userRepo, postgres, "SQL")
    Rel(videoRepo, postgres, "SQL")
    
    Rel(loginUseCase, jwtService, "Gera token")
    Rel(uploadUseCase, storageService, "Salva arquivo")
    Rel(uploadUseCase, messagePublisher, "Publica evento")
    Rel(downloadUseCase, storageService, "Lê arquivo")
    
    Rel(storageService, storage, "File I/O")
    Rel(messagePublisher, rabbitmq, "AMQP")
```

---

## 🔧 Nível 3: Diagrama de Componentes - Worker

Mostra os componentes internos do Worker.

```mermaid
C4Component
    title Diagrama de Componentes - Worker Service
    
    ContainerQueue(rabbitmq, "RabbitMQ")
    
    Container_Boundary(worker, "Worker Service") {
        Component(consumer, "VideoUploadedEventConsumer", "Consumer", "Consome eventos de upload")
        Component(consumerDef, "ConsumerDefinition", "Configuration", "Configurações de retry e circuit breaker")
        
        Component(processingService, "VideoProcessingService", "Service", "Orquestra processamento")
        Component(ffmpegService, "FFmpegVideoProcessingService", "Service", "Extração de frames com FFmpeg")
        Component(storageService, "LocalStorageService", "Service", "Gerenciamento de arquivos")
        Component(telegramService, "TelegramNotificationService", "Service", "Envio de notificações")
        
        Component(videoRepo, "VideoRepository", "Repository", "Acesso a vídeos")
        Component(metricsService, "VideoMetricsService", "Service", "Métricas Prometheus")
    }
    
    ContainerDb(postgres, "PostgreSQL")
    Container(storage, "Storage")
    System_Ext(telegram, "Telegram")
    
    Rel(rabbitmq, consumer, "VideoUploadedEvent", "AMQP")
    Rel(consumer, consumerDef, "Usa configurações")
    
    Rel(consumer, processingService, "Executa")
    Rel(processingService, ffmpegService, "Extrai frames")
    Rel(processingService, storageService, "Lê/Salva arquivos")
    Rel(processingService, videoRepo, "Atualiza status")
    Rel(processingService, metricsService, "Registra métricas")
    
    Rel(ffmpegService, storage, "File I/O")
    Rel(storageService, storage, "File I/O")
    Rel(videoRepo, postgres, "SQL")
    
    Rel(consumer, telegramService, "Notifica em sucesso/erro")
    Rel(telegramService, telegram, "HTTPS")
```

---

## 🔄 Diagrama de Sequência - Upload e Processamento

```mermaid
sequenceDiagram
    autonumber
    
    actor User as Usuário
    participant API as API REST
    participant DB as PostgreSQL
    participant MQ as RabbitMQ
    participant Worker as Worker
    participant FFmpeg as FFmpeg
    participant Storage as File Storage
    participant TG as Telegram
    
    User->>API: POST /api/videos/upload
    activate API
    
    API->>API: Validar (tamanho, formato)
    API->>Storage: Salvar arquivo original
    Storage-->>API: Path do arquivo
    
    API->>DB: INSERT Video (status=Uploaded)
    DB-->>API: Video ID
    
    API->>DB: UPDATE Video (status=Queued)
    API->>MQ: Publish VideoUploadedEvent
    
    API-->>User: 202 Accepted + Video ID
    deactivate API
    
    Note over MQ,Worker: Processamento Assíncrono
    
    MQ->>Worker: VideoUploadedEvent
    activate Worker
    
    Worker->>DB: UPDATE Video (status=Processing)
    Worker->>Storage: Ler vídeo original
    Storage-->>Worker: Stream do vídeo
    
    Worker->>FFmpeg: Extrair frames @ 1 FPS
    activate FFmpeg
    FFmpeg-->>Worker: Frames extraídos
    deactivate FFmpeg
    
    Worker->>Worker: Criar arquivo ZIP
    Worker->>Storage: Salvar ZIP
    Storage-->>Worker: Path do ZIP
    
    Worker->>DB: UPDATE Video (status=Completed, zipPath, frameCount)
    Worker->>TG: Enviar notificação de sucesso
    
    deactivate Worker
    
    Note over User,API: Consulta e Download
    
    User->>API: GET /api/videos/{id}/status
    activate API
    API->>DB: SELECT Video
    DB-->>API: Video details
    API-->>User: 200 OK (status=Completed)
    deactivate API
    
    User->>API: GET /api/videos/{id}/download
    activate API
    API->>DB: SELECT Video
    DB-->>API: Video (zipPath)
    API->>Storage: Ler ZIP
    Storage-->>API: Stream do ZIP
    API-->>User: 200 OK (arquivo ZIP)
    deactivate API
```

---

## 🔄 Diagrama de Sequência - Tratamento de Erro com Retry

```mermaid
sequenceDiagram
    autonumber
    
    participant MQ as RabbitMQ
    participant Worker as Worker
    participant FFmpeg as FFmpeg
    participant DB as PostgreSQL
    participant TG as Telegram
    
    MQ->>Worker: VideoUploadedEvent (Tentativa 1)
    activate Worker
    
    Worker->>DB: UPDATE status=Processing
    Worker->>FFmpeg: Processar vídeo
    FFmpeg-->>Worker: ❌ Erro (formato inválido)
    
    Worker->>Worker: Incrementar contador de tentativas
    Worker->>DB: UPDATE status=Failed (temporário)
    Worker-->>MQ: ❌ Throw exception
    deactivate Worker
    
    Note over MQ: Aguarda 1s (backoff)
    
    MQ->>Worker: VideoUploadedEvent (Tentativa 2)
    activate Worker
    
    Worker->>DB: UPDATE status=Processing
    Worker->>FFmpeg: Processar vídeo
    FFmpeg-->>Worker: ❌ Erro (novamente)
    
    Worker->>Worker: Incrementar contador (2)
    Worker->>DB: UPDATE status=Failed (temporário)
    Worker-->>MQ: ❌ Throw exception
    deactivate Worker
    
    Note over MQ: Aguarda 5s (backoff exponencial)
    
    MQ->>Worker: VideoUploadedEvent (Tentativa 3 - ÚLTIMA)
    activate Worker
    
    Worker->>DB: UPDATE status=Processing
    Worker->>FFmpeg: Processar vídeo
    FFmpeg-->>Worker: ❌ Erro (novamente)
    
    Worker->>Worker: Tentativas = 3 (MAX)
    Worker->>DB: UPDATE status=Failed (DEFINITIVO)
    Worker->>TG: Enviar notificação de erro
    Worker-->>MQ: ❌ Throw exception
    deactivate Worker
    
    Note over MQ: Mover para Dead Letter Queue
```

---

## 🏗️ Diagrama de Deployment

```mermaid
graph TB
    subgraph "Docker Host"
        subgraph "Rede: fiapx-network"
            API[API Container<br/>fiapx-api:latest<br/>Port: 8080]
            Worker[Worker Container<br/>fiapx-worker:latest]
            
            PG[(PostgreSQL Container<br/>postgres:16<br/>Port: 5432)]
            Redis[(Redis Container<br/>redis:7<br/>Port: 6379)]
            Rabbit[RabbitMQ Container<br/>rabbitmq:3.13<br/>Ports: 5672, 15672]
            
            Seq[Seq Container<br/>datalust/seq<br/>Port: 8081]
        end
        
        Storage[(/storage/<br/>uploads/<br/>outputs/<br/>temp/)]
    end
    
    User[Usuario] -->|HTTPS:8080| API
    Admin[Admin] -->|HTTP:15672| Rabbit
    Admin -->|HTTP:8081| Seq
    
    API --> PG
    API --> Redis
    API --> Rabbit
    API --> Storage
    API --> Seq
    
    Worker --> Rabbit
    Worker --> PG
    Worker --> Storage
    Worker --> Seq
    
    style API fill:#1168bd,color:#fff
    style Worker fill:#1168bd,color:#fff
    style PG fill:#336791,color:#fff
    style Redis fill:#d82c20,color:#fff
    style Rabbit fill:#ff6600,color:#fff
    style Seq fill:#512bd4,color:#fff
```

---

## 📊 Diagrama de Estados - Video Lifecycle

```mermaid
stateDiagram-v2
    [*] --> Uploaded: Upload concluído
    
    Uploaded --> Queued: Evento publicado na fila
    
    Queued --> Processing: Worker consome evento
    
    Processing --> Completed: Processamento bem-sucedido
    Processing --> Failed: Erro no processamento
    
    Failed --> Queued: Retry (< 3 tentativas)
    Failed --> [*]: Max retries atingido
    
    Completed --> [*]: Processo finalizado
    
    note right of Uploaded
        Status inicial após upload
        Arquivo salvo no storage
    end note
    
    note right of Queued
        Aguardando processamento
        Evento na fila RabbitMQ
    end note
    
    note right of Processing
        Worker processando
        Extração de frames em andamento
    end note
    
    note right of Completed
        ZIP disponível para download
        FrameCount e ProcessingDuration salvos
    end note
    
    note right of Failed
        ErrorMessage armazenado
        Notificação enviada (se habilitado)
    end note
```

---

## 🔐 Diagrama de Autenticação - JWT Flow

```mermaid
sequenceDiagram
    participant User
    participant API
    participant Validator
    participant UseCase
    participant UserRepo
    participant JWT
    participant DB
    
    User->>API: POST /api/auth/login<br/>{email, password}
    API->>Validator: Validar request
    Validator-->>API: ✓ Válido
    
    API->>UseCase: LoginUseCase.Execute()
    UseCase->>UserRepo: GetByEmailAsync(email)
    UserRepo->>DB: SELECT * FROM Users WHERE email=?
    DB-->>UserRepo: User data
    UserRepo-->>UseCase: User entity
    
    UseCase->>UseCase: PasswordHasher.Verify(password, user.PasswordHash)
    UseCase-->>UseCase: ✓ Senha válida
    
    UseCase->>JWT: GenerateToken(user.Id, user.Email, user.Name)
    JWT-->>UseCase: JWT Token (60 min expiration)
    
    UseCase-->>API: AuthResponse {token, expiresAt, user}
    API-->>User: 200 OK + JWT Token
    
    Note over User: Armazena token no cliente
    
    User->>API: GET /api/videos<br/>Header: Authorization: Bearer {token}
    API->>API: Validar token JWT
    API-->>User: 200 OK + Lista de vídeos
```

---

**Todos os diagramas acima são compatíveis com:**
- ✅ Mermaid Live Editor
- ✅ Draw.io (via plugin Mermaid)
- ✅ VS Code (via extensão Mermaid)
- ✅ GitHub/GitLab (renderização nativa)
- ✅ Confluence (via plugin)

---

**Desenvolvido com C4 Model para FIAP** 🏛️
