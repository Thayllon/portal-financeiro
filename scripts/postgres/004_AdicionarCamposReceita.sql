ALTER TABLE Receita ADD IdParceiro UUID NULL;
ALTER TABLE Receita ADD IdCategoriaServico UUID NULL;
ALTER TABLE Receita ADD IdSubcategoriaServico UUID NULL;
ALTER TABLE Receita ADD IdCliente UUID NULL;

ALTER TABLE Receita ADD CONSTRAINT FK_Receita_Parceiro FOREIGN KEY (IdParceiro) REFERENCES Pessoa(Id);
ALTER TABLE Receita ADD CONSTRAINT FK_Receita_CategoriaServico FOREIGN KEY (IdCategoriaServico) REFERENCES CategoriaServico(Id);
ALTER TABLE Receita ADD CONSTRAINT FK_Receita_SubcategoriaServico FOREIGN KEY (IdSubcategoriaServico) REFERENCES CategoriaServico(Id);
ALTER TABLE Receita ADD CONSTRAINT FK_Receita_Cliente FOREIGN KEY (IdCliente) REFERENCES Pessoa(Id);
