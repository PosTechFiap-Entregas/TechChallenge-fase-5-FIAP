# Diagrama de Sequência - Processamento de Vídeo

Este diagrama mostra o fluxo completo desde o upload até o download do resultado.

```mermaid
sequenceDiagram
    autonumber
    actor User as Usuário
    participant API as FiapX API
    participant DB as PostgreSQL
    participant Queue as RabbitMQ
    participant Storage as File Storage
    participant Worker as FiapX Worker
    participant FFmpeg as FFmpeg
    participant Telegram as Telegram Bot

    Note over User,Telegram: FASE 1: UPLOAD

    User->>API: POST /api/videos/upload<br/>(video.mp4 + JWT)
    activate API
    
    API->>API: Valida JWT
    API->>API: Valida arquivo (formato, tamanho)
    
    API->>Storage: Salva vídeo original
    activate Storage
    Storage-->>API: /uploads/{videoId}.mp4
    deactivate Storage
    
    API->>DB: INSERT Video<br/>(Status: Pending)
    activate DB
    DB-->>API: Video criado (Id: abc-123)
    deactivate DB
    
    API->>Queue: Publish VideoUploadedEvent<br/>{VideoId, UserId, StoragePath}
    activate Queue
    Queue-->>API: Event published
    deactivate Queue
    
    API-->>User: 201 Created<br/>{videoId: "abc-123", status: "Pending"}
    deactivate API

    Note over User,Telegram: FASE 2: PROCESSAMENTO ASSÍNCRONO

    Queue->>Worker: Consume VideoUploadedEvent
    activate Worker
    
    Worker->>DB: SELECT Video WHERE Id = abc-123
    activate DB
    DB-->>Worker: Video {Status: Pending}
    deactivate DB
    
    Worker->>DB: UPDATE Video<br/>(Status: Processing)
    activate DB
    DB-->>Worker: Updated
    deactivate DB
    
    Worker->>Storage: Lê vídeo original
    activate Storage
    Storage-->>Worker: video.mp4 (stream)
    deactivate Storage
    
    Worker->>FFmpeg: ffmpeg -i video.mp4<br/>-vf fps=1 frame_%04d.png
    activate FFmpeg
    
    loop Para cada segundo do vídeo
        FFmpeg->>FFmpeg: Extrai frame
    end
    
    FFmpeg-->>Worker: 150 frames extraídos
    deactivate FFmpeg
    
    Worker->>Worker: Cria ZIP com frames
    
    Worker->>Storage: Salva frames.zip
    activate Storage
    Storage-->>Worker: /outputs/{videoId}.zip
    deactivate Storage
    
    Worker->>DB: UPDATE Video<br/>(Status: Completed,<br/>ZipPath: /outputs/{videoId}.zip,<br/>FrameCount: 150)
    activate DB
    DB-->>Worker: Updated
    deactivate DB
    
    Worker->>Telegram: POST /sendMessage<br/>"✅ Vídeo processado!<br/>150 frames extraídos"
    activate Telegram
    Telegram-->>Worker: Message sent
    deactivate Telegram
    
    deactivate Worker

    Note over User,Telegram: FASE 3: DOWNLOAD

    User->>API: GET /api/videos/abc-123/status
    activate API
    
    API->>DB: SELECT Video WHERE Id = abc-123
    activate DB
    DB-->>API: Video {Status: Completed}
    deactivate DB
    
    API-->>User: 200 OK<br/>{status: "Completed",<br/>frameCount: 150}
    deactivate API
    
    User->>API: GET /api/videos/abc-123/download
    activate API
    
    API->>DB: SELECT Video WHERE Id = abc-123
    activate DB
    DB-->>API: Video {ZipPath: /outputs/{videoId}.zip}
    deactivate DB
    
    API->>Storage: Lê frames.zip
    activate Storage
    Storage-->>API: frames.zip (stream)
    deactivate Storage
    
    API-->>User: 200 OK<br/>Content-Type: application/zip<br/>(download do arquivo)
    deactivate API
```

## Cenários Alternativos

### Cenário 2: Erro no Processamento

```mermaid
sequenceDiagram
    autonumber
    participant Worker as FiapX Worker
    participant FFmpeg as FFmpeg
    participant DB as PostgreSQL
    participant Telegram as Telegram Bot
    participant Queue as RabbitMQ (DLQ)

    Worker->>FFmpeg: ffmpeg -i corrupted.mp4
    activate FFmpeg
    FFmpeg-->>Worker: ❌ Error: Invalid format
    deactivate FFmpeg
    
    Worker->>DB: UPDATE Video<br/>(Status: Failed,<br/>ErrorMessage: "Invalid format")
    activate DB
    DB-->>Worker: Updated
    deactivate DB
    
    Worker->>Telegram: POST /sendMessage<br/>"❌ Erro ao processar vídeo"
    activate Telegram
    Telegram-->>Worker: Message sent
    deactivate Telegram
    
    Note over Worker,Queue: Se falhar 3x, vai para DLQ
    
    Worker->>Queue: Move to Dead Letter Queue
    activate Queue
    Queue-->>Worker: Moved to DLQ
    deactivate Queue
```

---

### Cenário 3: Retry com Backoff

```mermaid
sequenceDiagram
    autonumber
    participant Queue as RabbitMQ
    participant Worker as FiapX Worker
    participant DB as PostgreSQL

    Queue->>Worker: Consume Event (Tentativa 1)
    activate Worker
    Worker->>DB: Connection timeout
    DB-->>Worker: ❌ Error
    Worker-->>Queue: Nack (requeue)
    deactivate Worker
    
    Note over Queue: Aguarda 1s (backoff)
    
    Queue->>Worker: Consume Event (Tentativa 2)
    activate Worker
    Worker->>DB: Connection timeout
    DB-->>Worker: ❌ Error
    Worker-->>Queue: Nack (requeue)
    deactivate Worker
    
    Note over Queue: Aguarda 4s (backoff exponencial)
    
    Queue->>Worker: Consume Event (Tentativa 3)
    activate Worker
    Worker->>DB: UPDATE Video SUCCESS
    DB-->>Worker: ✅ Updated
    Worker-->>Queue: Ack
    deactivate Worker
```

---

## Tempos Estimados

| Fase | Tempo Médio | Observação |
|---|---|---|
| Upload (API) | ~500ms | Depende do tamanho do arquivo |
| Enfileiramento | ~50ms | RabbitMQ |
| Processamento (Worker) | ~10-30s | Depende da duração do vídeo |
| Notificação | ~200ms | Telegram API |
| Download | ~1-3s | Depende do tamanho do ZIP |

---

## Estados do Vídeo

```
Pending → Processing → Completed
                    ↘ Failed
```

**Pending:** Vídeo enviado, aguardando processamento  
**Processing:** Worker está extraindo frames  
**Completed:** ZIP pronto para download  
**Failed:** Erro no processamento (formato inválido, corrompido, etc.)
