-- Portal Financeiro - Processos e etapas do mundo real (SQL Server, banco local).
-- 015: cria Processo (vínculo obrigatório e exclusivo: Parceria XOR Contrato)
-- e ProcessoEtapa (passos validáveis com ordem, previsão e conclusão).
-- Não toca Postgres (scripts/postgres/ mantido como está).

IF OBJECT_ID(N'dbo.Processo', N'U') IS NULL
BEGIN
    CREATE TABLE Processo (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        IdUsuario UNIQUEIDENTIFIER NOT NULL,
        Nome NVARCHAR(150) NOT NULL,
        Descricao NVARCHAR(500) NULL,
        IdParceria UNIQUEIDENTIFIER NULL,
        IdContrato UNIQUEIDENTIFIER NULL,
        Ativo BIT NOT NULL DEFAULT 1,
        DataCadastro DATETIME2 NOT NULL,
        DataAlteracao DATETIME2 NOT NULL,
        CONSTRAINT FK_Processo_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id),
        CONSTRAINT FK_Processo_Parceria FOREIGN KEY (IdParceria) REFERENCES Parceria(Id),
        CONSTRAINT FK_Processo_Contrato FOREIGN KEY (IdContrato) REFERENCES Contrato(Id),
        CONSTRAINT CK_Processo_Vinculo_Exclusivo CHECK (
            (IdParceria IS NOT NULL AND IdContrato IS NULL)
            OR (IdParceria IS NULL AND IdContrato IS NOT NULL)
        )
    );

    CREATE INDEX IX_Processo_Usuario ON Processo(IdUsuario);
    CREATE INDEX IX_Processo_Parceria ON Processo(IdParceria);
    CREATE INDEX IX_Processo_Contrato ON Processo(IdContrato);
END;

IF OBJECT_ID(N'dbo.ProcessoEtapa', N'U') IS NULL
BEGIN
    CREATE TABLE ProcessoEtapa (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        IdProcesso UNIQUEIDENTIFIER NOT NULL,
        Nome NVARCHAR(150) NOT NULL,
        Descricao NVARCHAR(500) NULL,
        Ordem INT NOT NULL DEFAULT 1,
        Concluida BIT NOT NULL DEFAULT 0,
        DataPrevista DATE NULL,
        DataConclusao DATETIME2 NULL,
        DataCadastro DATETIME2 NOT NULL,
        DataAlteracao DATETIME2 NOT NULL,
        CONSTRAINT FK_ProcessoEtapa_Processo FOREIGN KEY (IdProcesso) REFERENCES Processo(Id),
        CONSTRAINT CK_ProcessoEtapa_Ordem CHECK (Ordem >= 1)
    );

    CREATE INDEX IX_ProcessoEtapa_Processo ON ProcessoEtapa(IdProcesso);
END;

-- Libera o módulo "processos" para usuários existentes (padrão: sem acesso).
INSERT INTO PermissaoUsuario (Id, UsuarioId, Modulo, Nivel)
SELECT NEWID(), U.Id, 'processos', 0
FROM Usuario U
WHERE NOT EXISTS (
    SELECT 1 FROM PermissaoUsuario P
    WHERE P.UsuarioId = U.Id AND P.Modulo = 'processos'
);
