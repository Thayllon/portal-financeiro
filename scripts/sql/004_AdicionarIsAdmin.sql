-- Adiciona a flag de administrador ao usuário.
ALTER TABLE Usuario ADD IsAdmin BIT NOT NULL DEFAULT 0;
