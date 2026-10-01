-- Portal Financeiro - Motor de processos (SQL Server, banco local).
-- 021: cria ModeloProcesso/ModeloEtapa/ModeloItem (templates reutilizáveis),
-- ProcessoEtapaItem (sub-itens de cada fase) e ProcessoAnexo (estrutura para o Plano 2);
-- evolui Processo (IdModeloProcesso, IdCliente, DataEncerramento) e
-- ProcessoEtapa (DataInicio, para medir tempo por fase).
-- Limpa os dados atuais de Processo/ProcessoEtapa (eram dados de teste, sem modelo)
-- e semeia o modelo "Regularização de Imóvel" para cada usuário existente.
-- Não toca Postgres (scripts/postgres/ mantido como está).

IF OBJECT_ID(N'dbo.ModeloProcesso', N'U') IS NULL
BEGIN
    CREATE TABLE ModeloProcesso (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        IdUsuario UNIQUEIDENTIFIER NOT NULL,
        Nome NVARCHAR(150) NOT NULL,
        Descricao NVARCHAR(500) NULL,
        Ativo BIT NOT NULL DEFAULT 1,
        DataCadastro DATETIME2 NOT NULL,
        DataAlteracao DATETIME2 NOT NULL,
        CriadoPor UNIQUEIDENTIFIER NULL,
        AlteradoPor UNIQUEIDENTIFIER NULL,
        CONSTRAINT FK_ModeloProcesso_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id)
    );

    CREATE INDEX IX_ModeloProcesso_Usuario ON ModeloProcesso(IdUsuario);
END;

IF OBJECT_ID(N'dbo.ModeloEtapa', N'U') IS NULL
BEGIN
    CREATE TABLE ModeloEtapa (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        IdModeloProcesso UNIQUEIDENTIFIER NOT NULL,
        Nome NVARCHAR(150) NOT NULL,
        Descricao NVARCHAR(500) NULL,
        Ordem INT NOT NULL DEFAULT 1,
        DataCadastro DATETIME2 NOT NULL,
        DataAlteracao DATETIME2 NOT NULL,
        CriadoPor UNIQUEIDENTIFIER NULL,
        AlteradoPor UNIQUEIDENTIFIER NULL,
        CONSTRAINT FK_ModeloEtapa_Modelo FOREIGN KEY (IdModeloProcesso) REFERENCES ModeloProcesso(Id),
        CONSTRAINT CK_ModeloEtapa_Ordem CHECK (Ordem >= 1)
    );

    CREATE INDEX IX_ModeloEtapa_Modelo ON ModeloEtapa(IdModeloProcesso);
END;

IF OBJECT_ID(N'dbo.ModeloItem', N'U') IS NULL
BEGIN
    CREATE TABLE ModeloItem (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        IdModeloEtapa UNIQUEIDENTIFIER NOT NULL,
        Nome NVARCHAR(150) NOT NULL,
        Descricao NVARCHAR(500) NULL,
        Obrigatorio BIT NOT NULL DEFAULT 1,
        ExigeAnexo BIT NOT NULL DEFAULT 0,
        Ordem INT NOT NULL DEFAULT 1,
        DataCadastro DATETIME2 NOT NULL,
        DataAlteracao DATETIME2 NOT NULL,
        CriadoPor UNIQUEIDENTIFIER NULL,
        AlteradoPor UNIQUEIDENTIFIER NULL,
        CONSTRAINT FK_ModeloItem_Etapa FOREIGN KEY (IdModeloEtapa) REFERENCES ModeloEtapa(Id),
        CONSTRAINT CK_ModeloItem_Ordem CHECK (Ordem >= 1)
    );

    CREATE INDEX IX_ModeloItem_Etapa ON ModeloItem(IdModeloEtapa);
END;

IF COL_LENGTH(N'dbo.Processo', N'IdModeloProcesso') IS NULL
    ALTER TABLE Processo ADD IdModeloProcesso UNIQUEIDENTIFIER NULL;

IF COL_LENGTH(N'dbo.Processo', N'IdCliente') IS NULL
    ALTER TABLE Processo ADD IdCliente UNIQUEIDENTIFIER NULL;

IF COL_LENGTH(N'dbo.Processo', N'DataEncerramento') IS NULL
    ALTER TABLE Processo ADD DataEncerramento DATETIME2 NULL;

IF OBJECT_ID(N'dbo.FK_Processo_Modelo', N'F') IS NULL
    ALTER TABLE Processo ADD CONSTRAINT FK_Processo_Modelo FOREIGN KEY (IdModeloProcesso) REFERENCES ModeloProcesso(Id);

IF OBJECT_ID(N'dbo.FK_Processo_Cliente', N'F') IS NULL
    ALTER TABLE Processo ADD CONSTRAINT FK_Processo_Cliente FOREIGN KEY (IdCliente) REFERENCES Pessoa(Id);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Processo_Modelo' AND object_id = OBJECT_ID(N'dbo.Processo'))
    CREATE INDEX IX_Processo_Modelo ON Processo(IdModeloProcesso);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Processo_Cliente' AND object_id = OBJECT_ID(N'dbo.Processo'))
    CREATE INDEX IX_Processo_Cliente ON Processo(IdCliente);

IF COL_LENGTH(N'dbo.ProcessoEtapa', N'DataInicio') IS NULL
    ALTER TABLE ProcessoEtapa ADD DataInicio DATETIME2 NULL;

IF OBJECT_ID(N'dbo.ProcessoEtapaItem', N'U') IS NULL
BEGIN
    CREATE TABLE ProcessoEtapaItem (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        IdProcessoEtapa UNIQUEIDENTIFIER NOT NULL,
        Nome NVARCHAR(150) NOT NULL,
        Descricao NVARCHAR(500) NULL,
        Obrigatorio BIT NOT NULL DEFAULT 1,
        ExigeAnexo BIT NOT NULL DEFAULT 0,
        Ordem INT NOT NULL DEFAULT 1,
        Concluida BIT NOT NULL DEFAULT 0,
        DataInicio DATETIME2 NULL,
        DataConclusao DATETIME2 NULL,
        DataCadastro DATETIME2 NOT NULL,
        DataAlteracao DATETIME2 NOT NULL,
        CriadoPor UNIQUEIDENTIFIER NULL,
        AlteradoPor UNIQUEIDENTIFIER NULL,
        CONSTRAINT FK_ProcessoEtapaItem_Etapa FOREIGN KEY (IdProcessoEtapa) REFERENCES ProcessoEtapa(Id),
        CONSTRAINT CK_ProcessoEtapaItem_Ordem CHECK (Ordem >= 1)
    );

    CREATE INDEX IX_ProcessoEtapaItem_Etapa ON ProcessoEtapaItem(IdProcessoEtapa);
END;

IF OBJECT_ID(N'dbo.ProcessoAnexo', N'U') IS NULL
BEGIN
    CREATE TABLE ProcessoAnexo (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        IdProcessoEtapaItem UNIQUEIDENTIFIER NOT NULL,
        NomeArquivo NVARCHAR(255) NOT NULL,
        ContentType NVARCHAR(100) NULL,
        Tamanho BIGINT NOT NULL DEFAULT 0,
        DriveFileId NVARCHAR(255) NULL,
        Url NVARCHAR(500) NULL,
        DataCadastro DATETIME2 NOT NULL,
        DataAlteracao DATETIME2 NOT NULL,
        CriadoPor UNIQUEIDENTIFIER NULL,
        AlteradoPor UNIQUEIDENTIFIER NULL,
        CONSTRAINT FK_ProcessoAnexo_Item FOREIGN KEY (IdProcessoEtapaItem) REFERENCES ProcessoEtapaItem(Id)
    );

    CREATE INDEX IX_ProcessoAnexo_Item ON ProcessoAnexo(IdProcessoEtapaItem);
END;

GO

-- Limpa processos de teste sem modelo (aprovado: recriar com o modelo novo).
DELETE FROM ProcessoEtapa WHERE IdProcesso IN (SELECT Id FROM Processo WHERE IdModeloProcesso IS NULL);
DELETE FROM Processo WHERE IdModeloProcesso IS NULL;

GO

-- Semeia o modelo "Regularização de Imóvel" para cada usuário (só se ainda não existir).
DECLARE @IdUsuario UNIQUEIDENTIFIER;
DECLARE cur CURSOR LOCAL FAST_FORWARD FOR SELECT Id FROM Usuario;
OPEN cur;
FETCH NEXT FROM cur INTO @IdUsuario;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM ModeloProcesso WHERE IdUsuario = @IdUsuario AND Nome = N'Regularização de Imóvel')
    BEGIN
        DECLARE @Agora DATETIME2 = SYSUTCDATETIME();
        DECLARE @IdModelo UNIQUEIDENTIFIER = NEWID();
        INSERT INTO ModeloProcesso (Id, IdUsuario, Nome, Descricao, Ativo, DataCadastro, DataAlteracao, CriadoPor, AlteradoPor)
        VALUES (@IdModelo, @IdUsuario, N'Regularização de Imóvel', N'Modelo padrão de regularização de imóvel: documental, execução, protocolo e entrega.', 1, @Agora, @Agora, @IdUsuario, @IdUsuario);

        DECLARE @F1 UNIQUEIDENTIFIER = NEWID(), @F2 UNIQUEIDENTIFIER = NEWID(), @F3 UNIQUEIDENTIFIER = NEWID(), @F4 UNIQUEIDENTIFIER = NEWID();
        INSERT INTO ModeloEtapa (Id, IdModeloProcesso, Nome, Descricao, Ordem, DataCadastro, DataAlteracao, CriadoPor, AlteradoPor)
        VALUES
            (@F1, @IdModelo, N'Documental', N'Documentação inicial: orçamento, contrato e coleta de dados do cliente.', 1, @Agora, @Agora, @IdUsuario, @IdUsuario),
            (@F2, @IdModelo, N'Mão na massa', N'Levantamento em loco e desenho do imóvel no software.', 2, @Agora, @Agora, @IdUsuario, @IdUsuario),
            (@F3, @IdModelo, N'Protocolar', N'Protocolo do processo na prefeitura.', 3, @Agora, @Agora, @IdUsuario, @IdUsuario),
            (@F4, @IdModelo, N'Etapa final', N'Emissão do documento e entrega do projeto ao cliente.', 4, @Agora, @Agora, @IdUsuario, @IdUsuario);

        INSERT INTO ModeloItem (Id, IdModeloEtapa, Nome, Descricao, Obrigatorio, ExigeAnexo, Ordem, DataCadastro, DataAlteracao, CriadoPor, AlteradoPor)
        VALUES
            (NEWID(), @F1, N'Orçamento', N'Após mandar para o cliente, anexar o orçamento.', 1, 1, 1, @Agora, @Agora, @IdUsuario, @IdUsuario),
            (NEWID(), @F1, N'Contrato', N'Após mandar para o cliente, anexar o contrato.', 1, 1, 2, @Agora, @Agora, @IdUsuario, @IdUsuario),
            (NEWID(), @F1, N'Coleta de dados', N'Enviar ao cliente um formulário coletando todas as informações preliminares para dar início ao trabalho.', 1, 0, 3, @Agora, @Agora, @IdUsuario, @IdUsuario),
            (NEWID(), @F2, N'Levantamento em loco', N'Pedir as medidas do imóvel (máximo 3 visitas).', 1, 0, 1, @Agora, @Agora, @IdUsuario, @IdUsuario),
            (NEWID(), @F2, N'Desenho do imóvel', N'Desenhar o imóvel no software e anexar depois de pronto.', 1, 1, 2, @Agora, @Agora, @IdUsuario, @IdUsuario),
            (NEWID(), @F3, N'Protocolo na prefeitura', N'Protocolar o processo na prefeitura.', 1, 0, 1, @Agora, @Agora, @IdUsuario, @IdUsuario),
            (NEWID(), @F4, N'Emissão do documento', N'Emitir o documento final do processo.', 1, 1, 1, @Agora, @Agora, @IdUsuario, @IdUsuario),
            (NEWID(), @F4, N'Entrega do projeto', N'Entregar o projeto ao cliente.', 1, 0, 2, @Agora, @Agora, @IdUsuario, @IdUsuario);
    END;
    FETCH NEXT FROM cur INTO @IdUsuario;
END;
CLOSE cur;
DEALLOCATE cur;
