-- ============================================
-- FIAP X - Video Processing System
-- Database Schema Script
-- PostgreSQL 16
-- ============================================

-- Criação do schema (se necessário)
CREATE SCHEMA IF NOT EXISTS public;

-- ============================================
-- TABELA: Users
-- ============================================
CREATE TABLE IF NOT EXISTS "Users" (
    "Id" UUID PRIMARY KEY,
    "Email" VARCHAR(256) NOT NULL UNIQUE,
    "PasswordHash" VARCHAR(255) NOT NULL,
    "Name" VARCHAR(200) NOT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NULL
);

CREATE INDEX IF NOT EXISTS "IX_Users_Email" ON "Users"("Email");
CREATE INDEX IF NOT EXISTS "IX_Users_IsActive" ON "Users"("IsActive");
CREATE INDEX IF NOT EXISTS "IX_Users_CreatedAt" ON "Users"("CreatedAt");

-- ============================================
-- TABELA: Videos
-- ============================================
CREATE TABLE IF NOT EXISTS "Videos" (
    "Id" UUID PRIMARY KEY,
    "UserId" UUID NOT NULL,
    "OriginalFileName" VARCHAR(255) NOT NULL,
    "StoragePath" VARCHAR(500) NOT NULL,
    "FileSizeBytes" BIGINT NOT NULL,
    "Status" INTEGER NOT NULL DEFAULT 1,
    "UploadedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ProcessedAt" TIMESTAMP NULL,
    "ZipPath" VARCHAR(500) NULL,
    "FrameCount" INTEGER NULL,
    "ErrorMessage" TEXT NULL,
    "ProcessingDuration" INTERVAL NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NULL,
    
    CONSTRAINT "FK_Videos_Users_UserId" FOREIGN KEY ("UserId") 
        REFERENCES "Users"("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_Videos_UserId" ON "Videos"("UserId");
CREATE INDEX IF NOT EXISTS "IX_Videos_Status" ON "Videos"("Status");
CREATE INDEX IF NOT EXISTS "IX_Videos_UploadedAt" ON "Videos"("UploadedAt");
CREATE INDEX IF NOT EXISTS "IX_Videos_ProcessedAt" ON "Videos"("ProcessedAt");

-- ============================================
-- COMENTÁRIOS
-- ============================================
COMMENT ON TABLE "Users" IS 'Tabela de usuários do sistema';
COMMENT ON COLUMN "Users"."Email" IS 'Email único do usuário';
COMMENT ON COLUMN "Users"."PasswordHash" IS 'Hash BCrypt da senha';
COMMENT ON COLUMN "Users"."IsActive" IS 'Indica se o usuário está ativo';

COMMENT ON TABLE "Videos" IS 'Tabela de vídeos processados';
COMMENT ON COLUMN "Videos"."Status" IS 'Status: 1=Uploaded, 2=Queued, 3=Processing, 4=Completed, 5=Failed';
COMMENT ON COLUMN "Videos"."FileSizeBytes" IS 'Tamanho do arquivo em bytes';
COMMENT ON COLUMN "Videos"."FrameCount" IS 'Quantidade de frames extraídos';
COMMENT ON COLUMN "Videos"."ProcessingDuration" IS 'Tempo total de processamento';

-- ============================================
-- DADOS DE TESTE (Opcional - Remover em produção)
-- ============================================

-- Usuário de teste (senha: Test@123)
-- Hash gerado com BCrypt work factor 12
INSERT INTO "Users" ("Id", "Email", "PasswordHash", "Name", "IsActive", "CreatedAt")
VALUES 
    ('550e8400-e29b-41d4-a716-446655440000', 
     'teste@fiapx.com', 
     '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYVQZQzg0Mm', 
     'Usuário Teste', 
     TRUE, 
     CURRENT_TIMESTAMP)
ON CONFLICT ("Id") DO NOTHING;

-- ============================================
-- VIEWS ÚTEIS
-- ============================================

-- View de estatísticas por usuário
CREATE OR REPLACE VIEW "UserVideoStats" AS
SELECT 
    u."Id" AS "UserId",
    u."Email",
    u."Name",
    COUNT(v."Id") AS "TotalVideos",
    COUNT(CASE WHEN v."Status" = 4 THEN 1 END) AS "CompletedVideos",
    COUNT(CASE WHEN v."Status" = 5 THEN 1 END) AS "FailedVideos",
    COUNT(CASE WHEN v."Status" IN (2, 3) THEN 1 END) AS "ProcessingVideos",
    COALESCE(SUM(v."FileSizeBytes"), 0) AS "TotalSizeBytes",
    COALESCE(SUM(v."FrameCount"), 0) AS "TotalFrames"
FROM "Users" u
LEFT JOIN "Videos" v ON u."Id" = v."UserId"
GROUP BY u."Id", u."Email", u."Name";

-- View de vídeos pendentes
CREATE OR REPLACE VIEW "PendingVideos" AS
SELECT 
    v.*,
    u."Email" AS "UserEmail",
    u."Name" AS "UserName"
FROM "Videos" v
INNER JOIN "Users" u ON v."UserId" = u."Id"
WHERE v."Status" IN (2, 3) -- Queued ou Processing
ORDER BY v."UploadedAt" ASC;

-- ============================================
-- FUNÇÕES ÚTEIS
-- ============================================

-- Função para limpar vídeos antigos (mais de 30 dias)
CREATE OR REPLACE FUNCTION cleanup_old_videos(days_old INTEGER DEFAULT 30)
RETURNS INTEGER AS $$
DECLARE
    deleted_count INTEGER;
BEGIN
    DELETE FROM "Videos"
    WHERE "CreatedAt" < CURRENT_TIMESTAMP - (days_old || ' days')::INTERVAL
      AND "Status" IN (4, 5); -- Apenas completed ou failed
    
    GET DIAGNOSTICS deleted_count = ROW_COUNT;
    RETURN deleted_count;
END;
$$ LANGUAGE plpgsql;

-- ============================================
-- PERMISSÕES (Ajustar conforme necessário)
-- ============================================

-- Garantir permissões para o usuário da aplicação
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO fiapx_user;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO fiapx_user;

-- ============================================
-- FIM DO SCRIPT
-- ============================================

-- Verificar criação
SELECT 
    tablename, 
    schemaname
FROM pg_tables 
WHERE schemaname = 'public' 
  AND tablename IN ('Users', 'Videos')
ORDER BY tablename;
