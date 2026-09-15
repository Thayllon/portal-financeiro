-- Migration: Nome e percentual do parceiro na tabela Parceria.

ALTER TABLE Parceria ADD COLUMN IF NOT EXISTS Nome VARCHAR(150) NOT NULL DEFAULT '';
ALTER TABLE Parceria ADD COLUMN IF NOT EXISTS PercentualParceiro NUMERIC(5,2) NOT NULL DEFAULT 0;
