IF COL_LENGTH('dbo.ContaBancaria', 'EhPadrao') IS NULL
BEGIN
    ALTER TABLE dbo.ContaBancaria ADD EhPadrao BIT NOT NULL DEFAULT 0;
END