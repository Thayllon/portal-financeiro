const MS_POR_DIA = 24 * 60 * 60 * 1000;

export function diasEmAberto(dataCadastro: string, dataEncerramento?: string): number {
  const inicio = new Date(dataCadastro).getTime();
  const fim = dataEncerramento ? new Date(dataEncerramento).getTime() : Date.now();
  return Math.max(0, Math.floor((fim - inicio) / MS_POR_DIA));
}

export function duracaoDias(dataInicio?: string, dataConclusao?: string): number | null {
  if (!dataInicio) return null;
  const inicio = new Date(dataInicio).getTime();
  const fim = dataConclusao ? new Date(dataConclusao).getTime() : Date.now();
  return Math.max(0, Math.floor((fim - inicio) / MS_POR_DIA));
}

export function formatarDuracao(dias: number | null): string {
  if (dias === null) return '—';
  if (dias === 0) return 'menos de 1 dia';
  return dias === 1 ? '1 dia' : `${dias} dias`;
}
