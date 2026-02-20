## 🏗️ Arquitetura

### Visão Geral

O sistema FiapX segue os princípios de **Clean Architecture** e **Domain-Driven Design (DDD)**, com separação clara de responsabilidades em camadas.

```
src/
├── FiapX.API/              # 🌐 Camada de Apresentação (REST API)
├── FiapX.Application/      # 💼 Camada de Aplicação (Use Cases)
├── FiapX.Domain/           # 🎯 Camada de Domínio (Entidades + Regras)
├── FiapX.Infrastructure/   # 🔧 Camada de Infraestrutura (BD, Filas, Storage)
├── FiapX.Shared/           # 📦 Camada Compartilhada (DTOs, Responses)
└── FiapX.Worker/           # ⚙️ Background Processing Worker
```

### 📊 Diagramas C4

Documentação visual completa da arquitetura seguindo o modelo C4:

#### Nível 1 - Contexto do Sistema
Visão geral de como o sistema interage com usuários e sistemas externos.

📄 **[Ver Diagrama de Contexto →](./docs/diagrams/c4-level1-context.md)**

#### Nível 2 - Containers
Aplicações e armazenamentos de dados que compõem o sistema.

📄 **[Ver Diagrama de Containers →](./docs/diagrams/c4-level2-containers.md)**

Containers principais:
- **API REST** (.NET 8) - Endpoints HTTP/REST
- **Worker** (.NET 8) - Processamento assíncrono
- **PostgreSQL** 16 - Banco de dados relacional
- **Redis** 7 - Cache distribuído
- **RabbitMQ** 3 - Fila de mensagens
- **File Storage** - Armazenamento de vídeos e ZIPs

#### Nível 3 - Componentes da API
Arquitetura interna seguindo Clean Architecture.

📄 **[Ver Diagrama de Componentes →](./docs/diagrams/c4-level3-components.md)**

Camadas:
- **Controllers** - Recebem requisições HTTP
- **Use Cases** - Orquestram lógica de negócio
- **Repositories** - Acesso a dados
- **Services** - Serviços de infraestrutura

### 🔄 Fluxo de Processamento

Diagrama de sequência mostrando o fluxo completo desde o upload até o download.

📄 **[Ver Diagrama de Sequência →](./docs/diagrams/sequence-video-processing.md)**

**Fluxo:**
```
1. Upload → API salva vídeo e publica evento
2. Worker consome evento da fila RabbitMQ
3. FFmpeg extrai frames (1 por segundo)
4. Worker cria ZIP e atualiza banco
5. Telegram notifica usuário
6. Usuário baixa ZIP via API
```

### 🔐 Padrões e Princípios Aplicados

- ✅ **Clean Architecture** - Dependências apontam para dentro
- ✅ **Domain-Driven Design (DDD)** - Modelagem rica de domínio
- ✅ **SOLID** - Princípios de design orientado a objetos
- ✅ **CQRS** - Separação de comandos e queries
- ✅ **Event-Driven Architecture** - Comunicação via eventos
- ✅ **Repository Pattern** - Abstração de acesso a dados
- ✅ **Dependency Injection** - Inversão de controle
- ✅ **Unit of Work** - Gerenciamento transacional

📖 **[Documentação completa da arquitetura →](./docs/architecture.md)**

---
