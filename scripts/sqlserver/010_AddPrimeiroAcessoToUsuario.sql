-- Adiciona coluna PrimeiroAcesso na tabela Usuario.
-- Usado para controlar se o usuário deve redefinir senha no primeiro acesso.
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Usuario') AND name = 'PrimeiroAcesso')
BEGIN
    ALTER TABLE Usuario ADD PrimeiroAcesso BIT NOT NULL DEFAULT 1;
END;
GO

-- Marca todos os usuários existentes como "já acessou" (senha já definida)
UPDATE Usuario SET PrimeiroAcesso = 0;
