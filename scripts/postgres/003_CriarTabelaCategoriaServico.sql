-- Migração: Cria tabela CategoriaServico (PostgreSQL)
-- Requer: 001_CriarTabelas.sql

CREATE TABLE IF NOT EXISTS CategoriaServico (
    Id UUID PRIMARY KEY,
    IdUsuario UUID NOT NULL,
    Nome VARCHAR(100) NOT NULL,
    CategoriaPaiId UUID NULL,
    Ativo BOOLEAN NOT NULL DEFAULT TRUE,
    DataCadastro TIMESTAMP NOT NULL,
    DataAlteracao TIMESTAMP NOT NULL,
    CONSTRAINT FK_CategoriaServico_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id),
    CONSTRAINT FK_CategoriaServico_Pai FOREIGN KEY (CategoriaPaiId) REFERENCES CategoriaServico(Id)
);
