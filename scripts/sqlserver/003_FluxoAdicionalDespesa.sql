-- Fluxo adicional de despesa: Cliente + Categoria de Serviço
IF COL_LENGTH('Despesa', 'IdCliente') IS NULL
    ALTER TABLE Despesa ADD IdCliente UNIQUEIDENTIFIER NULL CONSTRAINT FK_Despesa_Cliente FOREIGN KEY (IdCliente) REFERENCES Pessoa(Id);

IF OBJECT_ID('DespesaServico', 'U') IS NULL
BEGIN
    CREATE TABLE DespesaServico (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        DespesaId UNIQUEIDENTIFIER NOT NULL,
        CategoriaServicoId UNIQUEIDENTIFIER NOT NULL,
        SubcategoriaServicoId UNIQUEIDENTIFIER NULL,
        CONSTRAINT FK_DespesaServico_Despesa FOREIGN KEY (DespesaId) REFERENCES Despesa(Id) ON DELETE CASCADE,
        CONSTRAINT FK_DespesaServico_CategoriaServico FOREIGN KEY (CategoriaServicoId) REFERENCES CategoriaServico(Id),
        CONSTRAINT FK_DespesaServico_SubcategoriaServico FOREIGN KEY (SubcategoriaServicoId) REFERENCES CategoriaServico(Id)
    );
    CREATE INDEX IX_DespesaServico_DespesaId ON DespesaServico(DespesaId);
END
