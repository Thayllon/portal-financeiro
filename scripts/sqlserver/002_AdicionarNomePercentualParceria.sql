-- Migration: Nome e percentual do parceiro na tabela Parceria.
-- Idempotente: pode ser re-executado com segurança.

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Parceria') AND name = 'Nome')
    ALTER TABLE Parceria ADD Nome NVARCHAR(150) NOT NULL DEFAULT '';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Parceria') AND name = 'PercentualParceiro')
    ALTER TABLE Parceria ADD PercentualParceiro DECIMAL(5,2) NOT NULL DEFAULT 0;
