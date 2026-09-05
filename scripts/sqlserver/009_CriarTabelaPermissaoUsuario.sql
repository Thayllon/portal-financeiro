-- Tabela de permissões por módulo por usuário
CREATE TABLE PermissaoUsuario (
    Id          UNIQUEIDENTIFIER PRIMARY KEY,
    UsuarioId   UNIQUEIDENTIFIER NOT NULL,
    Modulo      NVARCHAR(50)     NOT NULL,
    Nivel       INT              NOT NULL DEFAULT 0,
    CONSTRAINT FK_PermissaoUsuario_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuario(Id),
    CONSTRAINT UQ_PermissaoUsuario_UsuarioModulo UNIQUE (UsuarioId, Modulo)
);
