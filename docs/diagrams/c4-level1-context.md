# Diagrama C4 - Nível 1: Contexto do Sistema

Este diagrama mostra o contexto geral do sistema FiapX e como ele interage com usuários e sistemas externos.

```mermaid
C4Context
    title Diagrama de Contexto - Sistema FiapX
    
    Person(user, "Usuário", "Pessoa que faz upload de vídeos para extração de frames")
    
    System(fiapx, "FiapX System", "Sistema de processamento de vídeos com extração automática de frames")
    
    System_Ext(telegram, "Telegram Bot API", "Sistema de notificações em tempo real")
    
    Rel(user, fiapx, "Faz upload de vídeos, visualiza status e baixa resultados", "HTTPS/REST")
    Rel(fiapx, telegram, "Envia notificações de sucesso/falha", "HTTPS/Bot API")
    
    UpdateLayoutConfig($c4ShapeInRow="2", $c4BoundaryInRow="1")
```

## Descrição

**Usuário:**
- Faz upload de vídeos através da API REST
- Visualiza lista de vídeos processados
- Verifica status do processamento
- Baixa arquivo ZIP com frames extraídos

**Sistema FiapX:**
- Recebe vídeos via API REST
- Processa vídeos de forma assíncrona
- Extrai frames (1 por segundo)
- Gera arquivo ZIP
- Notifica usuário sobre conclusão

**Telegram Bot API:**
- Recebe comandos do sistema
- Envia notificações push
- Informa sucesso ou falha no processamento
