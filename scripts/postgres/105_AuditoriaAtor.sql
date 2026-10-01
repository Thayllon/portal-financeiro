-- 105_AuditoriaAtor.sql — PostgreSQL (Neon)
-- Registra quem criou e quem alterou por último (criadopor / alteradopor).
-- Nuláveis: dados anteriores à auditoria permanecem NULL (gravação daqui para frente).
-- Sem FK para usuario de propósito: o histórico sobrevive mesmo se o usuário for excluído.
-- IDEMPOTENTE. Execute no Neon (SQL Editor ou psql) após atualizar o código.

DO $$ DECLARE
  t TEXT;
BEGIN
  FOREACH t IN ARRAY ARRAY['contabancaria','pessoa','parceria','contrato','categoriareceita','categoriadespesa','categoriaservico','regrareceita','regradespesa','receita','despesa','processo','processoetapa']
  LOOP
    -- tabelas opcionais (processo/processoetapa podem não existir ainda no banco)
    IF to_regclass(format('%I', t)) IS NULL THEN CONTINUE; END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = t AND column_name = 'criadopor') THEN
      EXECUTE format('ALTER TABLE %I ADD COLUMN criadopor UUID NULL', t);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = t AND column_name = 'alteradopor') THEN
      EXECUTE format('ALTER TABLE %I ADD COLUMN alteradopor UUID NULL', t);
    END IF;
  END LOOP;
END $$;
