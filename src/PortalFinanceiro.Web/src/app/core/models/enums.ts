// Enums espelho do backend (PortalFinanceiro.Core.Domain.Enums / Entities).
// Wire format: strings (JsonStringEnumConverter) — ex.: status "Pendente", tipo "Pf".
// Banco: INT com CHECK (scripts 018_CheckEnums / 103_CheckEnums):
//   Status 1=Pendente 2=Realizado · Pessoa 1=Cliente 2=Parceiro · Conta 1=Pf 2=Pj.
export enum StatusLancamento {
  Pendente = 'Pendente',
  Realizado = 'Realizado',
}

export enum TipoPessoa {
  Cliente = 'Cliente',
  Parceiro = 'Parceiro',
}

export enum TipoConta {
  Pf = 'Pf',
  Pj = 'Pj',
}
