-- Migração 200: remoção do módulo Pró-labore / encargo INSS.
-- Idempotente: executa apenas o que ainda existe (bancos que já rodaram 001/099).

ALTER TABLE "Despesa" DROP COLUMN IF EXISTS "IdProLaboreOrigem";
DROP TABLE IF EXISTS "ProLabore";
