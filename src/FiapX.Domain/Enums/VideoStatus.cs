namespace FiapX.Domain.Enums;

/// <summary>
/// Status do processamento de vídeo
/// </summary>
public enum VideoStatus
{
    /// <summary>
    /// Vídeo foi enviado mas ainda não entrou na fila
    /// </summary>
    Uploaded = 1,
    
    /// <summary>
    /// Vídeo está na fila aguardando processamento
    /// </summary>
    Queued = 2,
    
    /// <summary>
    /// Vídeo está sendo processado pelo worker
    /// </summary>
    Processing = 3,
    
    /// <summary>
    /// Processamento concluído com sucesso
    /// </summary>
    Completed = 4,
    
    /// <summary>
    /// Falha no processamento
    /// </summary>
    Failed = 5
}
