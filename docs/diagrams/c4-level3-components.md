# Diagrama C4 - Nível 3: Componentes da API

Este diagrama mostra os componentes internos da API REST, seguindo Clean Architecture.

```mermaid
C4Component
    title Diagrama de Componentes - API REST (Clean Architecture)
    
    Person(user, "Usuário", "Cliente da API")
    
    Container_Boundary(api, "FiapX.API") {
        Component(authController, "AuthController", "Controller", "Endpoints de autenticação (register/login)")
        Component(videosController, "VideosController", "Controller", "Endpoints de vídeos (upload/list/download)")
        Component(jwtMiddleware, "JWT Middleware", "Middleware", "Valida tokens JWT")
        
        Component(loginUseCase, "LoginUseCase", "Use Case", "Autentica usuário e gera JWT")
        Component(registerUseCase, "RegisterUserUseCase", "Use Case", "Registra novo usuário")
        Component(uploadUseCase, "UploadVideoUseCase", "Use Case", "Processa upload de vídeo")
        Component(listVideosUseCase, "GetUserVideosUseCase", "Use Case", "Lista vídeos do usuário")
        Component(downloadUseCase, "DownloadVideoUseCase", "Use Case", "Gera link de download")
        
        Component(userRepo, "UserRepository", "Repository", "Acesso a dados de usuários")
        Component(videoRepo, "VideoRepository", "Repository", "Acesso a dados de vídeos")
        Component(storageService, "StorageService", "Service", "Gerencia arquivos no disco")
        Component(jwtService, "JwtService", "Service", "Gera e valida tokens")
        Component(messagePublisher, "MessagePublisher", "Service", "Publica eventos na fila")
    }
    
    ContainerDb(postgres, "PostgreSQL", "Database")
    ContainerDb(redis, "Redis", "Cache")
    ContainerQueue(rabbitmq, "RabbitMQ", "Queue")
    Container(storage, "File Storage", "Files")
    
    Rel(user, authController, "POST /auth/register, /auth/login", "HTTPS")
    Rel(user, videosController, "POST /videos/upload", "HTTPS")
    Rel(user, videosController, "GET /videos", "HTTPS")
    
    Rel(authController, loginUseCase, "Usa")
    Rel(authController, registerUseCase, "Usa")
    Rel(videosController, jwtMiddleware, "Protegido por")
    Rel(videosController, uploadUseCase, "Usa")
    Rel(videosController, listVideosUseCase, "Usa")
    Rel(videosController, downloadUseCase, "Usa")
    
    Rel(loginUseCase, userRepo, "Busca usuário")
    Rel(loginUseCase, jwtService, "Gera token")
    Rel(registerUseCase, userRepo, "Cria usuário")
    Rel(uploadUseCase, videoRepo, "Salva metadados")
    Rel(uploadUseCase, storageService, "Salva arquivo")
    Rel(uploadUseCase, messagePublisher, "Publica VideoUploadedEvent")
    Rel(listVideosUseCase, videoRepo, "Lista vídeos")
    
    Rel(userRepo, postgres, "SQL")
    Rel(videoRepo, postgres, "SQL")
    Rel(storageService, storage, "File I/O")
    Rel(jwtService, redis, "Cache")
    Rel(messagePublisher, rabbitmq, "AMQP")
    
    UpdateLayoutConfig($c4ShapeInRow="3", $c4BoundaryInRow="1")
```

## Arquitetura em Camadas

### Presentation Layer (Controllers)
```
FiapX.API/Controllers/
├── AuthController.cs       → Register, Login
└── VideosController.cs     → Upload, List, Status, Download
```

**Responsabilidades:**
- Receber requisições HTTP
- Validar inputs
- Chamar Use Cases
- Retornar respostas HTTP

---

### Application Layer (Use Cases)
```
FiapX.Application/UseCases/
├── Auth/
│   ├── LoginUseCase.cs
│   └── RegisterUserUseCase.cs
└── Videos/
    ├── UploadVideoUseCase.cs
    ├── GetUserVideosUseCase.cs
    ├── GetVideoStatusUseCase.cs
    └── DownloadVideoUseCase.cs
```

**Responsabilidades:**
- Orquestrar lógica de negócio
- Coordenar repositories e services
- Validar regras de negócio
- Publicar eventos de domínio

---

### Domain Layer (Entities)
```
FiapX.Domain/Entities/
├── User.cs                 → Entidade de usuário
└── Video.cs                → Entidade de vídeo
```

**Responsabilidades:**
- Encapsular regras de negócio
- Validar invariantes
- Métodos de domínio (StartProcessing, CompleteProcessing, FailProcessing)

---

### Infrastructure Layer
```
FiapX.Infrastructure/
├── Repositories/
│   ├── UserRepository.cs
│   └── VideoRepository.cs
├── Services/
│   ├── StorageService.cs
│   ├── JwtService.cs
│   └── VideoProcessingService.cs
└── Messaging/
    └── MessagePublisher.cs
```

**Responsabilidades:**
- Implementar interfaces de domínio
- Acesso a banco de dados (EF Core)
- Acesso a sistemas externos
- Gerenciamento de arquivos

---

## Fluxo de Requisição: Upload de Vídeo

```
1. User → POST /api/videos/upload (multipart/form-data)
2. VideosController.Upload()
   ↓
3. JWT Middleware valida token
   ↓
4. UploadVideoUseCase.Execute()
   ↓ (paralelo)
   4a. StorageService.SaveVideoAsync() → File System
   4b. VideoRepository.AddAsync() → PostgreSQL
   ↓
5. MessagePublisher.PublishAsync(VideoUploadedEvent) → RabbitMQ
   ↓
6. Return 201 Created + VideoId
```

---

## Princípios Aplicados

✅ **Clean Architecture:** Dependências apontam para dentro  
✅ **SOLID:** Separação de responsabilidades  
✅ **DDD:** Entidades com comportamento  
✅ **CQRS:** Separação de comandos e queries  
✅ **Event-Driven:** Comunicação via eventos
