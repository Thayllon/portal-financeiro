-- Portal Financeiro - Enums íntegros no banco (SQL Server, banco local).
-- 018: CHECK constraints documentando os valores válidos de cada "tipo".
--   ContaBancaria.Tipo .......... 1=PF, 2=PJ (TipoConta)
--   Pessoa.Tipo ................. 1=Cliente, 2=Parceiro (TipoPessoa)
--   PermissaoUsuario.Nivel ...... 0=Nenhum, 1=Leitura, 2=Escrita (NivelPermissao)
--   Receita/Despesa.Status ...... 1=Pendente, 2=Realizado (StatusMensal)
--   CategoriaHistorico.Tipo ..... 1=Receita, 2=Despesa, 3=Serviços (ETipoCategoria)
--   CategoriaHistorico.Acao ..... 1=Criado, 2=Editado, 3=Excluído (EAcaoCategoriaHistorico)
-- Idempotente: cria cada constraint somente se ainda não existir.

IF OBJECT_ID(N'dbo.CK_ContaBancaria_Tipo', N'C') IS NULL
BEGIN
    ALTER TABLE ContaBancaria ADD CONSTRAINT CK_ContaBancaria_Tipo CHECK (Tipo IN (1, 2));
END;

IF OBJECT_ID(N'dbo.CK_Pessoa_Tipo', N'C') IS NULL
BEGIN
    ALTER TABLE Pessoa ADD CONSTRAINT CK_Pessoa_Tipo CHECK (Tipo IN (1, 2));
END;

IF OBJECT_ID(N'dbo.CK_PermissaoUsuario_Nivel', N'C') IS NULL
BEGIN
    ALTER TABLE PermissaoUsuario ADD CONSTRAINT CK_PermissaoUsuario_Nivel CHECK (Nivel IN (0, 1, 2));
END;

IF OBJECT_ID(N'dbo.CK_Receita_Status', N'C') IS NULL
BEGIN
    ALTER TABLE Receita ADD CONSTRAINT CK_Receita_Status CHECK (Status IN (1, 2));
END;

IF OBJECT_ID(N'dbo.CK_Despesa_Status', N'C') IS NULL
BEGIN
    ALTER TABLE Despesa ADD CONSTRAINT CK_Despesa_Status CHECK (Status IN (1, 2));
END;

IF OBJECT_ID(N'dbo.CK_CategoriaHistorico_Tipo', N'C') IS NULL
BEGIN
    ALTER TABLE CategoriaHistorico ADD CONSTRAINT CK_CategoriaHistorico_Tipo CHECK (TipoCategoria IN (1, 2, 3));
END;

IF OBJECT_ID(N'dbo.CK_CategoriaHistorico_Acao', N'C') IS NULL
BEGIN
    ALTER TABLE CategoriaHistorico ADD CONSTRAINT CK_CategoriaHistorico_Acao CHECK (Acao IN (1, 2, 3));
END;
