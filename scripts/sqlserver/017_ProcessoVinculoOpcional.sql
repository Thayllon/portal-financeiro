-- Portal Financeiro - Processo com vínculo opcional (SQL Server, banco local).
-- 017: processo pode ser criado sem vínculo; no máximo um entre Parceria e Contrato.
-- Substitui a regra anterior (obrigatório e exclusivo: Parceria XOR Contrato).

IF OBJECT_ID(N'dbo.CK_Processo_Vinculo_Exclusivo', N'C') IS NOT NULL
BEGIN
    ALTER TABLE Processo DROP CONSTRAINT CK_Processo_Vinculo_Exclusivo;
END;

IF OBJECT_ID(N'dbo.CK_Processo_Vinculo_Unico', N'C') IS NULL
BEGIN
    ALTER TABLE Processo ADD CONSTRAINT CK_Processo_Vinculo_Unico
        CHECK (NOT (IdParceria IS NOT NULL AND IdContrato IS NOT NULL));
END;