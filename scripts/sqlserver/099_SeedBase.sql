-- Seed base (executado por último): usuário administrador padrão (admin@portal.com / senhasenha)
-- e categorias fiscais compartilhadas CNPJ -> DAS e INSS.
-- Idempotente: executa somente o que ainda não existe.
IF NOT EXISTS (SELECT 1 FROM Usuario WHERE Email = 'admin@portal.com')
BEGIN
    INSERT INTO Usuario (Id, Nome, Email, SenhaHash, IsAdmin, Ativo, DataCadastro, DataAlteracao)
    VALUES (NEWID(), 'Admin', 'admin@portal.com', 'nc0RKfw9YhrKHokj4xZ3AQ==.11eIHgy/7VkSsZ734otOeP/9387OU5Ka6HtuZBumDJY=', 1, 1, SYSUTCDATETIME(), SYSUTCDATETIME());
END;

DECLARE @AdminId UNIQUEIDENTIFIER = (SELECT Id FROM Usuario WHERE Email = 'admin@portal.com');

IF NOT EXISTS (SELECT 1 FROM CategoriaDespesa WHERE Nome = 'CNPJ' AND IdUsuario = @AdminId AND CategoriaPaiId IS NULL)
BEGIN
    DECLARE @CnpjId UNIQUEIDENTIFIER = NEWID();

    INSERT INTO CategoriaDespesa (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao)
    VALUES (@CnpjId, @AdminId, 'CNPJ', NULL, 1, SYSUTCDATETIME(), SYSUTCDATETIME());
END;

DECLARE @CnpjId2 UNIQUEIDENTIFIER = (SELECT Id FROM CategoriaDespesa WHERE Nome = 'CNPJ' AND IdUsuario = @AdminId AND CategoriaPaiId IS NULL);

IF @CnpjId2 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM CategoriaDespesa WHERE Nome = 'DAS' AND IdUsuario = @AdminId AND CategoriaPaiId = @CnpjId2)
BEGIN
    INSERT INTO CategoriaDespesa (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao)
    VALUES (NEWID(), @AdminId, 'DAS', @CnpjId2, 1, SYSUTCDATETIME(), SYSUTCDATETIME());
END;

IF @CnpjId2 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM CategoriaDespesa WHERE Nome = 'INSS' AND IdUsuario = @AdminId AND CategoriaPaiId = @CnpjId2)
BEGIN
    INSERT INTO CategoriaDespesa (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao)
    VALUES (NEWID(), @AdminId, 'INSS', @CnpjId2, 1, SYSUTCDATETIME(), SYSUTCDATETIME());
END;
